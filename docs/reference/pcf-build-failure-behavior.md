# PCF build failures and Dataverse solution packaging in Visual Studio

A failed solution build does not make any ZIP produced during that build safe to
use. With Visual Studio's default build policy, a failed `.pcfproj` can be followed
by a successful `.cdsproj` packaging step that consumes the previous PCF bundle.
An ordinary `ProjectReference` and the correct project dependency ordering do not
provide a success prerequisite for the dependent project's build.

## Investigation result

Reproduced on 2026-11-11 in a disposable copy of a real PCF/Dataverse solution:

| Component | Version |
| --- | --- |
| Visual Studio | 18 Insiders, Experimental instance |
| Full-framework MSBuild | 18.13.0.52701 |
| Installed CPS VS implementation | 18.13.43.11567 |
| Microsoft.PowerApps.MSBuild.Pcf | 1.52.1 |
| Microsoft.PowerApps.MSBuild.Solution | 2.14.1 |
| Node.js / npm | 22.15.0 / 11.4.2 |

The fixture used the real TypeScript/ESLint/webpack PCF pipeline and Microsoft's
solution packaging targets. It did not reference XrmTools.Build. The original
checkout was read and copied, not edited or built. No VS build settings were changed.

The cause is the combination of **standard VS scheduling after a project failure**
and **packaging targets that consume reference output paths, not build-success
receipts**. It is not a lost PCF failure result in Xrm Tools.

### Build-result and packaging path

1. `pcf-scripts` reports the introduced `const broken = ;` syntax error during
   ESLint. The PCF `Exec` task fails with `MSB3077`, before bundling, leaving the
   last successful bundle on disk.
2. The stock CPS default build provider returns the MSBuild result. Its
   `BuildableProjectConfig` passes failure to `IVsBuildStatusCallback.BuildEnd(0)`.
   Xrm Tools registers the project types and CPS providers; it does not replace
   that build provider or the solution build scheduler.
3. The VS automation build events confirm receipt of the failure **before** the
   dependent project starts. For example, the selected-project Build command
   produced this sequence (UTC):

   ```text
   12:23:14.816 COMMAND Build.BuildSelection
   12:23:24.588 PROJECT DONE PcfControl\PcfControl.pcfproj Success=False
   12:23:24.590 PROJECT BEGIN DataverseSolution\DataverseSolution.cdsproj
   12:23:25.092 PROJECT DONE DataverseSolution\DataverseSolution.cdsproj Success=True
   LAST BUILD INFO=1
   ```

4. VS honors the dependency's ordering but still starts the dependent project.
   A separate control solution containing only ordinary SDK-style C# projects
   behaved the same way: the upstream C# compilation failed with `CS1525`, then
   a changed dependent compiled successfully against the old upstream assembly.
   Its custom build hook also ran. This is not unique to the Xrm Tools project types.
5. In `Microsoft.Common.CurrentVersion.targets`, `ResolveProjectReferences` uses
   `GetTargetPath` for IDE builds instead of building the reference again. The
   child-build path is conditioned on `BuildingInsideVisualStudio != true`.
   Diagnostics showed `BuildingInsideVisualStudio=true` in VS; notably,
   `BuildProjectReferences=true` was also present. The latter alone does not
   select the child-build path while inside VS.
6. Microsoft's solution targets run `ProcessCdsProjectReferencesOutputs` before
   `PowerAppsPackage`. It calls `GetProjectOutputPath` on references and copies
   the output it finds. It does not retrieve the earlier VS build's success
   status. With the old PCF output still present, Solution Packager can write a
   new ZIP successfully.

The solution-build failure status is therefore correct. The missing policy is
"do not start this dependent after a failed dependency", not "discover the
reference" or "report the PCF error".

## Validation

VS solution builds were driven through `SolutionBuild.Build`. The project-context
path was exercised both through `SolutionBuild.BuildProject` and by selecting the
CDS project in Solution Explorer and executing `Build.BuildSelection`. Automation
recorded build scope, project completion status/order, and the Build output pane;
it did not simulate a mouse click on the context menu.

| Check | Observed result |
| --- | --- |
| Initial successful VS build | PCF built and solution packaged successfully. |
| Build Solution with old valid bundle and new PCF syntax error | Overall failure; CDS still invoked Solution Packager. ZIP timestamp and SHA-256 both changed. **The requested skip behavior is not provided by default VS.** |
| Selected CDS project Build with that error | Same failure and subsequent packaging, including through `Build.BuildSelection`. The new ZIP's embedded bundle matched the previous valid PCF bundle byte for byte. |
| Repair PCF, change revision, Build Solution in the same VS session | Success, exactly one `pcf-scripts build` and one package invocation. Embedded bundle contained the new revision and matched generated output byte for byte. |
| Repeat failure, repair with another revision, selected-project Build in the same session | Same successful recovery, also using `Build.BuildSelection`; one PCF build and one packaging invocation. No reload or clean. |
| Build again without changing inputs | Success; no new PCF npm build. The SDK's incremental behavior remained intact. |
| Direct full-framework MSBuild of CDS with broken PCF | Exit code 1; no Solution Packager invocation. Existing ZIP timestamp and SHA-256 remained unchanged. |
| Repair and directly build CDS with a new revision | Success with one PCF build and one packaging invocation; embedded bundle contained the revision and matched generated output byte for byte. |
| Native C# control: failed upstream plus changed dependent | VS started and successfully compiled the dependent against the previous upstream assembly; custom hook ran. |

For the VS solution recovery, embedded/generated bundle SHA-256 was
`361C2B168C2B3AACF9AC6028CD3272778DBE051441C455F793AF5116045399DD`
(revision `xrmtools-vs-solution-recovery-20261111-1219`). For selected-project
`Build.BuildSelection` recovery it was
`6CD094AD5E61DBFFD36810C3672DAD5968084132347FD57D32B7062DCE69553B`
(revision `xrmtools-vs-build-selection-recovery-20261111-1223`). In both cases,
`Controls/xt_XrmToolsTests.SmokeControl/bundle.js` was compared with the generated
PCF bundle as bytes, not just searched for a string.

## Practical remedies

### Verified, without changing VS settings or consumer targets

For a packaging build that must stop after a failed reference, use one
full-framework MSBuild invocation from Developer PowerShell, with Node/npm on PATH:

```powershell
MSBuild.exe .\DataverseSolution\DataverseSolution.cdsproj /t:Build /p:Configuration=Debug /m /bl:dataverse-build.binlog
if ($LASTEXITCODE -ne 0) { throw 'Dataverse solution build failed; do not use its ZIP.' }
```

This uses the normal command-line project-reference build path. A failed PCF child
build prevents packaging. There is no extra PCF build added to the CDS targets,
no cleanup of previous outputs, and no global setting change. Restore packages
normally when required. An earlier successful ZIP may remain on disk; publish or
import artifacts only after a successful invocation.

### Optional VS policies (not changed or validated in this investigation)

- VS 2026 18.11+ documents **Tools > Options > Projects and Solutions > Build and
  Run > Stop solution build after first failure**. It is opt-in. It stops the
  whole solution build, including independent work, so it is broader than
  dependency-only skipping. Verify both build scopes on the installed VS version.
- The separate **Fail Fast** extension advertises skipping failed projects'
  dependants while continuing independent projects. Its behavior with these PCF/CDS
  projects was not tested here; it is not an Xrm Tools dependency or an automatic
  installation recommendation.

No Xrm Tools build-result guard or XrmTools.Build workaround was added. The
requested VS packaging-skip behavior requires an explicit scheduler policy; it
has not been silently enabled by this investigation. The separate CDS NuGet UI
issue is outside this investigation.

## References

- [MSBuild project-reference handling](https://github.com/dotnet/msbuild/blob/main/src/Tasks/Microsoft.Common.CurrentVersion.targets), `ResolveProjectReferences`.
- [Configure Visual Studio build and run options](https://learn.microsoft.com/en-us/visualstudio/ide/configure-build-run-options?view=visualstudio), "Stop solution builds after the first failure".
- [VS stop-on-failure policy and its solution-wide scope](https://devblogs.microsoft.com/visualstudio/stop-waiting-on-a-failed-build/).
- [Fail Fast extension behavior](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.FailFast).

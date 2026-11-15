# PCF projects in Visual Studio

Xrm Tools opens standard PAC-generated `.pcfproj` projects without converting their project files. Microsoft PCF targets remain responsible for normal builds. Node.js, npm and the Power Platform CLI must be installed separately; start Visual Studio with Node.js and npm on its `PATH`.

## Create a control

From the solution or a solution folder, choose **Add > Power Apps Component Project...**. The Visual Studio-themed wizard collects the project name and location, control name, namespace, field/dataset template and optional React framework. It runs `pac pcf init` and adds the generated project to the selected solution location. Creation does not require Dataverse authentication.

**Install npm dependencies** is an explicit opt-in, unchecked by default. On failure or cancellation, generated files remain available for inspection; an unsuccessful creation is not added to the solution. See the first-build instructions below before choosing Rebuild or starting the harness.

PAC projects using `RestoreProjectStyle=PackageReference` participate in CPS NuGet restore so their Microsoft build targets can be restored in Visual Studio. Existing `packages.config` projects are not automatically migrated.

## First Build and Rebuild

Install Node.js and npm separately, choosing a version compatible with the project's `.node-version`, `.nvmrc`, and `package.json` engine requirements where present. [Microsoft's PCF setup guidance](https://learn.microsoft.com/power-apps/developer/component-framework/create-custom-controls-using-pcf) recommends Node LTS. PAC is required for creation and deployment, but not for the Microsoft MSBuild targets' npm steps.

For Microsoft.PowerApps.MSBuild.Pcf 1.52.1, a normal **Build** automatically runs `npm install` when its incremental install target determines it is needed; `PcfEnableAutoNpmInstall` and `PcfAlwaysNpmInstall` control that behavior. It may update `package-lock.json`; it is not equivalent to `npm ci`. Respect any project-specific npm configuration. To start the harness, dependencies must already be installed.

On a fresh checkout, use **Build before Rebuild** if the project permits automatic installation, or explicitly install dependencies from the directory containing the PCF `package.json`: use `npm ci` when a compatible lockfile exists, otherwise follow the project's instructions (typically `npm install`). This is necessary even when automatic install is enabled: Microsoft's Rebuild runs Clean first, and its `PcfClean` runs `npm run clean` before its automatic install target. If the clean script uses `pcf-scripts` and dependencies are absent, it fails with `'pcf-scripts' is not recognized` / `MSB3073`. Do not delete `node_modules` to fix this. A project with a custom clean script may have different prerequisites.

Microsoft's targets remain in charge: Xrm Tools does not automatically install Node, override npm scripts, or prepare dependencies before Clean. Explicit install and the wizard's opt-in are developer actions. `PcfAlwaysNpmInstall`, `PcfEnableAutoNpmInstall`, and custom scripts keep their normal Microsoft behavior. NuGet restore is separate from npm dependency installation.

## Troubleshoot Node and npm discovery

If Build reports `'npm' is not recognized` and `MSB3073` with exit code `9009`, the Microsoft PCF targets cannot find npm in the build process environment. Other nonzero exits can mean Node/npm is present but unusable; read the preceding output. The project path on the MSBuild error identifies the affected project. Creating a project with **Install npm dependencies** unchecked does not exercise npm and can still succeed. Changing debugger settings will not fix a build.

Visual Studio and reused MSBuild worker processes inherit their environment; a successful command in a different terminal does not update them. For a normal Windows launch, make the intended Node and npm available on the environment PATH before starting VS, then restart VS after PATH changes. Alternatively, select an existing installation **for this launch only** in a Developer PowerShell, replacing the example directory with your actual installation containing `node.exe` and the npm launcher:

```powershell
$env:Path = 'C:\path\to\your\node-installation;' + $env:Path
node.exe --version
npm.cmd --version
devenv.exe
```

For fnm, initialize it explicitly in the shell, select the project's configured version, verify it, and launch VS from that same shell. For example, when the project intentionally uses the fnm `default` alias:

```powershell
fnm env --use-on-cd --shell powershell | Out-String | Invoke-Expression
fnm use default
node.exe --version
npm.cmd --version
devenv.exe
```

The first command evaluates code produced by the installed fnm executable in your shell: use it only if you trust that installation. To honor a project version file instead of the default alias, start in the project directory and use `fnm use` without an explicit version; review [fnm's options](https://github.com/Schniz/fnm/blob/master/docs/commands.md) for directory search and `engines.node` resolution. Install the required version yourself if it is missing. Do not copy a temporary `fnm_multishells` path into project files. With other version managers, follow their documented shell activation and launch VS from that shell; automatic integration with them has not been implemented or tested here.

Close existing VS instances before launching so the solution opens in the intended environment; for Experimental use `devenv.exe /RootSuffix Exp` and also close the main instance if it launches Experimental. Restart after changing or correcting the toolchain: an already-running VS or reused MSBuild worker may retain the old environment. The shell checks are useful but do not prove what the build sees; inspect Build output or use the optional project-local diagnostic below. These steps do not change machine-wide PATH or PowerShell profiles.

Selection is process-wide for these build targets, not per-project: the inherited PATH determines which `node` and bare `npm` MSBuild executes. Xrm Tools does not resolve `.node-version`, `.nvmrc`, or `engines.node` for builds, override a project's configuration, or switch Node for individual projects. Projects requiring different versions need separate VS launches/environments or their own independently tested build tooling. The local harness has its own discovery: it resolves `node.exe` and either `npm.exe` or `npm.cmd` from the VS process PATH, and reports the selected npm launcher in the Xrm Tools output pane. The Web Resources SDK also invokes bare `npm` through MSBuild `Exec`.

### Optional build-environment diagnostic

For clearer errors on the npm steps, a team can opt in to the following MSBuild target. Add it inside an existing `Directory.Build.targets` that applies to the PCF project, or import a file containing it from the `.pcfproj`. Do not create a nearer `Directory.Build.targets` without accounting for an existing parent file: [MSBuild stops its upward search at the first one](https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory). This changes project build configuration and applies wherever imported, including command-line builds; it is not installed automatically by Xrm Tools.

```xml
<Target Name="XrmToolsCheckPcfNode" BeforeTargets="PcfClean;PcfBuild;PcfRebuild;RestoreNPM"
        Condition="'$(MSBuildProjectExtension)' == '.pcfproj' and '$(DesignTimeBuild)' != 'true'">
  <Exec Command="node --version" IgnoreExitCode="true" StandardOutputImportance="low" StandardErrorImportance="low">
    <Output TaskParameter="ExitCode" PropertyName="_XrmToolsPcfNodeExitCode" />
  </Exec>
  <Error Condition="'$(_XrmToolsPcfNodeExitCode)' != '0'"
         Text="PCF project '$(MSBuildProjectFullPath)': Node.js is unavailable or unusable in the MSBuild environment. Install or activate the project's Node version, then start Visual Studio from that environment." />
  <Exec Command="npm --version" IgnoreExitCode="true" StandardOutputImportance="low" StandardErrorImportance="low">
    <Output TaskParameter="ExitCode" PropertyName="_XrmToolsPcfNpmExitCode" />
  </Exec>
  <Error Condition="'$(_XrmToolsPcfNpmExitCode)' != '0'"
         Text="PCF project '$(MSBuildProjectFullPath)': npm is unavailable or unusable in the MSBuild environment. Install or activate npm for the project's Node version, then start Visual Studio from that environment." />
</Target>
```

The checks run in the same MSBuild environment as Microsoft's npm commands and fail before those named targets, without changing PATH. They add two process launches when an npm target actually runs; an unchanged incremental Build can skip those targets and therefore the checks. They do not verify engine compatibility, guarantee that node and npm belong to the same installation, or check dependencies. Custom targets that invoke npm independently are not covered. The hook was tested with command-line MSBuild, not verified in Visual Studio; version changes to the Microsoft targets may require review.

### First-build investigation evidence

On Windows, a disposable project generated with PAC 2.14.1 used Microsoft.PowerApps.MSBuild.Pcf 1.52.1, `pcf-scripts` 1.51.1, full-framework MSBuild 18.13.0.52701, fnm 1.38.1, Node 22.15.0 and npm 11.4.2. The supplied smoke-test checkout was inspected read-only. Its PCF directories had no `.node-version` or `.nvmrc`, and the inspected `PcfControl` package did not declare `engines.node`.

1. With fnm initialized as above and no `node_modules`, `MSBuild <project>.pcfproj -restore -t:Rebuild -m:1 -nr:false` failed at Microsoft's `PcfClean` because `pcf-scripts` was unavailable.
2. `MSBuild <project>.pcfproj -t:Build -m:1 -nr:false` installed dependencies and completed real manifest validation, TypeScript compilation and bundling. Rebuild then succeeded; a subsequent unchanged Build skipped both npm install and npm build.
3. With the optional target imported, running `MSBuild <project>.pcfproj -t:PcfClean -m:1 -nr:false` in a process with only Windows system directories on PATH produced the project-specific Node diagnostic. A PATH containing a copy of `node.exe` but no npm produced the npm diagnostic. With the fnm environment restored, Build succeeded; the next unchanged Build skipped install and compilation. In a second fresh PAC project with fnm on PATH but not initialized, `-restore -t:Build` also produced the Node diagnostic before npm install.
4. Separately, with only Windows system directories on PATH and fnm 1.38.1 invoked by its explicit executable path, `fnm exec --using .node-version node --version` and `fnm exec --using .node-version npm.cmd --version` ran Node 22.15.0 and npm 11.4.2 for a disposable version file. An uninstalled `--using 99.0.0` failed rather than installing Node. Concurrent isolated fnm child processes selected Node 22.15.0 and 24.21.0 respectively. These were not Visual Studio builds or automatic Xrm Tools integration.
5. A disposable MSBuild project set the global `Path` property to the fnm Node installation while the MSBuild process PATH contained only Windows system directories. An `Exec` of `node --version` still failed with exit code 9009: setting the MSBuild property alone did not set the child process environment.
6. In another fresh PAC project with no Node or npm on the parent PATH, running `fnm exec --using default <full-path-to-MSBuild.exe> <project>.pcfproj -restore -t:Build -m:1 -nr:false` installed dependencies and completed Microsoft's real PCF build; the next unchanged Build skipped install and compilation. This proves an isolated **whole MSBuild process** can inherit fnm's environment; it does not prove that VS's coordinated build can use that process.
7. In a separate fresh project, a disposable `Directory.Build.targets` hooked `BeforeRebuild` and called Microsoft's `RestoreNPM` when `node_modules/pcf-scripts/package.json` was absent and auto-install was enabled. Invoking Rebuild through the same fnm/MSBuild command installed once, then completed Microsoft's clean, build and rebuild scripts. Microsoft's normal incremental install target skipped the second install in this default configuration. This hook did not serialize competing builds, cover forced-install behavior, or test cancellation; it was not installed into Xrm Tools.

These are command-line observations, not proof of VS build-process propagation. Normal Windows launch, fnm absent from VS's environment, same-session recovery, and multiple project versions have not been verified for this diagnostic. The extension does not inject it automatically: Microsoft's targets invoke bare npm in their own `Exec` tasks, and mutating a shared or reused MSBuild process environment to select a version would risk leaking one project's selection into another. The [`Exec` task's `EnvironmentVariables` parameter](https://learn.microsoft.com/visualstudio/msbuild/exec-task) applies to that task's child process; Microsoft's shipped PCF `Exec` tasks do not expose an equivalent project property. Running `fnm exec` for a separate preparation process therefore does not select Node for Microsoft's later tasks. [MSBuild target-ordering guidance](https://learn.microsoft.com/visualstudio/msbuild/target-build-order) and the installed Microsoft PCF targets describe the hooks used above; the optional target does not replace them. The diagnostic and manual setup above are a fallback, not the desired automatic first-build experience.

The VS build entry uses the solution build manager; a standalone fnm-wrapped MSBuild is not the coordinated VS build. CPS's `IBuildProvider` is a replacement build implementation, not an established delegating environment wrapper; its `IBuildProject.BuildAsync` properties are MSBuild properties rather than process environment variables, and its cancellation token is documented as ignored in VS in favor of solution-build cancellation. A custom build implementation would need to preserve VS coordination, logging, configuration, cancellation and dependencies; launching a second MSBuild alone does not establish those guarantees. Microsoft's [custom IDE build guidance](https://learn.microsoft.com/visualstudio/msbuild/starting-a-build-from-within-the-ide) requires `IVsBuildManagerAccessor` for coordinated custom builds.

The pre-Rebuild hook has a separate injection and concurrency problem. The CPS global-properties provider can set `CustomAfterMicrosoftCommonTargets`, but an MSBuild global property overrides a value supplied by the project: using it unconditionally could replace a team's existing custom import. `CallTarget` alone does not lock the project against another build's npm install, nor does it ensure resource release and safe retry after cancellation. A production solution needs a composable import mechanism and a cancellation-aware cross-process lock around **all** competing installs, including the normal Microsoft install path; this sample provides neither. [Overriding the MSBuild `Exec` task](https://learn.microsoft.com/visualstudio/msbuild/usingtask-element-msbuild) would affect the project's other Exec tasks too and compete with any existing override; replacing the Microsoft targets would need to track their future changes and customizations. Neither alternative has been validated as preserving project behavior. An upstream PCF target extension exposing per-invocation npm environment and coordinated preparation, or a separately engineered replacement build integration with equivalent behavior, would provide a boundary to implement and test. Neither has been established here.

## Test locally

Set the PCF project as the startup project:

- **F5** starts the project's existing `npm start -- watch` script, waits for the harness's actual loopback address, and launches Microsoft Edge through Visual Studio's JavaScript browser debugger. This requires the Visual Studio JavaScript browser-debugging components and Edge. Original TypeScript breakpoints additionally require the project's bundler to emit accurate source maps; see the limitation below.
- **Ctrl+F5** starts the same harness without attaching a debugger. The Microsoft harness opens the default browser.
- The project context-menu **Start Local Test Harness** command also starts without debugging.
- **Shift+F5** ends a debugging session and stops its owned harness process tree. Use **Stop Local Test Harness** for a non-debugging session. Unloading the project also stops its harness; another project's harness is unaffected.

A project can own only one harness session at a time. Stop its current session before changing between debugging and non-debugging. Output and actionable startup errors appear in the Xrm Tools output pane. The address is read from the harness rather than assuming port 8181, allowing another control to use a different port.

The Microsoft harness itself may also open a default-browser tab during F5; use the Edge window launched by Visual Studio for breakpoints. Xrm Tools does not patch the harness's browser-launch implementation. The debugger uses an isolated Edge profile rather than your regular browser profile.

Watch mode rebuilds control source and resources. Structural manifest changes may require restarting the harness. The harness is not Dataverse: Web API and some model-driven/dataset behaviors require testing in a real app. See [Microsoft's local debugging guidance](https://learn.microsoft.com/power-apps/developer/component-framework/debugging-custom-controls).

### TypeScript source-map limitation

The tested PAC-generated project with `pcf-scripts` 1.51.1 uses webpack's `eval` development output: its runtime script is named `index.ts` but contains transpiled JavaScript without a TypeScript source map. A real Visual Studio browser-debugger attachment succeeds, but breakpoints in the original TypeScript can remain unbound. Enabling `sourceMaps` in a debugger cannot manufacture missing mappings; Xrm Tools does not disable source-content validation or silently rewrite the project to hide this limitation.

For projects that already emit accurate source maps, the debugger uses those maps. A disposable comparison fixture can enable TypeScript `compilerOptions.sourceMap`, `pcfAllowCustomWebpack` in `featureconfig.json`, and webpack `devtool: "inline-source-map"` to verify this boundary. This changes PCF tooling configuration and uses its custom-webpack feature flag; it is not automatically applied or presented as a supported Microsoft production configuration. With stock tooling, use the browser's developer tools and generated runtime source as described in Microsoft's guidance above.

## Push to a development environment

On a PCF project, choose **Push to Environment...**. The dialog displays the active Xrm Tools environment and checks loaded `.cdsproj` project references. If exactly one distinct solution unique name is referenced, Xrm Tools reads it from that project's `Other\Solution.xml` and verifies it against the active environment's unmanaged-solution list; the dialog shows the inferred destination for confirmation instead of asking you to choose it. If multiple distinct solutions reference the control, choose among those related solutions. **Change...** opens the full destination choices. If the referenced solution is missing, managed, or cannot be verified, Xrm Tools does not guess another destination: it explains the issue and requires an explicit choice. With no referencing solution project, choose either **Use publisher prefix** and enter a prefix, or **Use an existing solution** and select an unmanaged solution from the active environment. The solution dropdown loads only when selected, shows friendly and unique names, and passes the selected unique name to PAC. **Refresh** retries loading after errors or an empty result. Loading can be canceled; the prefix route does not require the solution list. PAC does not accept both options together. Incremental push is optional. Inputs are validated before authentication. The selected environment is passed explicitly to `pac pcf push` using the existing PAC authentication flow. If the active environment changes while the dialog is open, close it and start again rather than silently deploying somewhere else.

The dialog shows authentication guidance and live PAC progress, and activates the Xrm Tools output pane for full details. If a browser sign-in page opens, complete authentication there. Push and input fields are disabled during destination discovery, solution loading, and deployment; **Cancel** remains available. Cancel, Escape, or closing the window requests cancellation; the dialog stays open until the local operation finishes stopping. Canceling destination discovery or solution loading does not start deployment. Cancellation cannot roll back Dataverse changes already submitted. During an operation, the shared Shell themed progress control replaces the separator above the footer buttons without moving the buttons or adding a progress row. The original themed separator returns when the operation completes, fails, or is canceled. The dialog fits its content vertically without reserving an empty progress area; status and errors wrap in a bounded scroll area while the width stays fixed.

Push is a development deployment, not a production ALM replacement. Review the environment before confirming. The extension does not push a control automatically when creating, building or debugging it.

### Fluent platform-library import error

If Dataverse rejects a React control with `platform library fluent_9_68_0 with version 9.68.0 is not supported by the platform`, check the control's source manifest. Microsoft documents an allowed Fluent 9 manifest version range of `9.4.0` through `9.46.2`, even though the runtime currently loads `9.68.0`. Declaring the runtime version is not equivalent to declaring a supported manifest version. This exact PAC React-template problem is also reported in [microsoft/powerplatform-build-tools#1265](https://github.com/microsoft/powerplatform-build-tools/issues/1265).

Use `<platform-library name="Fluent" version="9.46.2" />` in the control's source manifest, build, and push again. For a consistent supported build baseline, use `@fluentui/react-components` version `9.46.2` in the component's npm dependencies and update its lockfile with npm; first check that the component does not depend on newer Fluent APIs. If the control does not use Fluent at all, Microsoft allows removing its Fluent platform-library declaration instead. Do not edit generated manifests under the output directory. Xrm Tools reports this server-side validation failure; it does not rewrite component dependencies or platform-library declarations. See [Microsoft's supported platform libraries list](https://learn.microsoft.com/power-apps/developer/component-framework/react-controls-platform-libraries#supported-platform-libraries-list).

## Versioning

Edit the `version` attribute in `ControlManifest.Input.xml` using the normal editor. Version automation is intentionally not part of this round.

## Validation and remaining scope

Validated in an isolated Visual Studio 18 Insiders `18.13.12409.143` instance with MSBuild `18.13.0.52701`, PAC `2.14.1`, Node `22.15.0`, npm `11.4.2` and `pcf-scripts` `1.51.1`:

- Real F5 browser-debugger attachment, including the stock tooling's unbound original-TypeScript breakpoint limitation.
- In the source-map comparison fixture described above, the debugger verified the breakpoint at the original `index.ts` line 25, stopped with reason `breakpoint`, accepted Step Over, and stopped with reason `step`.
- Shift+F5 terminated the owned harness tree. Ctrl+F5 served the harness with the debugger still in design mode; the context-menu Start/Stop commands also worked. Watch mode served a new unique control revision without restarting the harness.
- The Add new Power Apps Component wizard generated and added a fresh PCF project. With npm install unchecked, no `node_modules` was created. VS automatically produced the PackageReference restore assets and imports for the original Microsoft packages.
- Additional npm-launcher validation used the Windows persistent PATH with an nvm-managed Node `22.15.0` and npm `10.9.2` executable shim, without fnm shell activation. F5 built the project, served the harness with HTTP 200, and entered the browser debugger. Regression tests also executed a batch launcher from a path containing spaces, an ampersand, and literal percent signs.
- Push-dialog regression tests show the compiled WPF dialog with simulated authentication and PAC execution, verify compact idle layout, busy-only themed progress, theme-resource updates and cancellation, and confirm a 4 KB error wraps and scrolls with bounded vertical growth and no width expansion. An isolated VS local-scaffold smoke test also verified the shared output helper makes the Output window visible and selects the logger's Xrm Tools pane. No live Dataverse deployment was performed during these checks.

# PCF projects in Visual Studio

Xrm Tools opens standard PAC-generated `.pcfproj` projects without converting their project files. Microsoft PCF targets remain responsible for normal builds. Node.js, npm and the Power Platform CLI must be installed separately; start Visual Studio with Node.js and npm on its `PATH`.

## Create a control

From the solution or a solution folder, choose **Add > Power Apps Component Project...**. The Visual Studio-themed wizard collects the project name and location, control name, namespace, field/dataset template and optional React framework. It runs `pac pcf init` and adds the generated project to the selected solution location. Creation does not require Dataverse authentication.

**Install npm dependencies** is an explicit opt-in, unchecked by default. Otherwise run `npm install` in the generated project directory before building or starting the harness. On failure or cancellation, generated files remain available for inspection; an unsuccessful creation is not added to the solution.

PAC projects using `RestoreProjectStyle=PackageReference` participate in CPS NuGet restore so their Microsoft build targets can be restored in Visual Studio. Existing `packages.config` projects are not automatically migrated.

## Troubleshoot npm discovery

If Build reports `'npm' is not recognized` and `MSB3073` with exit code `9009`, the Microsoft PCF targets cannot find npm in Visual Studio's inherited process environment. This happens before the local harness starts; changing debugger settings will not fix it. Creating a project with **Install npm dependencies** unchecked does not exercise npm and can still succeed.

Activate your Node installation in a Developer PowerShell (including any version-manager initialization), then check and launch Visual Studio from that same shell:

```powershell
node.exe --version
npm.cmd --version
devenv.exe /RootSuffix Exp
```

Close the existing Experimental instance first so the solution opens in the newly launched process. A working npm command in a different terminal does not update an already-running Visual Studio process. For normal Visual Studio, omit `/RootSuffix Exp`. This requires no project-file changes or machine-wide PATH changes; other developers and CI also need their own working Node/npm environment.

After installing or switching a Windows PATH-based toolchain, restart all Visual Studio instances, including the main instance used to launch Experimental. An Experimental instance launched from an older VS process can still inherit its old PATH. The harness resolves `node.exe` and either `npm.exe` (including version-manager shims) or `npm.cmd` from the process PATH; it does not require npm's JavaScript implementation to be beside Node. The selected npm launcher is reported in the Xrm Tools output pane. These checks do not change the ordinary Microsoft PCF build targets or update the global environment.

The Web Resources SDK does not supply a separate npm runtime: it also invokes bare `npm` through MSBuild `Exec`. In a side-by-side Visual Studio test, both Web Resources and PCF failed with exit code `9009` without npm on the inherited PATH; both built after launching VS with the existing Node/npm installation on that PATH. Xrm Tools does not automatically select Visual Studio's internal Node installation or alter the global environment.

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

# PCF projects in Visual Studio

Xrm Tools opens standard PAC-generated `.pcfproj` projects without converting their project files. Microsoft's PCF targets and the project's own `pcf-scripts` remain responsible for builds. Xrm Tools finds Node.js and npm for those builds, installs a new project's dependencies, and tells you in Visual Studio what to do when something needs your decision. The Power Platform CLI (PAC) is still required to create and push controls.

## Create a control

From the solution or a solution folder, choose **Add > Power Apps Component Project...**. The wizard collects the project name and location, control name, namespace, field/dataset template and optional React framework. It runs `pac pcf init` and adds the generated project to the selected solution location. Creation does not require Dataverse authentication.

While you fill in those details the wizard checks for Node.js and npm. When they are usable it shows one line, for example `Node.js v22.15.0 with npm 10.9.2 — Visual Studio's PATH`, and installs the project's dependencies after creating it. **Setup options** holds the rest: the Node.js installation to use, **Browse** for one that was not found, **Get Node.js**, and **Install project dependencies**, which you can clear. If Node.js or npm is missing or does not fit, the section opens by itself and says what is wrong and what to do.

**Get Node.js** opens the official download page in your browser. Xrm Tools does not install software on the computer. After installing Node.js, choose **Check again**; Visual Studio does not need to restart.

Creation and installation show their progress, with npm's output, and **Cancel** stops either one, including every process npm started. If the project was created but its dependencies could not be installed, the wizard stays open, says why, and offers **Retry setup** and **Open project**. The project is kept either way. A project opened that way shows a notification that its setup is incomplete. If creation itself fails or is canceled, generated files remain in the project folder and nothing is added to the solution.

If you clear **Install project dependencies**, nothing is installed until you build.

PAC projects using `RestoreProjectStyle=PackageReference` participate in CPS NuGet restore so their Microsoft build targets can be restored in Visual Studio. Existing `packages.config` projects are not automatically migrated.

## Build an existing project

Open the project and choose **Build** or **Rebuild**. There is nothing to enable and no project file to edit. Opening a project never installs anything.

- **Fresh checkout, Build.** Microsoft's targets run `npm install` when dependencies are missing, as they always have.
- **Fresh checkout, Rebuild.** Microsoft's Rebuild runs `npm run clean` before its own install, which used to fail with `'pcf-scripts' is not recognized`. Xrm Tools now runs Microsoft's install target first, so Rebuild works on a fresh checkout. This honors `PcfEnableAutoNpmInstall`.
- **Later builds.** Nothing is installed again, and an unchanged project is not rebuilt.

When a build cannot proceed without a decision, only the affected project stops, before any npm command runs. The Error List shows what is wrong and what to do, and a notification offers **Set up PCF build**. The same command is on the project's context menu as **Set Up PCF Build...**.

### Which Node.js a build uses

In this order:

1. The installation you selected for the project in **Set Up PCF Build**.
2. A Node.js and npm that Visual Studio's builds already see on PATH, when it fits the project.
3. A version already installed with [fnm](https://github.com/Schniz/fnm) that fits the project.

"Fits" means the project's `.node-version` or `.nvmrc` (in the project folder or above it, up to the repository root) and `engines.node` in its `package.json`. Version files are resolved by fnm itself, so aliases such as `lts/*` work. With no declaration, a working PATH is kept; otherwise your fnm default is used.

Xrm Tools never installs a Node.js version and never picks one arbitrarily. It asks you to choose when a declared version is not installed, when fnm has several versions and no default, when several installed versions fit `engines.node` and your default is not one of them, or when the version file and `engines.node` contradict each other.

A selection is stored per project under `%LOCALAPPDATA%\XrmTools\Pcf`, not in the project or repository. If the selected installation is later moved or deleted, or the project's declared version changes so that it no longer fits, the next build says so and asks again. **Use automatic selection** in the setup dialog forgets a selection.

Node.js installed while Visual Studio is open is found on the Windows PATH without a restart. Other version managers work when they put Node.js on the PATH Visual Studio starts with, or through **Browse**.

A selection that is not already on Visual Studio's PATH is put first on PATH for the duration of that build only, and each project's build confirms it is running the expected version before npm starts. The build output names it, for example `Xrm Tools: building 'MyControl' with Node.js v22.15.0, npm 11.4.2 (fnm default)`.

**One build uses one Node.js installation.** If loaded PCF projects in a solution need different versions, the ones that cannot use the build's version stop with `XTPCF007` instead of building with the wrong one. Select a common installation for them if they can share it, unload the others, or open the project in a second Visual Studio window.

### Set Up PCF Build

The dialog shows the project's Node.js, package manager and dependencies, with the same Node.js choices as the wizard. **Set up** saves a changed selection and installs dependencies when they are missing:

- `npm ci` when the project has a `package-lock.json` or `npm-shrinkwrap.json`, so the lock file is reproduced, not rewritten.
- `npm install` when it has none.

npm runs in the project folder with no extra options, so the project's `.npmrc`, registry and script settings apply. Setup can be canceled, and a canceled or failed setup is never reported as ready; run it again, or build, to finish. Full npm output is kept in the Xrm Tools pane of the Output window.

When npm reports a cause, setup names it: the registry rejected the request, the registry could not be reached, a package was not found, a package does not support the selected Node.js, or files in the project could not be changed. For any other failure it reports npm's exit code and the last error lines without guessing. If `npm ci` refuses because the lock file does not match `package.json`, nothing is changed; **Update lock file** runs `npm install` if that is what you want. Xrm Tools does not run `npm audit fix`, change package versions or touch registry credentials.

Setup and builds of the same project do not overlap. Setup refuses to start while a build is running, and a build of a project whose setup is still running, in this or another Visual Studio window, stops with `XTPCF006`.

### Projects that use another package manager

Xrm Tools prepares npm projects only. A project counts as another package manager's when `package.json` declares a non-npm `packageManager`, or when a `yarn.lock`, `pnpm-lock.yaml`, `pnpm-workspace.yaml` or Bun lock file is present without an npm lock file. The nearest folder that says anything decides, from the project up to the repository root. Having Yarn or pnpm installed on the computer does not matter.

For such a project Xrm Tools does not run npm, and it stops the build before Microsoft's `npm install` would add npm files to it (`XTPCF004`). No file is changed. Install dependencies with the project's package manager and set `<PcfEnableAutoNpmInstall>false</PcfEnableAutoNpmInstall>` in the project file; builds then run the project's scripts without installing. A project with lock files from two package managers stops the same way until one of them is removed or `packageManager` is declared.

### Messages

| Code | Meaning | What to do |
| --- | --- | --- |
| `XTPCF001` | Node.js was not found on Visual Studio's PATH, the Windows PATH or in fnm. | Install Node.js (LTS) or choose an installation in **Set Up PCF Build**. |
| `XTPCF002` | Node.js was found but cannot be used as is: it does not fit the project, the required version is not installed, the choice is ambiguous, or the selected installation is gone. | Follow the message; usually choose an installation in **Set Up PCF Build**. |
| `XTPCF003` | Node.js was found without npm. | Repair that installation or choose one that includes npm. |
| `XTPCF004` | The project belongs to another package manager, or has lock files from two. | See the section above. |
| `XTPCF005` | Dependencies are not installed and this step cannot install them: Clean on a fresh checkout, or automatic install is turned off. | Build the project, or use **Set Up PCF Build**. |
| `XTPCF006` | Setup is still installing dependencies for this project. | Build again when setup has finished. |
| `XTPCF007` | The project needs a different Node.js than the other PCF projects in this build. | See "One build uses one Node.js installation" above. |
| `XTPCF008` | The build process did not see the Node.js the build was set up for. Nothing was built. | Build again. If the message returns, close all Visual Studio windows and reopen the solution. |

Errors from npm scripts themselves (TypeScript, ESLint, webpack) are reported by Microsoft's targets as before.

### How it fits into the build

- Visual Studio's solution build manager still schedules, logs and cancels the build, and Microsoft's targets still run every npm command. Xrm Tools does not start a separate MSBuild, replace Microsoft's targets or override the `Exec` task.
- When a solution build starts, Xrm Tools decides the Node.js installation for that build. If it is not already on Visual Studio's PATH, it is put first on the PATH of the Visual Studio process until the build ends, then the original value is restored. In the acceptance runs every MSBuild worker of the build, including reused ones, ran npm with that installation. The machine PATH, fnm's default, shell profiles and `fnm_multishells` folders are never touched, and no shell profile is run.
- That PATH change is visible to the whole Visual Studio process while the build runs, not only to PCF projects. Other projects in the same build, and anything else Visual Studio starts during it, find the selected Node.js first. When Visual Studio's own PATH already has a fitting Node.js, nothing is changed.
- Deciding takes a few short process launches per PCF project (`node --version`, and fnm when it is needed) at the start of each solution build, with an upper limit of 30 seconds, after which the build continues without Xrm Tools' preparation.
- Each PCF project gets a generated file, `obj\<project>.pcfproj.xrmtools.g.targets`, in the MSBuild extensions folder that `Microsoft.Common.targets` imports by wildcard. This is the route by which NuGet brings in Microsoft's PCF targets, so nothing in the project file or its imports changes. The targets in it only run in front of Microsoft's `RestoreNPM`, `PcfClean`, `PcfBuild` and `PcfRebuild`.
- Those targets do nothing unless Visual Studio passes `XrmToolsPcfBuild=true`, so command-line builds and builds without Xrm Tools are unchanged. Deleting `obj` is harmless; the file is written again.

Limitations:

- Only one Node.js installation per solution build, as described above.
- **Push to Environment** runs PAC, which starts its own build with Visual Studio's PATH. It does not yet use a Node.js selected in **Set Up PCF Build**.
- `Microsoft.PowerApps.MSBuild.Pcf` reinstalls on every build when `package.json` is newer than `package-lock.json`. That is Microsoft's incremental check, and Xrm Tools does not change lock file timestamps.
- If a later version of Microsoft's package renames its npm targets, the added checks stop applying and builds behave as they do without Xrm Tools.
- Validated on Visual Studio 18 Insiders only. Visual Studio 2022, Build Selection, and solutions that mix PCF with other project types have not been tested.

Tested with PAC 2.14.1, Microsoft.PowerApps.MSBuild.Pcf 1.52.1, pcf-scripts 1.51.1, Visual Studio 18 Insiders (MSBuild 18.13.0.52701), fnm 1.38.1, Node.js 22.15.0 and 24.21.0. See [PCF build setup validation](pcf-build-setup-validation.md) for the acceptance runs.

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

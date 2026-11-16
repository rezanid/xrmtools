# PCF build setup validation

What was checked for the PCF setup and build experience described in [PCF projects in Visual Studio](pcf-projects.md), who checked it, and what was not checked.

## Environment

Windows on ARM64. Visual Studio 18 Insiders 18.13.12409.143, Experimental instance (`/rootsuffix Exp`), MSBuild 18.13.0.52701. PAC 2.14.1, Microsoft.PowerApps.MSBuild.Pcf 1.52.1, pcf-scripts 1.51.1. Node.js v22.15.0 with npm 10.9.2 on the registered Windows PATH. fnm 1.38.1 with v22.15.0 (default, npm 11.4.2) and v24.21.0 (npm 11.19.0).

Every fixture project's `build`, `clean` and `rebuild` npm scripts first print `XTNODE <project> <node version> <node path>`, so the build output shows which Node.js Microsoft's npm commands really ran.

## Manual checks in Visual Studio

Performed by the repository owner on 2026-11-16 against commit `722d0d892ae1dfa646aee5dc974aa7f3c92e0cab`, in an Experimental instance started from a shell with Node.js v22.15.0 on PATH, using fixtures created by [New-PcfManualFixtures.ps1](pcf-build-setup-evidence/scripts/New-PcfManualFixtures.ps1). All steps were reported as passed. The results were reported as pass or fail per section; build output was not recorded.

| # | Check | Expected result that was confirmed |
| --- | --- | --- |
| 1 | New-project wizard with default setup, then Build Solution | The wizard shows Node.js v22.15.0 and says dependencies will be installed; after Create the project is added without a setup notification; the solution builds. |
| 2.1 | Fresh project (lock file, no `node_modules`): first Build | Succeeds and runs with Node.js v22.15.0. |
| 2.2 | Separate fresh project: first Rebuild | Xrm Tools runs Microsoft's npm restore before Clean; Rebuild succeeds. |
| 2.3 | Separate fresh project: first Clean | Stops within seconds with `XTPCF005`, creates no `node_modules`, and does not fail with `'pcf-scripts' is not recognized`. |
| 2.4 | Unchanged project: Build again | Up to date; no npm install and no PCF compilation. |
| 3 | Cancel a Build while Microsoft's `npm install` runs, once in a project with a lock file and once in a project without one, then repair and build in the same session | The canceled build leaves a half-installed `node_modules` and a notification. The next Build stops with `XTPCF009` and runs no npm command. **Set up PCF build** offers **Repair dependencies** and says what it does: `npm ci` with a lock file; removal of the project's `node_modules` followed by `npm install` without one. After the repair the dialog reports the project ready, the project builds, and its source files and `package.json` are still there. |
| 4.1 | Solution with a project that needs Node.js 22 and one that needs Node.js 24, in a window with Node.js 22: Build Solution | The Node.js 22 project builds; the Node.js 24 project stops with `XTPCF007`. |
| 4.2 | **Set up PCF build**, **Open a second window instead** | A second Experimental window opens the solution; the first stays open. |
| 4.3 | Second window: Rebuild the Node.js 24 project | Runs with v24.21.0 from fnm's folder. |
| 4.4 | Second window: Rebuild the Node.js 22 project | Stops with `XTPCF007` and runs no npm script, so it is not built with Node.js 24. |

[Watch-PcfInstall.ps1](pcf-build-setup-evidence/scripts/Watch-PcfInstall.ps1) is the aid used for check 3. It only reads the project folder: it says when to cancel and reports whether the cancel left a half-installed folder.

## Automated rehearsals in Visual Studio

Run by UI automation on 2026-11-16 against the working tree that became commit `722d0d8`, before the interrupted-install recovery in that commit was written. They were not repeated afterwards.

| Check | Result | Log |
| --- | --- | --- |
| A window without Node.js on its PATH, fnm default installed: Rebuild, setup, restart, Rebuild | Rebuild stopped with `XTPCF007` in 4 s without running an npm script, and the window's PATH was unchanged. **Restart** with an unsaved edit showed Visual Studio's save prompt; **Cancel** left the window, the solution and the edit as they were and started nothing. **Restart** then **Save** started `devenv.exe /rootsuffix Exp "<solution>"`, the first window exited by itself, and the edit was saved. In the new window Rebuild ran `node.exe` from fnm's default folder (v22.15.0) and succeeded; a second Build installed and compiled nothing. | [02-fnm-restart.log](pcf-build-setup-evidence/02-fnm-restart.log) |
| Build Selection while another loaded PCF project needs a Node.js this window lacks; that project in a second window | Rebuild Selection of the fitting project succeeded and showed no notification about the other project. Rebuild Solution built the fitting project and stopped the other with `XTPCF007`. **Open a second window instead** left the first window and its PATH unchanged; the second window rebuilt the other project with v24.21.0 from fnm's folder. | [04-selected-project-build.log](pcf-build-setup-evidence/04-selected-project-build.log) |
| Solution with a PCF project, a C# library and a Dataverse solution project | All three built. Visual Studio's PATH was identical before, during (recorded by a build step of the C# project) and after the build. With the PCF project asking for Node.js 24, it stopped with `XTPCF007`, the PATH was still identical, and fnm's Node.js 24 folder was not on it. | [05-mixed-solution.log](pcf-build-setup-evidence/05-mixed-solution.log) |
| Setup requested while a build installs dependencies for the same project; build while another process holds the project's setup lock | Setup refused with "A build is running" and ran no npm; the build finished undisturbed with one install. With the lock held, Rebuild stopped with `XTPCF006` in 2 s; after the lock was released Rebuild succeeded and no lock file was left. | [06-cancel-retry-contention.log](pcf-build-setup-evidence/06-cancel-retry-contention.log), 6b and 6c |

Notes on those logs:

- Section 6a of the last log shows the defect that the recovery in `722d0d8` fixes: after a canceled build, the next build ran `npm install` on top of the half-installed folder, cleared the marker and failed with `Cannot find module`. Manual check 3 above is the verification of the fix.
- In the first log, the comparison "PATH of the new window = fnm default folder + PATH of the first window" printed `False`. The difference was not investigated. The same log shows that the new window's PATH starts with fnm's default folder and that the build ran `node.exe` from it.
- In the second log, "P24 appears in that build's output: True" matched NuGet's `Restored ...P24.pcfproj` line. The `Project:` lines show that only P22 was built.
- The second log's last line is an up-to-date Build of the Node.js 22 project in the Node.js 24 window, which exercises nothing. Manual check 4.4 covers that case with a Rebuild.
- The mixed-solution log shows the Dataverse solution project starting after the PCF project failed. That is Visual Studio's scheduling of a dependent project after a failed prerequisite, described in [PCF build failure behavior](pcf-build-failure-behavior.md), and is not changed here.

## Covered by automated tests only

Not exercised in Visual Studio for this revision:

- The **Done** button of Set Up PCF Build and its return to **Set up** when another Node.js is chosen (added after the manual checks).
- **Retry setup** in the wizard removing what a canceled install left.
- Repair refusing a `node_modules` that is a link, and removing links inside it without following them.
- **Push to Environment** starting PAC with the project's Node.js. No deployment was run.
- No usable Node.js at all, an unreachable registry during the wizard's install, and a project configured for another package manager. These were exercised in Visual Studio for the earlier revision `eeb3902` of this work and have unit tests in this one, but were not repeated.
- Node.js without npm, a registry that rejects the request, and a lock file that does not match `package.json`.
- Lookup of `node.com`, `node.bat` and `node.cmd` for shim-based version managers.

## Not checked

- Visual Studio 2022.
- Version managers other than fnm, beyond a Node.js that is already on PATH.

## Known limitations

See the limitations in [PCF projects in Visual Studio](pcf-projects.md#how-it-fits-into-the-build). In particular:

- An interrupted build-time `npm install` in a project that already had dependencies is not detected, so no repair is offered for it.
- Clean on a fresh checkout stops with `XTPCF005` and does not install.
- After a detected interrupted install, Clean and Push to Environment are stopped too until the project is repaired.
- One Node.js per Visual Studio window.

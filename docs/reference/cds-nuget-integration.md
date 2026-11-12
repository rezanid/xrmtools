# NuGet Package Manager for PAC-generated Dataverse projects

## Supported behavior

A PAC-generated `.cdsproj` opened through Xrm Tools can use **Manage NuGet Packages** with its existing `PackageReference` format. Opening the project does not rewrite it. Installing or uninstalling packages uses Visual Studio's normal CPS item-editing services; commit the resulting project file, not a VS-specific replacement project format.

The integration applies to native, non-.NET Xrm Tools projects whose **evaluated** `RestoreProjectStyle` is `PackageReference` and whose project directory does not contain `packages.config`. The same guarded integration also supports [PCF projects](pcf-projects.md), allowing their Microsoft build targets to restore in Visual Studio. It does not change SDK-managed projects, machine-wide NuGet settings, or existing `packages.config` installations. After manually changing the package-management format, reload the project before using the package manager.

Projects already modified by the old UI behavior are deliberately not migrated. Restore the accidental `packages.config`, its `None` entry, and the added assembly reference/`HintPath` changes from source control if those changes were unintended, preserving any legitimate package changes. Merely deleting `packages.config` is not a complete reversal of an installation.

## Root cause and provider trace

The [NuGet project-system integration documentation](https://learn.microsoft.com/en-us/nuget/visual-studio-extensibility/project-system-support) requires capabilities to represent implemented behavior. `RestoreProjectStyle` alone does not select NuGet's CPS provider.

1. `CdsProject.pkgdef` registers the `.cdsproj` selector for project type `D3ACB8F3-9E41-48CD-A302-4B7A9D6A2A49`. The `ProjectTypeRegistration` in `Properties/AssemblyInfo.cs` and `DataverseSolutions/CdsProjectRegistration.cs` supply the CPS project registration and static capabilities.
2. Before this fix, a fresh PAC project reported the following **runtime** values through `IVsHierarchy`'s `VSHPROPID_ProjectCapabilitiesChecker`, `IVsBuildPropertyStorage`, and DTE:

   | Check | Before | After |
   |---|---|---|
   | Project kind | Xrm Tools CDS GUID above | unchanged |
   | `CPS` | true | true |
   | `PackageReferences` | **false** | **true** |
   | `.NET` | false | false |
   | `AssemblyReferences` | true | true |
   | `RestoreProjectStyle` | `PackageReference` | unchanged |
   | DTE `Project.Object is VSProject4` | false | false |

3. In the exact installed NuGet release, [`CpsPackageReferenceProjectProvider.TryCreateNuGetProject`](https://github.com/NuGet/NuGet.Client/blob/ef43ac21c658b15d568f49b6375c98a8ba6f162c/src/NuGet.Clients/NuGet.PackageManagement.VisualStudio/Projects/CpsPackageReferenceProjectProvider.cs) first requires a hierarchy, then **both `CPS` and `PackageReferences`**. The second capability was the first failed condition. The subsequent restore-style condition would have passed.
4. [`LegacyPackageReferenceProjectProvider.TryCreateProjectServices`](https://github.com/NuGet/NuGet.Client/blob/ef43ac21c658b15d568f49b6375c98a8ba6f162c/src/NuGet.Clients/NuGet.PackageManagement.VisualStudio/Projects/LegacyPackageReferenceProjectProvider.cs) cannot rescue this project: its first gate requires `Project.Object as VSProject4`, which was null. Adding `AssemblyReferences` cannot fix either gate; it was already present.
5. The fallback [`MSBuildNuGetProjectProvider`](https://github.com/NuGet/NuGet.Client/blob/ef43ac21c658b15d568f49b6375c98a8ba6f162c/src/NuGet.Clients/NuGet.PackageManagement.VisualStudio/Projects/MSBuildNuGetProjectProvider.cs) creates `VsMSBuildNuGetProject`, using a packages.config-based project. Its Installed list does not represent the project's existing Microsoft `PackageReference` items. Its Newtonsoft installation creates `packages.config` plus an assembly reference instead.

This is an extension-owned missing opt-in, not evidence that PAC projects must be converted to the AlbanianXrm SDK or to the C# project type. The AlbanianXrm comparison changes both the project declaration and project system. The same untouched PAC declaration works with the Xrm Tools CDS project type after this fix.

## Why the capability is backed by services

`CdsPackageReferenceCapabilitiesProvider` is a configured-project capability provider, restricted to the CDS/PCF file extensions and `(XrmToolsCdsProject | XrmToolsPcfProject) & !.NET` applicability. It reads the evaluated restore style under CPS's project read lock, honors the packages.config guard, and publishes only `PackageReferences`. Its base class refreshes after configured-project changes. It neither edits the project nor intercepts NuGet installation.

The installed Visual Studio implementations were inspected, rather than assuming a capability string implements NuGet:

- CPS `Microsoft.VisualStudio.ProjectSystem.References.PackageReferencesService` exports `IPackageReferencesService` with `AppliesTo("PackageReferences")`. It implements item addition/removal and version metadata through CPS/MSBuild editing services.
- The managed project system's `PackageRestoreDataSource`, `PackageRestoreConfiguredInputDataSource`, restore rules, and `NuGetRestoreService` apply to `PackageReferences`, not exclusively `.NET`. They collect evaluated/build restore inputs and nominate through `IVsSolutionRestoreService5.NominateProjectAsync`.
- [`CpsPackageReferenceProject`](https://github.com/NuGet/NuGet.Client/blob/ef43ac21c658b15d568f49b6375c98a8ba6f162c/src/NuGet.Clients/NuGet.PackageManagement.VisualStudio/Projects/CpsPackageReferenceProject.cs) consumes nominated restore information from `IProjectSystemCache` and the resulting assets to obtain installed packages. A missing nomination is an error, not permission to pretend installation succeeded. Installation calls the suggested configured project's `Services.PackageReferences.AddAsync`; uninstallation calls its removal service.
- Real UI listing, installation, restore, reload, and removal all succeeded with `.NET=False`. No `.NET` capability, managed-language project conversion, custom restore implementation, or consumer import injection was needed.

## Desktop validation

Tested in VS Experimental with:

| Component | Version |
|---|---|
| Visual Studio 18 Insiders (`devenv.exe`) | `18.13.12409.143` |
| Full-framework MSBuild | `18.13.0.52701` |
| CPS implementation | `18.13.43.11567` |
| NuGet package-management/VS assemblies | file version `8.1.0.38`, product `8.1.0-rc.38` |
| NuGet source commit embedded in the installed assemblies | `ef43ac21c658b15d568f49b6375c98a8ba6f162c` |
| PAC CLI | `2.14.1+g6f517e5` |

NuGet's `dev` sources were not used as the release-behavior authority. The fix retains the existing CPS `17.9.380` build-time dependency; no VSIX NuGet dependencies were upgraded. This desktop run does not establish UI compatibility for every other supported VS release.

Fresh disposable projects were generated using `pac solution init`; none referenced XrmTools.Build. Both original Microsoft references were preserved:

```xml
<PackageReference Include="Microsoft.PowerApps.MSBuild.Solution" Version="1.*" />
<PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies" Version="1.0.0" PrivateAssets="All" />
```

### Before the fix

Installed was empty. Installing Newtonsoft.Json `13.0.3` through the real NuGet UI produced these additional project items:

```xml
<None Include="packages.config" />
<Reference Include="Newtonsoft.Json">
  <HintPath>packages\Newtonsoft.Json.13.0.3\lib\net45\Newtonsoft.Json.dll</HintPath>
</Reference>
```

It also wrote:

```xml
<packages>
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net462" />
</packages>
```

The relative HintPath depends on the solution/project directory layout.

### After the fix

| Acceptance check | Result |
|---|---|
| Open untouched PAC project through Xrm Tools CDS type | Passed; project bytes unchanged on open |
| Installed lists existing direct Microsoft references | Passed; UI also displayed the transitive net462 reference-assemblies package |
| Install Newtonsoft.Json `13.0.3` through UI | Passed; PackageReference only, no packages.config or Newtonsoft assembly reference/HintPath |
| Save, close/reopen solution in the same VS process, reopen Installed | Passed; Newtonsoft and original references remained listed |
| Full-framework MSBuild `/t:Restore` on a separate copy containing only the saved project and solution sources, without copied `obj` state | Passed |
| Uninstall Newtonsoft through UI and save | Passed; only its PackageReference removed |
| Compare original Microsoft reference XML/metadata and imports after installation and removal | Passed; all preserved |
| Reopen the already packages.config-modified baseline with the fixed extension | Passed; capability stayed false, UI listed the legacy Newtonsoft installation, project hash unchanged |

The actual semantic installation diff was:

```diff
 <PackageReference Include="Microsoft.PowerApps.MSBuild.Solution" Version="1.*" />
 <PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies" Version="1.0.0" PrivateAssets="All" />
+<PackageReference Include="Newtonsoft.Json">
+  <Version>13.0.3</Version>
+</PackageReference>
```

CPS saved the version as child metadata, equivalent to a `Version="13.0.3"` attribute. Its normal save also removed blank lines; the extension did not perform a format conversion or project rewrite on open. Uninstall removed exactly the added reference semantically, retaining the original references, metadata, and imports.

## Repeating the acceptance check

1. Generate a fresh project with `pac solution init`. Open it in VS Experimental using the Xrm Tools CDS project type, not `Type="C#"`. Confirm opening has not changed the project file.
2. Right-click **Manage NuGet Packages**, choose **Installed**, and confirm both direct Microsoft references above are present.
3. Browse nuget.org, select Newtonsoft.Json version `13.0.3`, and install. **Save All** before inspecting the project diff. Expect only a new PackageReference, with no packages.config or Newtonsoft assembly HintPath.
4. Reload/reopen the solution and package manager. Confirm Newtonsoft remains listed.
5. Copy the saved project and solution sources to a fresh directory without `obj` or a local packages folder. Run full-framework `MSBuild.exe <project.cdsproj> /t:Restore` and confirm success.
6. Uninstall Newtonsoft through the UI and save. Confirm both original Microsoft references, their metadata, and all consumer imports remain.

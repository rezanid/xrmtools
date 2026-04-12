# XrmTools.UI.Controls SDK-Style Conversion Tasks

## Overview

This document tracks the conversion of XrmTools.UI.Controls from legacy MSBuild format to SDK-style project format. The conversion preserves WPF functionality, assembly metadata, and Visual Studio SDK integration while modernizing the project structure.

**Progress**: 3/3 tasks complete (100%) ![0%](https://progress-bar.xyz/100)

---

## Tasks

### [✓] TASK-001: Verify baseline and prerequisites *(Completed: 2026-04-12 15:17)*
**References**: Plan §Phase 1

- [✓] (1) Verify current branch is `135-fetchxml-editor-param-in-code-completion`
- [✓] (2) Current branch confirmed (**Verify**)
- [✓] (3) Record current `XrmTools.UI.Controls.csproj` contents for rollback reference
- [✓] (4) Project file backed up (**Verify**)
- [✓] (5) Verify `Properties/AssemblyInfo.cs` contains `ThemeInfo` and assembly metadata
- [✓] (6) AssemblyInfo.cs structure confirmed (**Verify**)
- [✓] (7) Verify project contains key file groups: `*.xaml`, `*.xaml.cs`, `Properties/Resources.resx`, `Properties/Settings.settings`
- [✓] (8) All key file groups present (**Verify**)

---

### [✓] TASK-002: Convert project file to SDK-style format *(Completed: 2026-04-12 15:19)*
**References**: Plan §Phase 2, Plan §Phase 3, Plan §Risks

- [✓] (1) Replace root element with `<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">`
- [✓] (2) Add core properties per Plan §Phase 2: `TargetFramework=net48`, `OutputType=Library`, `UseWPF=true`, `RootNamespace=XrmTools.UI.Controls`, `AssemblyName=XrmTools.UI.Controls`, `LangVersion=13.0`, `Deterministic=true`, `GenerateAssemblyInfo=false`, `SignAssembly=false`
- [✓] (3) Preserve dependencies: `Community.VisualStudio.Toolkit.17`, `Microsoft.VisualStudio.LanguageServices`, `Microsoft.VisualStudio.SDK`, `System.Text.Json`, project reference to `XrmTools.Core`
- [✓] (4) Remove legacy constructs: `ToolsVersion`, `Microsoft.Common.props` import, `Microsoft.CSharp.targets` import, configuration-specific PropertyGroups, explicit framework references, explicit Compile/Page items
- [✓] (5) Check if `Resources.resx` and `Settings.settings` require explicit metadata per Plan §Phase 3
- [✓] (6) Add minimal explicit item metadata only if default handling insufficient (**Verify**)
- [✓] (7) Verify XAML theme files under `Themes\` are included by WPF defaults
- [✓] (8) Project file is valid SDK-style XML (**Verify**)
- [✓] (9) Commit changes with message: "TASK-002: Convert XrmTools.UI.Controls to SDK-style format"

---

### [✓] TASK-003: Build and validate converted project *(Completed: 2026-04-12 15:20)*
**References**: Plan §Phase 4, Plan §Validation Checklist

- [✓] (1) Restore packages for `XrmTools.UI.Controls`
- [✓] (2) Packages restored successfully (**Verify**)
- [✓] (3) Build `XrmTools.UI.Controls`
- [✓] (4) Project builds with 0 errors (**Verify**)
- [✓] (5) Verify no duplicate assembly attribute errors
- [✓] (6) No duplicate attribute errors present (**Verify**)
- [✓] (7) Verify XAML-generated files produced correctly
- [✓] (8) XAML generation successful (**Verify**)
- [✓] (9) Verify `Resources.Designer.cs` remains valid
- [✓] (10) Resource designer valid (**Verify**)
- [✓] (11) Verify `Settings.Designer.cs` remains valid
- [✓] (12) Settings designer valid (**Verify**)
- [✓] (13) Build at least one dependent project that consumes `XrmTools.UI.Controls`
- [✓] (14) Dependent project builds successfully (**Verify**)
- [✓] (15) Remove any temporary compatibility entries no longer needed
- [✓] (16) Commit cleanup changes with message: "TASK-003: Complete SDK-style conversion validation"

---






XrmTools.WebResources.Sdk is an MSBuild project SDK that lets you build Power Platform (Dataverse) TypeScript/JavaScript web resources as part of your regular build. It wraps your npm-based web resource project so it restores, builds, and cleans through standard MSBuild and Visual Studio commands, and integrates cleanly with the Power Platform Solution Packager.

This package is part of the [Xrm Tools](https://marketplace.visualstudio.com/items?itemName=rezanid.XrmTools) extension for Visual Studio, which provides a set of tools to enhance your development experience with Microsoft Dataverse (formerly known as Common Data Service or Dynamics 365).

If you aren't already using [Xrm Tools](https://marketplace.visualstudio.com/items?itemName=rezanid.XrmTools) for Power Platform development, check out the [Xrm Tools Wiki](https://github.com/rezanid/xrmtools/wiki) to learn how a modern approach to Power Platform development can enhance your experience.

## Usage

Reference the SDK from your web resource project file:

```xml
<Project Sdk="XrmTools.WebResources.Sdk/1.0.0">
</Project>
```

Your project directory must contain a `package.json`. The SDK invokes the following npm scripts during the corresponding MSBuild targets:

| MSBuild target | Default command      |
| -------------- | -------------------- |
| Restore        | `npm install` (local) / `npm ci` (CI) |
| Build (Debug)  | `npm run build:debug`|
| Build (Release)| `npm run build:release`|
| Clean          | `npm run clean`      |

### Restore behavior

For local development the Restore target runs `npm install`, so adding or changing packages in `package.json` is picked up automatically on the next build — no need to run `npm install` from a terminal.

In a build pipeline the SDK switches to `npm ci` for a clean, reproducible install, but only when a committed `package-lock.json` is present (it is required by `npm ci`). A build is treated as CI when any of `ContinuousIntegrationBuild`, `CI`, `TF_BUILD`, or `GITHUB_ACTIONS` is `true`. If no lock file exists, it falls back to `npm install`.

You can force either behavior explicitly with `NpmRestoreCommand` (see below), or set `IsCiBuild` yourself.

## Customization

You can override any of the default commands or the output folder via MSBuild properties in your project:

```xml
<PropertyGroup>
  <NpmRestoreCommand>npm install</NpmRestoreCommand>
  <NpmBuildCommand>npm run build</NpmBuildCommand>
  <NpmCleanCommand>npm run clean</NpmCleanCommand>
  <BuildOutputFolder>$(MSBuildProjectDirectory)\dist</BuildOutputFolder>
</PropertyGroup>
```

For example, to always use a reproducible install, pin the command regardless of environment:

```xml
<PropertyGroup>
  <NpmRestoreCommand>npm ci</NpmRestoreCommand>
</PropertyGroup>
```

To learn more about the Xrm Tools extension for Visual Studio, please refer to:
* [GitHub repository](https://github.com/rezanid/xrmtools)
* [Official documentation](https://rezanid.github.io/xrmtools/)

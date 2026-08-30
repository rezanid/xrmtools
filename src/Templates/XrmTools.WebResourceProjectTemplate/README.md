# $projectname$

A small TypeScript web-resource project for Microsoft Dataverse, built with
`XrmTools.WebResources.Sdk`.

The sample adds a friendly notification to the Account form and keeps it current when the
account name changes. More importantly, it demonstrates the scaffolding:

- source files live in `src`;
- build output is written to `dist`;
- form handlers are exported through a global namespace; and
- the project declares the solution and web-resource name prefix used during registration.

## Before the first registration

Open `$safeprojectname$.esproj` and replace:

- `YourSolutionUniqueName` with the unique name of an unmanaged Dataverse solution;
- `new_` with your publisher prefix (and adjust the rest of the path if desired).

Build the project. Xrm Tools restores the npm packages and compiles
`src/AccountExperience.ts` to `dist/AccountExperience.js`.

Then right-click the project and choose **Register Web Resources**. The sample is registered as
`new_/scripts/AccountExperience.js` when the default prefix is used.

## Add the sample to an Account form

1. Add the registered JavaScript web resource as a form library.
2. Add `AccountExperience.onLoad` to the form **On Load** event.
3. Select **Pass execution context as first parameter**.
4. Save and publish the form.

The handler also subscribes to changes of the Account Name field. You do not need to configure a
separate On Change handler, but `AccountExperience.onNameChange` is public if you prefer to wire it
explicitly.

## Grow the project

Create another `.ts` file under `src` for each form or feature. Keep form entry points inside a
namespace so Dataverse can find them by name. Build output beneath `dist` is discovered
automatically; no project-file entry is needed for each new web resource.

Keep the generated `package-lock.json` in source control so CI builds remain reproducible.

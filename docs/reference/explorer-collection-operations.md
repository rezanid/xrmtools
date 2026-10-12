# Explorer collection operations

Dataverse Explorer supports focused collection tasks inside its existing tool window.
The first task is **Generate OpenAPI specification…** on Plugin Packages, Plugin
Assemblies, a package, or an assembly. The existing single-API command remains available.

## Developer experience

The native search box and dropdown provide name search, match-case and recent-update
filters for the active workspace. Browsing search is restored on returning from a task.
The collection workspace shows its operation, environment and scope, then discovers
eligible APIs independently of which browsing branches have been expanded. APIs are
grouped by assembly and show both display and unique names.

- Check an API or use the assembly checkbox to select its matching APIs.
- Search API names or assembly names. Searching preserves checked items outside the results.
- **Select matching** adds the current matches; **Clear selection** clears all checked items.
- The count shows both total selection and matching candidates.
- Use the Explorer toolbar's **Refresh** to reload candidates and reconcile selection.
- Native toolbar moniker buttons provide Generate, Cancel, Select matching and Clear selection.
- **Generate** opens one unsaved document using the configured OpenAPI version and output format.
- Toggle **Generate Scalar HTML documentation** (Web icon) to also open a companion
  `custom-apis.openapi.html` document. This is off for each new selection session.
  Save both documents with their generated names in the same folder, then serve that
  folder over HTTP to view the HTML page. It reads the JSON/YAML specification by
  relative URL, so subsequent specification edits appear without regenerating HTML.
  Scalar loads from its CDN and requires an internet connection.
- **Cancel** or Escape returns to browsing. Ctrl+F focuses the active workspace's search.
- Arrow keys navigate the tree; Space toggles the focused row or group.

Package/category scope follows the browsing categories: Plugin Assemblies contains
standalone assemblies; Plugin Packages contains packaged assemblies. APIs without an
associated plugin assembly are outside these scopes.

## Adding another collection task

Export `IExplorerCollectionOperation` through MEF. Supply the title/action label,
supported scopes, candidate discovery, and execution. Candidates carry a stable identity,
display/unique names, grouping information and an operation-specific target.

`ExplorerCollectionSession` owns checked identities, filtered views, group states,
loading, error handling, cancellation and immutable execution selection. The shared
`ExplorerCollectionControl` handles presentation and keyboard interaction; native Visual
Studio toolbar commands route actions to the active session. The Explorer
adds entry commands to applicable context menus. No new dialog or browsing-node flags
are needed for another task.

Browsing objects are retained while the collection workspace is visible. Its search,
expansion, scroll position and focused row are independent of collection state. Source
navigation is not triggered by checking collection candidates.

Environment changes and disposal cancel the session. Registration changes reload
candidates when idle and cancel an executing task. Candidate refresh preserves surviving
identities, reports removed selections, and preserves group expansion. Failed discovery
disables execution until a successful refresh. Failed execution retains selection for retry.

## OpenAPI generation

Discovery uses a paged Custom API query with single-valued plugin-type and assembly
expansions. It does not fetch parameters, plugin steps or images. Generation retrieves
details only for selected APIs and shares table metadata across the document.

The writer builds operations and one document envelope. Server/security settings and
the common Dataverse error schema are emitted once. Assembly names become operation
tags by default; disable **Group APIs by Assembly** under General Options →
**OpenAPI Generation** to omit those tags. Operation and component ordering is deterministic. Duplicate identities/operation
IDs and invalid API metadata fail the operation rather than produce partial output.

Source models use short type names without a `Source_` prefix or nullable suffix.
Namespace qualification is retained only when different types have the same short name.
Identical source component definitions are shared. Conflicting names and any schemas
that depend on those conflicts are namespaced by API unique name. References, including
recursive references, are rewritten in schema locations only. Example payloads are
preserved. Source schemas cannot replace the built-in Dataverse error envelope.

## Verification

Tests cover selection/filtering, refresh reconciliation, cancellation and late results,
browsing-state preservation, WPF bindings, scope membership, discovery/parameter paging,
environment checks, shared metadata, mixed actions/functions, both output formats,
both schema versions, and recursive schema collisions.

For optional offscreen light/dark previews, set `XRMTOOLS_COLLECTION_PREVIEW_DIR` when
running `CollectionSessionTests`. These are WPF renders, not a connected Dataverse session.

Before release, smoke-test the extension in Visual Studio against an environment with
standalone and packaged APIs: verify context-menu scopes, keyboard and checkbox behavior,
toolbar refresh, cancellation during discovery/generation, environment switching, and
the generated document in the intended OpenAPI consumer.

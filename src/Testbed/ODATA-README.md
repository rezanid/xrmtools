# Dataverse request editor

Open `DataverseRequests.odata` in the experimental Visual Studio instance after building Xrm Tools. Select an environment using the existing Xrm Tools toolbar. Opening a `.odata` file is the opt-in; there is no separate enable setting. The editor shows a Send Request link above each request and a resizable response pane with Body, Headers and Raw tabs. The response pane starts hidden and appears on the first Send Request attempt, including when validation fails. It remains visible for the rest of that document session. Send at caret is also available once the pane is visible. While a request runs, its inline action becomes Cancel Request and other request actions are disabled. The action returns to Send Request when execution finishes.

The environment name and URL are shown above the response. Refresh environment rereads the selection without signing in. Sending obtains a valid token using the existing authentication service. If the displayed environment no longer matches the current selection before dispatch, the request is rejected; refresh and send again. A request already dispatched stays bound to its captured environment. Cancel stops waiting; it cannot undo server-side work.

The response pane defaults to "Below the document". To place it on the right, choose "On the side" under Tools > Options > Xrm Tools > General > OData Editor > Preview location, using the same labels as the FetchXML editor. Close and reopen existing .odata documents after changing this option. Drag the divider to resize the pane in either layout.

## Format

- `### Optional heading` separates requests. `#` and `//` comments are supported outside bodies.
- GET, POST, PUT, PATCH, DELETE, HEAD and OPTIONS are supported. HTTP/1.1 is optional on the request line.
- Relative paths, including `/WhoAmI`, resolve under `/api/data/v9.2/`. Absolute URLs must remain under that same environment's Web API root.
- Headers follow the request line. A blank line starts the body. JSON bodies default to `application/json; charset=utf-8`.
- Define document variables with `@name = value` outside requests and reference them with `{{name}}`. Names are case-sensitive; values are expanded literally, so encode URL values and escape JSON values yourself.
- For a saved `.odata` file, `odata-client.env.json` is searched for in its directory and each parent directory up to the filesystem root. The nearest file supplies variables from a profile whose name matches the selected Xrm Tools toolbar environment. That one file is used for the whole document; a nearer file takes precedence rather than merging with its parents. Profile values are literal strings; document declarations override profile values. Missing files and unmatched profiles contribute no variables.

For example, put this JSON in the nearest `odata-client.env.json`:

```json
{
  "Development": { "account1": "11111111-1111-1111-1111-111111111111" },
  "Test": { "account1": "22222222-2222-2222-2222-222222222222" }
}
```

With **Development** selected, `GET /accounts({{account1}})` uses its ID and switches to the **Test** ID when that toolbar environment is selected. `@account1 = ...` in the `.odata` document overrides either profile. Changes to any candidate sidecar and toolbar selection refresh the editor context; malformed or ambiguous configuration is diagnosed without showing values. In that case, a warning bar at the top of the `.odata` document explains why environment variables are unavailable. Use **Open environment file** in that bar to open the nearest `odata-client.env.json`. The bar closes after the file is fixed and saved. If you dismiss it, it stays hidden until the error changes. File I/O is performed in the background, not while typing.
- Use `{{$guid}}` to generate a fresh GUID for each occurrence when sending a request. This also works through document variables. In JSON, put the placeholder in quotes, for example `"accountid": "{{$guid}}"`.
- Name a request with `# @name createAccount` (or `// @name createAccount`) before its request line. Send it manually before using `{{createAccount.response.body.$.accountid}}`, `{{createAccount.response.body.$['@odata.etag']}}`, or `{{createAccount.response.headers.OData-EntityId}}`. Body selectors support properties and individual nonnegative indexes, such as `$.value[0].accountid`; header names are case-insensitive. Names must be unique and case-sensitive.
- Response values in JSON strings are escaped; an unquoted whole-value placeholder retains the selected JSON type. Headers and other URL positions require scalars, but an array of scalars can fill an entire query alias value, for example `&@ids={{listContacts.response.body.$.value[*].contactid}}`. The editor URL-encodes that array as JSON. Other URL values and OData string literals still need appropriate encoding/escaping. Document variables retain their literal behavior.
- Use one `[*]` in a response body selector to project values from an array, for example `{"ids": {{listContacts.response.body.$.value[*].contactid}}}`. Leave the placeholder unquoted. The result is an array in source order, even with zero or one element. A missing property or wrong type in any selected item fails the request.
- Captures are in memory for the document session and shared by views of the same buffer. Starting a named rerun clears its previous result; only a completed 2xx response replaces it. Editing its executable text or referenced document variables, renaming or deleting it, switching connection/environment, or closing the last view clears the affected capture. Moving it or changing its heading does not. Captures of other completed requests remain available. Chaining requires the same verifiable authenticated identity; resolving references never sends a request.
- Authentication is supplied in memory. Do not put bearer tokens in these files. Authorization, Host and transport framing headers cannot be overridden. Credential and cookie headers cannot be referenced from responses.
- No automatic retries or redirects. Downloads support gzip and deflate and stop at 100 MiB of decoded body bytes. The HTTP timeout is two minutes, separately from authentication. Bodies above 256 KiB are stored temporarily and shown as a truncated preview. Use **Save full body** to save the decoded payload.
- Named captures have a combined 200 MiB body budget per document. The oldest capture is evicted first. Chaining scans the full captured body and limits expanded URLs to 32 Ki characters and other substituted values to 4 Mi characters.

Custom API example (replace the API and parameter names with yours):

```http
### Invoke API
POST /new_MyCustomApi
Content-Type: application/json

{
  "Input": "example"
}
```

This is a separate content type, not Visual Studio's HTTP editor. XPath, dynamic variables other than `$guid`, `$shared`, `.user` overrides, `$processEnv`, secret providers, scripts, file includes, HTTP/2 and HTTP/3 are not supported. Embedded JSON/OData expression coloring and a language server are future work. It never exports tokens into process variables.

Inside `{{…}}`, IntelliSense suggests document variables, environment variables, built-ins such as `$guid`, and named requests. Environment variables have their own completion category; a document declaration that overrides one appears once as a document variable. After a named request, type a dot to continue with `response`, then `body` or `headers`. Completion also offers the JSON root `$` after `body.`. After sending a named request, try completion after `response.headers.`, `response.body.$.`, or `response.body.$.value[`. Keys requiring bracket notation are inserted with quotes. Projection suggestions sample up to eight items. You can invoke completion with Ctrl+Space.

## Editing and navigation

- Syntax highlighting covers request lines, headers, headings, comments, declarations and placeholders. Use the outlining controls in the editor gutter to collapse multi-line request sections.
- Editor diagnostics flag duplicate request names, unknown document or environment variables or named requests, malformed response selectors, and invalid sidecar configuration.
- Use **Go To Definition** on a variable or named-request reference to jump to its document declaration. Environment variables have no document definition.
- Use **Rename** on a document variable or named request, from its declaration or a reference. Environment variables are not renameable. The popup starts with the current name selected. Enter applies the new name throughout the document, Esc discards the draft, and Undo reverts a committed rename in one step.
- On a literal declaration such as `@account1 = 11111111-1111-1111-1111-111111111111`, choose **Move to Environment Variables** to add it to the selected toolbar environment's profile in the nearest sidecar, or create a sibling sidecar when none exists. The declaration is removed only after the sidecar update succeeds. Other profiles are preserved and must receive their own values; an existing different value is reported as a conflict and is not overwritten. Declarations containing placeholders such as `{{$guid}}` are not offered. Undo restores the declaration in the editor; it does not undo the sidecar update.

JSON responses use a read-only Visual Studio JSON view when available. Use Ctrl+A and Ctrl+C, or the right-click **Select All** and **Copy** commands, to copy text from that view. Other response formats use a plain-text preview.

## Manual validation

1. Open the sample in each layout and confirm the response pane initially occupies no space. Choose Send Request and confirm it appears and stays visible after success, failure or cancellation. Reopen the document and confirm it starts hidden again. Also verify Send Request actions stay attached to their requests after edits, scrolling and zooming.
2. Execute WhoAmI and accounts; inspect body, response headers and the resolved request URL.
3. Switch environments and confirm the target label and `account1` resolution update from the selected profile. Create, edit, and delete a nearer `odata-client.env.json` while open and confirm it takes precedence; delete it and verify the parent profile is restored. Confirm completion and diagnostics refresh. Change environment during authentication and verify no request is dispatched to the new target.
4. Run a malformed request or unknown variable; confirm an actionable error without a network request. Try malformed JSON and duplicate profile names; verify diagnostics do not reveal variable values. Confirm that the document warning bar appears once, even in a split window. Confirm that **Open environment file** opens the nearest sidecar, and that the bar closes after you save a fix.
5. Start a request using either its link or Send at caret. Verify only that request shows Cancel Request and other links are disabled. Insert lines above the running request and cancel using its moved link. Check that all links reset after success, failure and cancellation. Also close during execution and reopen; verify the editor remains usable.
6. Check basic request syntax and placeholder coloring in dark and light themes, including incomplete requests, a long document, scrolling and edits that add or remove the header/body separator. Confirm body text with colons is not colored as headers. Check resizing the response pane in both layouts. Change the layout option with a document open: its current response should remain intact; close and reopen it to apply the new layout. Verify Send Request and Send at caret both target the visible pane.
7. On a saved copy of the sample, move a literal declaration into an ancestor sidecar and into a newly created sibling sidecar. Verify other profiles and entries remain intact, an identical value is reused, and a conflicting value is reported without being overwritten. Try a declaration containing `{{$guid}}` and confirm no action is offered. Undo the move and verify the declaration returns while the sidecar value remains. Also test a failed sidecar write and confirm the declaration remains.
8. In a designated test environment only, send the sample create request, then the retrieve request using its returned `accountid`. Confirm the retrieved account, then send the cleanup DELETE and verify removal. Test unresolved references before creating anything; cancel or fail a rerun and confirm the old capture cannot be used.

The old HTTP probe document is retained for investigation history; its timer and environment-file writer have been removed.

The response summary shows HTTP status, elapsed send/download time (excluding authentication), and response body size in bytes before decoding or formatting. Raw shows the managed request and response, including unformatted bodies; Authorization, Proxy-Authorization, Cookie and Set-Cookie headers are redacted. It is not a byte-for-byte wire capture; other application data is displayed as received.

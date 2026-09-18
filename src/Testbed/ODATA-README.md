# Dataverse request editor

Open `DataverseRequests.odata` in the experimental Visual Studio instance after building Xrm Tools. Select an environment using the existing Xrm Tools toolbar. Opening a `.odata` file is the opt-in; there is no separate enable setting. The editor shows a Send Request link above each request and a resizable response pane with Body, Headers and Raw tabs. The response pane starts hidden and appears on the first Send Request attempt, including when validation fails. It remains visible for the rest of that document session. Send at caret is also available once the pane is visible. While a request runs, its inline action becomes Cancel Request and other request actions are disabled. The action returns to Send Request when execution finishes.

The environment name and URL are shown above the response. Refresh environment rereads the selection without signing in. Sending obtains a valid token using the existing authentication service. If the displayed environment no longer matches the current selection before dispatch, the request is rejected; refresh and send again. A request already dispatched stays bound to its captured environment. Cancel stops waiting; it cannot undo server-side work.

The response pane defaults to "Below the document". To place it on the right, choose "On the side" under Tools > Options > Xrm Tools > General > OData Editor > Preview location, using the same labels as the FetchXML editor. Close and reopen existing .odata documents after changing this option. Drag the divider to resize the pane in either layout.

## Format

- `### Optional name` separates requests. `#` and `//` comments are supported outside bodies.
- GET, POST, PUT, PATCH, DELETE, HEAD and OPTIONS are supported. HTTP/1.1 is optional on the request line.
- Relative paths, including `/WhoAmI`, resolve under `/api/data/v9.2/`. Absolute URLs must remain under that same environment's Web API root.
- Headers follow the request line. A blank line starts the body. JSON bodies default to `application/json; charset=utf-8`.
- Define document variables with `@name = value` outside requests and reference them with `{{name}}`. Names are case-sensitive; values are expanded literally, so encode URL values and escape JSON values yourself.
- Authentication is supplied in memory. Do not put bearer tokens in these files. Authorization, Host and transport framing headers cannot be overridden.
- No automatic retries or redirects. Responses are buffered up to 4 MiB; larger responses fail. Requests time out after two minutes, separately from authentication.

Custom API example (replace the API and parameter names with yours):

```http
### Invoke API
POST /new_MyCustomApi
Content-Type: application/json

{
  "Input": "example"
}
```

This is a separate content type, not Visual Studio's HTTP editor. Environment JSON files, `$processEnv`, request chaining, scripts, file includes, HTTP/2 and HTTP/3 are not supported. Syntax completion and a language server are future work. It never exports tokens into process variables.

## Manual validation

1. Open the sample in each layout and confirm the response pane initially occupies no space. Choose Send Request and confirm it appears and stays visible after success, failure or cancellation. Reopen the document and confirm it starts hidden again. Also verify Send Request actions stay attached to their requests after edits, scrolling and zooming.
2. Execute WhoAmI and accounts; inspect body, response headers and the resolved request URL.
3. Switch environments and confirm the target label updates. Change environment during authentication and verify no request is dispatched to the new target.
4. Run a malformed request or unknown variable; confirm an actionable error without a network request.
5. Start a request using either its link or Send at caret. Verify only that request shows Cancel Request and other links are disabled. Insert lines above the running request and cancel using its moved link. Check that all links reset after success, failure and cancellation. Also close during execution and reopen; verify the editor remains usable.
6. Check dark/light themes and resizing the response pane in both layouts. Change the layout option with a document open: its current response should remain intact; close and reopen it to apply the new layout. Verify Send Request and Send at caret both target the visible pane.

The old HTTP probe document is retained for investigation history; its timer and environment-file writer have been removed.

The response summary shows HTTP status, elapsed send/download time (excluding authentication), and response body size in bytes before decoding or formatting. Raw shows the managed request and response, including unformatted bodies; Authorization, Proxy-Authorization, Cookie and Set-Cookie headers are redacted. It is not a byte-for-byte wire capture; other application data is displayed as received.

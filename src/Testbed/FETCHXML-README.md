# FetchXML editor UI validation

The existing FetchXML options still control preview visibility, initial side/below location, and manual/on-save/on-change execution. The preview is enabled and on the side by default. Location changes apply after closing and reopening the document.

A single **Execute query** link appears above the document's fetch element. It executes the current document text, including edits made just before clicking. During execution both the inline link and the preview button offer **Cancel**. Explicit execution reveals a disabled preview for that document session.

The preview shows Ready, Running, Success, Error, or Canceled. Success includes the returned record count and Web API request duration; more-records information is preserved. Errors appear as selectable text. Previous rows remain available during execution and after failure/cancellation, with an explicit previous-results label.

## Manual checks in the experimental VS instance

1. Open two FetchXML documents. Confirm each has one action above the fetch root (also with an XML declaration and comments), and independently execute different queries.
2. Check both preview locations, drag the divider, and reopen to confirm size persistence. Change location with a document open and verify it stays in its original location until reopened.
3. Execute immediately after editing the entity or filter. Verify the latest query is used.
4. Verify success with rows and with zero rows, then cause a server or parsing error. Confirm error details are selectable and old rows are clearly identified.
5. Cancel a slow request; rapidly edit in on-change mode; close a view during execution. Confirm no stale response replaces newer results and no work starts after closing.
6. Verify manual, on-save, on-change, and run-on-open options. With preview disabled, automatic on-change/on-save execution should not run; explicitly executing reveals the preview.
7. Verify inline links track edits, scroll and zoom; check keyboard navigation and light/dark/high-contrast themes.

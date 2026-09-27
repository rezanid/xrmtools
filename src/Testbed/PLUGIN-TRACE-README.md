# Plugin Trace Explorer

Open **View > Other Windows > Plugin Trace Explorer** in Visual Studio with Xrm Tools installed. It uses the currently selected Dataverse environment and reads `plugintracelogs`; it does not enable tracing or modify/delete logs.

## Filters

- Choose a relative duration or a custom local start/end time, enter a full or partial type name, and optionally select **Errors only**. Apply with the button or Enter in the quick filters.
- **Advanced OData filter** normally adds a parenthesized condition to the quick filters. **Edit entire $filter** replaces them completely. The effective filter preview shows how conditions combine. Ctrl+Enter applies from the editor.
- Filter edits are drafts until Apply. Invalid server expressions leave the previous results visible. Refresh uses the last successfully applied filter.
- Open **Saved views** to select, save, rename, or delete a filter without leaving the filter bar. Save overwrites the selected view when its name is unchanged; changing the name creates the renamed view. Selecting a view applies it immediately. Views persist in your Visual Studio user settings; relative durations remain relative.
- Results arrive newest first by creation time, with the record ID as a deterministic tie-breaker. Every column is sortable; the selected column and direction remain in effect as results refresh. Displayed timestamps are local; API filtering uses UTC.

## Reading and refreshing

- Details are closed initially. Select a row to open its exception, trace message, and full record. **Close details** or Escape restores the full-width list. The same row can be opened again with one click.
- Right-click a trace and choose **Go To Definition**, or press F12 with a row selected, to open the exact plugin type declaration in the current solution. Assembly-qualified names select the matching project. Missing or ambiguous types produce a status message; no unrelated class is opened.
- The selected detail is a snapshot. Auto-refresh never replaces its text, selected text, tab, or scroll position.
- Polling defaults to every 10 seconds and can be switched off or changed. The Refresh button becomes Cancel while a refresh is active. Polling is noninteractive, skips hidden windows, and does not overlap requests.
- After the initial query, **Trace logging** shows the environment's current setting and can change it to Off, Exceptions, or All. The selector is disabled if the setting cannot be read or updated, leaving the displayed traces unchanged.
- Changed results appear behind **Show updates**. Applying those updates preserves the selected record and list scroll anchor. A selected record outside the new result window remains available until selection is cleared.
- If the selected trace changes, **Load updated trace** explicitly replaces its snapshot.
- **Show related execution** queries the correlation ID across the time window. **Back to results** restores the previous results, filter draft, and selected trace.
- Queries follow server paging up to 2,000 records. A visible warning explains when results are capped; narrow the filter to see later traces. Full records are fetched only on selection.
- Environment switches cancel outstanding work and clear results. Apply to load the newly selected environment. Authentication, filter, and network errors leave existing results available.

## Validation in a connected environment

1. Open the window and confirm details are closed and rows are chronological.
2. Trigger a plugin that emits multiple trace lines, and another that throws. Verify the exception/message and full record views, wrapping, text search, and copy selection.
3. Select a row, scroll/select text in details, and trigger more executions. Confirm only the update notification changes; Show updates must keep the selected trace and its text stable.
4. Close details and reopen the same row. Turn auto-refresh off and verify no scheduled requests occur. Manual Refresh remains available.
5. Save a relative filter, reopen Visual Studio, and select it. Confirm it uses the new current time. Test additional OData conditions and full-filter replacement, including an invalid expression.
6. Change environments during a slow request. Old responses must not populate the new environment's view.
7. Inspect a related execution and return to the original results. Check both light and dark themes and resize the details split.

Automated coverage includes filter composition/escaping, relative/custom windows, saved-filter serialization, chronological snapshot merging, authentication/paging/cancellation/environment races, and WPF details visibility/reopening.

Schema reference: [Microsoft's Plugin Trace Log Web API reference](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/webapi/reference/plugintracelog?view=dataverse-latest).

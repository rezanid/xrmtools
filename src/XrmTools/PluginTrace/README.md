# Plugin Trace Explorer

## Implementation plan

1. Introduce replaceable boundaries for Dataverse access, saved-view storage, source navigation and UI dispatch.
2. Separate editable filters, selected-record details and logging presentation from query/lifecycle orchestration.
3. Bind the existing XAML to properties and commands. Keep viewport, focus, text selection and hosted editor lifetime in the view.
4. Validate asynchronous workflows without WPF, then validate the bindings and visual interactions with focused WPF tests and a Visual Studio smoke test.

## Responsibilities

- `TraceExplorerViewModel` owns the active environment, applied filter, results, selection, saved views, investigation history and query/navigation cancellation.
- `TraceFilterViewModel` owns the editable draft, date parsing, validation preview and derived filter visibility. It creates independent `TraceFilter` snapshots.
- `TraceDetailViewModel` owns the selected immutable snapshot, full-record request, updated-record notice and detail cancellation.
- `TraceLoggingViewModel` distinguishes a user's logging option from Dataverse's confirmed mode. It owns logging requests and derives polling eligibility and countdown presentation.
- `TraceExplorerControl` owns WPF lifecycle notifications, timer ticks, sorting, selection/viewport preservation, popup closure, keyboard focus, find/wrap behavior and editor disposal. It does not query Dataverse or persist settings.
- `ITraceExplorerService`, `ITraceViewStore`, `ITraceNavigator` and `ITraceDispatcher` isolate external dependencies. Production adapters use existing VS settings, navigation and dispatcher APIs.
- `TraceLoggingLeaseManager` remains owned by the shared service, independently of the view and view model. Closing or unloading the view does not cancel the timed lease.

## State and lifetime rules

The view activates/deactivates its view model on WPF load/unload. Activation reads saved views and the current environment and logging configuration; deactivation cancels requests and unsubscribes from service events. Disposal is idempotent.

Service notifications may arrive on background threads. The dispatcher adapter marshals them to the UI; environment matching and active-session checks reject irrelevant notifications. Query generations and cancellation reject late responses, including responses from a transport that ignores cancellation.

Draft and applied filters remain separate. Auto-refresh uses the applied snapshot. An explicit Apply replaces the investigation; Refresh preserves the selected snapshot and viewport. Related execution and Back preserve the prior draft, results and full record. Back reuses completed detail data, and restarts a detail load only if that load was interrupted. Reopening the view also resumes an interrupted detail load.

Automatic polling requires an active visible view, enabled auto-refresh, a confirmed non-Off logging mode, an applied filter and no running query. Polling never extends a timed lease. Manual Refresh extends it to one hour from the manual action, as before.

Timed logging restores Off after an original Off or All setting, and Exceptions after an original Exceptions setting. Restarting a matching lease preserves its original restore target. Expiry is best effort and retains the existing persisted recovery/retry behavior.

Recovery uses a service-owned 30-second periodic pulse and checks the UTC deadline on each pulse. Sleeping pauses timer callbacks, but does not extend the UTC lease: the first pulse after waking can restore an overdue lease. Retry deadlines are also UTC-based, with delays of 30, 60, 120, 240 and then at most 300 seconds. Startup immediately checks persisted overdue leases, even if a retry was scheduled by the previous process. Concurrent pulses do not queue duplicate restorations. Recovery requests are non-interactive and use a two-minute cancellation timeout.

The persisted lease records the last attempt, failure count, error and next retry time. The expired countdown tooltip displays these diagnostics; restoration attempts, successes, failures and abandoned recoveries also appear in Visual Studio's Activity Log under `XrmTools.PluginTrace`. Older saved leases remain readable without the new diagnostic fields.

## Verification

Headless view-model tests cover polling, cancellation, environment switches, detail snapshots, related executions, storage failures and logging notifications. WPF tests cover bindings, selection reopening and theme resources. Service and core tests retain transport, filter and snapshot coverage.

Before shipping, test in a Visual Studio experimental instance connected to a development environment:

1. Edit quick/custom/advanced filters, save and overwrite a view, delete it, and use Apply/Refresh/Cancel via button and keyboard.
2. Refresh while a row is selected and the grid is sorted/scrolled. Check that the selected detail and viewport stay stable, and that Load updated trace changes only on request.
3. Open related execution and return with Back. Verify draft, selected record, full record and viewport restoration.
4. Exercise Off/Exceptions/All/timed logging. Check expiry selection/countdown, restoration targets, manual extension and paused auto-refresh.
5. Hide/reopen the window and switch environments during requests. Confirm that old results cannot appear under the new environment label and timed recovery continues independently of the window.
6. Verify find/wrap, splitter width, F12 navigation, full-record syntax highlighting and light/dark themes.
7. Enable timed logging, sleep the laptop across the UTC deadline, then wake it. Once networking/authentication is available, check that restoration begins on a short pulse rather than waiting for the remaining hour of awake time. If recovery fails, hover over Restore pending to inspect the last error and retry deadline; restoration must work without reopening the window or restarting VS.

#nullable enable
namespace XrmTools.DataverseSolutions;

using System.ComponentModel.Composition;
using System.Threading;

// Shared across solution commands and project creation, including PAC authentication.
[Export]
[PartCreationPolicy(CreationPolicy.Shared)]
internal sealed class DataverseSolutionOperationState
{
    private int _isBusy;

    public bool IsBusy => Volatile.Read(ref _isBusy) != 0;

    public bool TryEnter() => Interlocked.CompareExchange(ref _isBusy, 1, 0) == 0;

    public void Exit() => Interlocked.Exchange(ref _isBusy, 0);
}
#nullable restore

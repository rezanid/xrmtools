#nullable enable
namespace XrmTools.DataverseSolutions;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class PacVersionResult
{
    public string RawText { get; init; } = string.Empty;

    public string? Version { get; init; }
}

public sealed class PacAuthProfile
{
    public int Index { get; init; }

    public bool IsActive { get; init; }

    public string? Kind { get; init; }

    public string? Name { get; init; }

    public string? User { get; init; }

    public string? Cloud { get; init; }

    public string? Type { get; init; }

    public string? EnvironmentName { get; init; }

    public string? EnvironmentUrl { get; init; }
}

public sealed class PacAuthCreateRequest
{
    public string ProfileName { get; init; } = string.Empty;

    public string EnvironmentUrl { get; init; } = string.Empty;

    public string? UserName { get; init; }

    public string? Password { get; init; }

    public string? ApplicationId { get; init; }

    public string? ClientSecret { get; init; }

    public string? CertificateDiskPath { get; init; }

    public string? CertificatePassword { get; init; }

    public string? TenantId { get; init; }

    public bool UseDeviceCode { get; init; }
}

public interface IPacCli
{
    Task<PacVersionResult> GetVersionAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<PacAuthProfile>> ListAuthProfilesAsync(CancellationToken cancellationToken);

    Task SelectAuthProfileAsync(PacAuthProfile profile, CancellationToken cancellationToken);

    Task CreateAuthProfileAsync(PacAuthCreateRequest request, CancellationToken cancellationToken);

    Task<ProcessCommandResult> RunSolutionCommandAsync(
        CdsProjectInfo project,
        IReadOnlyList<string> arguments,
        IProgress<ProcessOutputLine> output,
        CancellationToken cancellationToken);
}

internal sealed class PacCliNotFoundException : InvalidOperationException
{
    public PacCliNotFoundException()
        : base("Power Platform CLI (pac) was not found. Install PAC CLI and ensure pac.exe is available on PATH.")
    {
    }
}
#nullable restore

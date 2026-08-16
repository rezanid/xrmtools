#nullable enable
namespace XrmTools.WebApi;

public sealed class EmptyResponse
{
    public static EmptyResponse Instance { get; } = new();

    private EmptyResponse()
    {
    }
}
#nullable restore

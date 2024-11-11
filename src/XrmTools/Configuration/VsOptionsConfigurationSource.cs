namespace XrmGen.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

internal class VsOptionsConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new VsOptionsConfigurationProvider();
}

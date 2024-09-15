namespace XrmGen.Configuration;

using Microsoft.Extensions.Configuration;

internal class VsOptionsConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new VsOptionsConfigurationProvider();
}

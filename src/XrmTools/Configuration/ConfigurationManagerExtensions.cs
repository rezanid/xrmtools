namespace XrmGen.Configuration;

using Microsoft.Extensions.Configuration;

public static class ConfigurationManagerExtensions
{
    public static ConfigurationManager AddEntityConfiguration(
        this ConfigurationManager manager)
    {
        IConfigurationBuilder configBuilder = manager;
        configBuilder.Add(new VsOptionsConfigurationSource());

        return manager;
    }
}

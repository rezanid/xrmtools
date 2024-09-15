#nullable enable
namespace XrmGen.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using XrmGen.Options;

internal class VsOptionsConfigurationProvider: ConfigurationProvider, IDisposable
{
    private readonly ILogger<VsOptionsConfigurationProvider> logger;
    private readonly VsOptionsConfigurationSource source;

    public VsOptionsConfigurationProvider(
        VsOptionsConfigurationSource source, ILogger<VsOptionsConfigurationProvider> logger)
    {
        this.logger = logger;
        this.source = source;
        GeneralOptions.Instance.OptionsChanged += OnOptionsChange;
    }


    public override void Load()
    {
        Data = new Dictionary<string, string?>
        {
            ["Logging:LogLevel:Default"] = GeneralOptions.Instance.LogLevel.ToString(),
        };
        logger.LogDebug("Configuration (re)loaded.");
    }

    private void OnOptionsChange(object sender, EventArgs e) => Load();

    public void Dispose()
    {
        GeneralOptions.Instance.OptionsChanged -= OnOptionsChange;
    }
}
#nullable restore
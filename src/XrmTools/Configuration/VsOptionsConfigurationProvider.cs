#nullable enable
namespace XrmGen.Configuration;

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using XrmGen.Options;

internal class VsOptionsConfigurationProvider: ConfigurationProvider, IDisposable
{
    public VsOptionsConfigurationProvider()
    {
        GeneralOptions.Instance.OptionsChanged += OnOptionsChange;
    }

    public override void Load()
    {
        Data = new Dictionary<string, string?>
        {
            ["Logging:LogLevel:Default"] = GeneralOptions.Instance.LogLevel.ToString(),
        };
    }

    private void OnOptionsChange(object sender, EventArgs e) => Load();

    public void Dispose()
    {
        GeneralOptions.Instance.OptionsChanged -= OnOptionsChange;
    }
}
#nullable restore
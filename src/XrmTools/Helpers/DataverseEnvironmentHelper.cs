#nullable enable
namespace XrmGen.Helpers;

using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmGen.Options;

internal class DataverseEnvironmentHelper
{
    public static async Task<DataverseEnvironment?> GetActiveEnvironmentAsync()
    {
        if (GeneralOptions.Instance.EnvironmentSettingLevel == EnvironmentSettingLevel.Options)
        {
            return GeneralOptions.Instance.CurrentEnvironment;
        }
        if (GeneralOptions.Instance.EnvironmentSettingLevel == EnvironmentSettingLevel.Solution)
        {
            return await GetEnvironmentFromVsSolutionAsync();
        }

        return null;
    }

    private static async Task<DataverseEnvironment?> GetEnvironmentFromVsSolutionAsync()
    {
        // Find EnvironmentUrl from the solution.
        var solution = await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(false);
        if (solution == null) return null;

        return null;
    }
}
#nullable restore
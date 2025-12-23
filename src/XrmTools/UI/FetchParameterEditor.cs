#nullable enable
namespace XrmTools.UI;

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using XrmTools.FetchXml.Model;

internal interface IFetchParameterEditor
{
    /// <summary>
    /// Shows the Parameter Editor dialog and returns the parameter values if the user confirmed.
    /// </summary>
    /// <param name="parameters">The list of parameters to edit</param>
    /// <returns>A dictionary of parameter names to values if confirmed, null if cancelled</returns>
    Task<Dictionary<string, string>?> EditParametersAsync(List<FetchParameter> parameters);
}

[Export(typeof(IFetchParameterEditor))]
internal class FetchParameterEditor : IFetchParameterEditor
{
    public async Task<Dictionary<string, string>?> EditParametersAsync(List<FetchParameter> parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var viewModel = new FetchParameterEditorViewModel(parameters);

        var dialog = new FetchParameterEditorDialog
        {
            DataContext = viewModel
        };

        var result = dialog.ShowDialog();
        
        if (result == true)
        {
            // Check if all required parameters have values
            if (!viewModel.AllRequiredParametersHaveValues())
            {
                return null;
            }

            // Build the result dictionary
            var parameterValues = new Dictionary<string, string>();
            foreach (var param in viewModel.Parameters)
            {
                parameterValues[param.Name] = param.Value ?? param.DefaultValue ?? string.Empty;
            }

            return parameterValues;
        }

        return null;
    }
}
#nullable restore

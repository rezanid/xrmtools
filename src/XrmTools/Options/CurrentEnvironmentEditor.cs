#nullable enable
namespace XrmTools.Options;
using System;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Crm.Sdk.Messages;
using XrmTools.Resources;

public class CurrentEnvironmentEditor : UITypeEditor
{
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
        // Indicate that this editor will display a drop-down list.
        return UITypeEditorEditStyle.DropDown;
    }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
        var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

        if (editorService != null && context.Instance is GeneralOptions options)
        {
            // Create a panel to hold the dropdown and button
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true
            };
            // Set column widths: first column (ListBox) takes the remaining space, second column (Button) auto-sizes
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // Create a ListBox to show the environments
            var listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                SelectionMode = SelectionMode.One,
                BorderStyle = BorderStyle.None
            };

            // Add the environments to the ListBox
            foreach (var env in options.Environments)
            {
                listBox.Items.Add(env);
            }

            // Pre-select the current environment in the list
            listBox.SelectedItem = value;

            // Create the "Test" button
            var testButton = new Button
            {
                Text = "Test",
                AutoSize = true,
                Padding = new Padding(5, 0, 5, 0)
            };

            // Add the List and Button to the panel
            panel.Controls.Add(listBox, 0, 0);
            panel.Controls.Add(testButton, 1, 0);

            // Subscribe to the "Test" button click event
            testButton.Click += (sender, e) =>
            {
                var selectedEnvironment = (DataverseEnvironment)listBox.SelectedItem;
                TestEnvironment(selectedEnvironment);
            };

            // Show the panel as a dropdown in the PropertyGrid
            editorService.DropDownControl(panel);

            // Return the selected value after the dropdown is closed
            return listBox.SelectedItem;
        }
        return value;
    }

    private void TestEnvironment(DataverseEnvironment environment)
    {
        var (success, message) = TestConnection(environment);

        if (success)
        {
            _ = VS.MessageBox.Show("Test Successful", message, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }
        else
        {
            _ = VS.MessageBox.Show("Test Failed", message, OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }
    }

    private (bool, string) TestConnection(DataverseEnvironment environment)
    {
        if (!environment.IsValid) return (false, string.Format(Strings.EnvironmentConnectionStringError, environment.Name));
        try
        {
            using var client = new ServiceClient(environment.ConnectionString);
            var response = client.Execute(new WhoAmIRequest());
        }
        catch (Exception ex)
        {
            return (false, string.Format(Strings.EnvironmentConnectionError, environment, ex));
        }
        return (true, string.Format(Strings.EnvironmentConnectionSuccess, environment));
    }
}
#nullable restore
#nullable enable
namespace XrmTools.Options;
using System;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Crm.Sdk.Messages;
using XrmTools.Resources;
using XrmTools.Xrm;
using XrmTools.Core;
using XrmTools.Xrm.Repositories;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System.Threading;

public class CurrentEnvironmentEditor : UITypeEditor
{
    [Import]
    internal IRepositoryFactory? RepositoryFactory { get; set; }

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

            // Create the "Test" button
            var testButton = new Button
            {
                Text = "Test",
                AutoSize = true,
                Padding = new Padding(5, 0, 5, 0),
            };

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

            listBox.SelectedIndexChanged += (sender, e) =>
            {
                testButton.Visible = listBox.SelectedItem != null;
            };
            // Pre-select the current environment in the list
            listBox.SelectedItem = value;

            testButton.Click += (sender, e) =>
            {
                var selectedEnvironment = (DataverseEnvironment)listBox.SelectedItem;
                TestEnvironment(selectedEnvironment);
            };

            panel.Controls.Add(listBox, 0, 0);
            panel.Controls.Add(testButton, 1, 0);

            // Show the panel as a dropdown in the PropertyGrid
            editorService.DropDownControl(panel);

            // Return the selected value after the dropdown is closed
            return listBox.SelectedItem;
        }
        return value;
    }

    private void TestEnvironment(DataverseEnvironment environment)
    {
        if (RepositoryFactory is null)
        {
            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            if (componentModel == null)
            {
                _ = VS.MessageBox.Show("Testing connection not available", "Testing connection is not available currently, please try again later.", OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK);
                return;
            }
            componentModel.DefaultCompositionService.SatisfyImportsOnce(this);
        }

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
        if (RepositoryFactory == null) return (false, "Testing connection is not available currently, please try again later.");
        if (environment is null) return (false, "Environment is not selected. Please select an environment first.");
        if (!environment.IsValid) return (false, string.Format(Strings.EnvironmentConnectionStringError, environment.Name));
        try
        {
            using var repo = ThreadHelper.JoinableTaskFactory.Run(async () => await RepositoryFactory.CreateRepositoryAsync<ISystemRepository>(environment));
            var response = ThreadHelper.JoinableTaskFactory.Run(async () => await repo.WhoAmIAsync(CancellationToken.None));
        }
        catch (Exception ex)
        {
            return (false, string.Format(Strings.EnvironmentConnectionError, environment, ex));
        }
        return (true, string.Format(Strings.EnvironmentConnectionSuccess, environment));
    }
}
#nullable restore
namespace XrmTools.Tests.PluginTrace;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using XrmTools.Shell.Styles;
using Moq;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.OData;
using XrmTools.PluginTrace;
using Xunit;

public class TraceExplorerControlTests
{
    [Fact]
    public Task ShellControlsResolveStylesAndRespondToThemeChanges() => OnStaThread(() =>
    {
        var panel = new StackPanel();
        panel.Resources.MergedDictionaries.Add(DefaultStyles.Instance);
        panel.Resources[ShellColors.ControlFillDefaultBrushKey] = Brushes.DimGray;
        panel.Resources[ShellColors.TextFillPrimaryBrushKey] = Brushes.White;
        var combo = new XrmTools.Shell.Controls.ComboBox { Width = 200, IsEditable = true };
        var check = new XrmTools.Shell.Controls.CheckBox { Content = "Auto-refresh" };
        panel.Children.Add(combo);
        panel.Children.Add(check);
        combo.Items.Add("Last 30 minutes");
        combo.SelectedIndex = 0;
        combo.Style = (Style)DefaultStyles.Instance[typeof(XrmTools.Shell.Controls.ComboBox)];
        check.Style = (Style)DefaultStyles.Instance[typeof(XrmTools.Shell.Controls.CheckBox)];
        Assert.NotNull(combo.Template);
        Assert.NotNull(check.Template);
        Assert.Same(Brushes.DimGray, combo.Background);
        Assert.Same(Brushes.White, check.Foreground);
        panel.Resources[ShellColors.ControlFillDefaultBrushKey] = Brushes.White;
        panel.Resources[ShellColors.TextFillPrimaryBrushKey] = Brushes.Black;
        Assert.Same(Brushes.White, combo.Background);
        Assert.Same(Brushes.Black, check.Foreground);
        Assert.Equal("Last 30 minutes", combo.Text);
    });

    [Fact]
    public Task DetailsStartClosedAndCanCloseAndReopenTheSameRow() => OnStaThread(() =>
    {
        var service = new TraceExplorerService(Mock.Of<IEnvironmentSelection>(), Mock.Of<IXrmHttpClientFactory>(), Mock.Of<IODataTransport>());
        using var control = new TraceExplorerControl(service);
        Assert.Equal(Visibility.Collapsed, control.Details.Visibility);
        Assert.Equal(0, control.DetailColumn.Width.Value);
        Assert.True(control.Logs.CanUserSortColumns);

        using var json = JsonDocument.Parse("{\"plugintracelogid\":\"00000001-0000-0000-0000-000000000000\",\"createdon\":\"2026-09-22T12:00:00Z\",\"messageblock\":\"hello\"}");
        var record = new TraceRecord(json.RootElement);
        control.Logs.ItemsSource = new[] { record };
        control.Logs.SelectedItem = record;
        Assert.Equal(Visibility.Visible, control.Details.Visibility);
        Assert.Equal("hello", control.MessageText.Text);

        control.CloseDetailsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(Visibility.Collapsed, control.Details.Visibility);
        Assert.Null(control.Logs.SelectedItem);
        Assert.Single(control.Logs.Items);

        control.Logs.SelectedItem = record;
        Assert.Equal(Visibility.Visible, control.Details.Visibility);
        Assert.Equal("hello", control.MessageText.Text);
    });

    private static Task OnStaThread(Action action)
    {
        var completion = new TaskCompletionSource<bool>();
        var thread = new Thread(() =>
        {
            try { action(); completion.SetResult(true); }
            catch (Exception ex) { completion.SetException(ex); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }
}

namespace XrmTools.Tests.PluginTrace;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

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
    public Task SavedViewBindingsPreserveSelectionAfterAnOverwrite() => OnStaThread(() =>
    {
        var service = new Mock<ITraceExplorerService>();
        var store = new Mock<ITraceViewStore>();
        var environment = new DataverseEnvironment { ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" };
        service.Setup(s => s.GetEnvironmentAsync()).ReturnsAsync(environment);
        service.Setup(s => s.InitializeTraceLoggingLeasesAsync()).Returns(Task.CompletedTask);
        service.Setup(s => s.GetTraceLoggingAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>(), true, true))
            .ReturnsAsync(new TraceLoggingConfiguration(Guid.NewGuid(), TraceLoggingMode.Off));
        service.Setup(s => s.QueryAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TraceQueryResult(Array.Empty<TraceRecord>(), false));
        var existing = new TraceFilter { Name = "Original" };
        store.Setup(s => s.LoadAsync()).ReturnsAsync(new[] { existing });
        store.Setup(s => s.SaveAsync(It.IsAny<System.Collections.Generic.IReadOnlyList<TraceFilter>>())).Returns(Task.CompletedTask);
        using var model = new TraceExplorerViewModel(service.Object, store.Object, Mock.Of<ITraceNavigator>(), new TraceDispatcher(System.Windows.Threading.Dispatcher.CurrentDispatcher));
        using var control = new TraceExplorerControl(model);
        var activation = model.ActivateAsync(); Assert.True(activation.IsCompleted); activation.GetAwaiter().GetResult();
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.DoesNotContain("Could not", model.Status);
        control.SavedViews.SelectedItem = existing;
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Same(existing, model.SelectedView);
        control.ViewName.Text = "Renamed";
        var save = model.SaveViewCommand.ExecuteAsync(null); Assert.True(save.IsCompleted); save.GetAwaiter().GetResult();
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal("Renamed", model.SavedViewLabel);
        Assert.Same(model.SelectedView, control.SavedViews.SelectedItem);
        Assert.Equal("Renamed", control.SavedViewsButton.Content);
        service.Verify(s => s.QueryAsync(It.Is<DataverseEnvironment>(e => e.ConnectionString == environment.ConnectionString), It.IsAny<TraceFilter>(), true, It.IsAny<CancellationToken>()), Times.Exactly(2));
    });

    [Fact]
    public Task FilterBindingsUpdateTheDraftAndDerivedPresentation() => OnStaThread(() =>
    {
        var service = new TraceExplorerService(Mock.Of<IEnvironmentSelection>(), Mock.Of<IXrmHttpClientFactory>(), Mock.Of<IODataTransport>());
        using var control = new TraceExplorerControl(service);
        var model = (TraceExplorerViewModel)control.DataContext;
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal("30", ((ComboBoxItem)control.Duration.SelectedItem).Tag);
        Assert.Equal("10", ((ComboBoxItem)control.Interval.SelectedItem).Tag);
        control.Duration.SelectedIndex = 5;
        control.Expression.Text = "depth gt 2";
        control.FullControl.IsChecked = true;
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(0, model.Filter.Minutes);
        Assert.Equal("depth gt 2", model.Filter.Expression);
        Assert.Equal("depth gt 2", control.Preview.Text);
        Assert.False(control.TypeName.IsEnabled);
        Assert.True(control.QueryButton.IsEnabled);
        Assert.Equal(Visibility.Collapsed, control.CustomRange.Visibility);
        control.FullControl.IsChecked = false;
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(Visibility.Visible, control.CustomRange.Visibility);
        model.IntervalSeconds = 60;
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal("60", ((ComboBoxItem)control.Interval.SelectedItem).Tag);
    });

    [Fact]
    public Task ConfirmedRestorationUpdatesTheDropdownOnTheDispatcher() => OnStaThread(() =>
    {
        var service = new TraceExplorerService(Mock.Of<IEnvironmentSelection>(), Mock.Of<IXrmHttpClientFactory>(), Mock.Of<IODataTransport>());
        using var control = new TraceExplorerControl(service);
        var environment = new DataverseEnvironment { ConnectionString = "Url=https://contoso.crm.dynamics.com" };
        var viewModel = (TraceExplorerViewModel)control.DataContext;
        viewModel.Logging.SetEnvironment(environment);
        viewModel.Logging.Confirm(environment, new TraceLoggingConfiguration(Guid.NewGuid(), TraceLoggingMode.All));
        viewModel.Logging.Confirm(environment, new TraceLoggingConfiguration(Guid.NewGuid(), TraceLoggingMode.Off));
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(TraceLoggingMode.Off, ((TraceLoggingOption)control.TraceLogging.SelectedItem).Mode);
        Assert.Contains("paused", (string)control.AutoRefresh.ToolTip);
        Assert.True(control.AutoRefresh.IsChecked);

        viewModel.Logging.Confirm(environment with { ConnectionString = "Url=https://other.crm.dynamics.com" },
            new TraceLoggingConfiguration(Guid.NewGuid(), TraceLoggingMode.All));
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(TraceLoggingMode.Off, ((TraceLoggingOption)control.TraceLogging.SelectedItem).Mode);
    });

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
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(Visibility.Collapsed, control.Details.Visibility);
        Assert.Equal(0, control.DetailColumn.Width.Value);
        Assert.True(control.Logs.CanUserSortColumns);
        Assert.Equal("Apply", control.QueryButton.Content);

        using var json = JsonDocument.Parse("{\"plugintracelogid\":\"00000001-0000-0000-0000-000000000000\",\"createdon\":\"2026-09-22T12:00:00Z\",\"messageblock\":\"hello\"}");
        var record = new TraceRecord(json.RootElement);
        control.Logs.ItemsSource = new[] { record };
        control.Logs.SelectedItem = record;
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(Visibility.Visible, control.Details.Visibility);
        Assert.Equal("hello", control.MessageText.Text);

        control.CloseDetailsButton.Command.Execute(null);
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(Visibility.Collapsed, control.Details.Visibility);
        Assert.Null(control.Logs.SelectedItem);
        Assert.Single(control.Logs.Items);

        control.Logs.SelectedItem = record;
        control.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
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

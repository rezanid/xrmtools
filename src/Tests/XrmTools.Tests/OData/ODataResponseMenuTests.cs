namespace XrmTools.Tests.OData;

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Moq;
using XrmTools.OData;
using Xunit;
using ShellContextMenu = XrmTools.Shell.Controls.ContextMenu;
using ShellMenuItem = XrmTools.Shell.Controls.MenuItem;

public class ODataResponseMenuTests
{
    [Fact]
    public Task TextResultMenuUsesShellThemeAndTargetsItsReadOnlyControl() => OnStaThread(() =>
    {
        var text = ODataResponseMargin.ResponseText();
        Assert.True(text.IsReadOnly);
        AssertClipboardMenu(text);
        text.Text = "response";
        var menu = (ShellContextMenu)text.ContextMenu;
        var selectAll = (ShellMenuItem)menu.Items[1];
        ApplicationCommands.SelectAll.Execute(null, selectAll.CommandTarget);
        Assert.Equal("response", text.SelectedText);
    });

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public Task LayoutHostsMoveTheSameContentAndPreserveState(bool initialRight, bool rightFirst) => OnStaThread(() =>
    {
        var content = new TextBox { Text = "response", Visibility = Visibility.Visible };
        bool configuredRight = initialRight;
        var placement = new ODataMarginPlacement(content, initialRight, right => configuredRight = right);
        var first = new ODataMarginHost(placement, rightFirst);
        var second = new ODataMarginHost(placement, !rightFirst);
        var bottom = rightFirst ? second : first;
        var right = rightFirst ? first : second;
        Assert.Same(initialRight ? right : bottom, content.Parent);
        Assert.Equal(initialRight ? Visibility.Visible : Visibility.Collapsed, right.Visibility);
        Assert.Equal(initialRight ? Visibility.Collapsed : Visibility.Visible, bottom.Visibility);
        placement.Toggle();
        Assert.Equal(!initialRight, configuredRight);
        Assert.Same(initialRight ? bottom : right, content.Parent);
        Assert.Equal("response", content.Text);
        Assert.Equal(Visibility.Visible, content.Visibility);
        content.Visibility = Visibility.Collapsed;
        placement.Toggle();
        Assert.Same(initialRight ? right : bottom, content.Parent);
        Assert.Equal(Visibility.Collapsed, content.Visibility);
        Assert.Equal(initialRight, configuredRight);
        right.Dispose();
        Assert.False(placement.CanSwitch);
        placement.Toggle();
        Assert.Equal(initialRight, placement.IsRight);
        bottom.Dispose();
    });

    [Fact]
    public Task LayoutMenuUsesShellThemeAndReflectsCurrentPosition() => OnStaThread(() =>
    {
        bool right = false;
        bool canSwitch = true;
        var menu = ODataResponseMargin.LayoutMenu(() => right, () => canSwitch, () => right = !right);
        Assert.IsType<ShellContextMenu>(menu);
        var item = Assert.IsType<ShellMenuItem>(Assert.Single(menu.Items));
        menu.RaiseEvent(new RoutedEventArgs(ContextMenu.OpenedEvent));
        Assert.Equal("Move response to side", item.Header);
        Assert.True(item.IsEnabled);
        item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        Assert.True(right);
        canSwitch = false;
        menu.RaiseEvent(new RoutedEventArgs(ContextMenu.OpenedEvent));
        Assert.Equal("Move response below document", item.Header);
        Assert.False(item.IsEnabled);
    });

    [Fact]
    public Task RunControlsUseShellThemedComponents() => OnStaThread(() =>
    {
        var grid = ODataResponseMargin.ResultTable();
        Assert.True(grid.IsReadOnly);
        Assert.False(grid.CanUserSortColumns);
        Assert.Equal(DataGridSelectionUnit.FullRow, grid.SelectionUnit);
        Assert.Equal(new[] { "", "Method", "Request", "Status", "Time" }, System.Linq.Enumerable.Select(grid.Columns, column => column.Header));
        Assert.IsType<DataGridTemplateColumn>(grid.Columns[0]);
        var dialog = new ODataRunConfirmation(new XrmTools.DataverseEnvironment
        {
            Name = "Development", ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test"
        }, 2, "POST /accounts");
        Assert.IsAssignableFrom<XrmTools.Shell.Controls.DialogWindow>(dialog);
        var content = Assert.IsType<StackPanel>(dialog.Content);
        var writeList = Assert.IsType<XrmTools.Shell.Controls.ScrollViewer>(content.Children[3]);
        Assert.Equal(240, writeList.MaxHeight);
        Assert.Equal("POST /accounts", Assert.IsType<XrmTools.Shell.Controls.TextBlock>(writeList.Content).Text);
        var footer = Assert.IsType<StackPanel>(dialog.FooterContent);
        Assert.Equal(XrmTools.Shell.Styles.ButtonKind.Accent, Assert.IsType<XrmTools.Shell.Controls.Button>(footer.Children[0]).Kind);
        Assert.True(Assert.IsType<XrmTools.Shell.Controls.Button>(footer.Children[1]).IsCancel);
    });

    [Fact]
    public void ToolbarSummaryKeepsRunTargetStatisticsAndStopReasonAcrossEnvironmentChanges()
    {
        var dev = new XrmTools.DataverseEnvironment { Name = "Dev", ConnectionString = "Url=https://dev.crm.dynamics.com;TenantId=test" };
        var test = new XrmTools.DataverseEnvironment { Name = "Test", ConnectionString = "Url=https://test.crm.dynamics.com;TenantId=test" };
        var run = new ODataExecutionResult { IsDocument = true, Environment = dev, StopReason = "Canceled.", Rows = new[]
        {
            new ODataExecutionRow { State = "Succeeded" }, new ODataExecutionRow { State = "Failed" },
            new ODataExecutionRow { State = "Skipped" }, new ODataExecutionRow { State = "Running" }
        } };
        string same = ODataResponseMargin.EnvironmentLabel(dev, false, false, null, null, run);
        Assert.StartsWith("Dev — 3/4 finished · 1 succeeded · 1 failed · 1 skipped · Canceled.", same);
        Assert.Contains("https://dev.crm.dynamics.com", same);
        Assert.DoesNotContain("Run target:", same);
        string switched = ODataResponseMargin.EnvironmentLabel(test, false, false, null, "Missing profile", run);
        Assert.StartsWith("Test · Run target: Dev — 3/4 finished", switched);
        Assert.Contains("https://test.crm.dynamics.com", switched);
        Assert.Contains("Run URL: https://dev.crm.dynamics.com", switched);
        Assert.Contains("No matching variable profile", switched);
        string unavailable = ODataResponseMargin.EnvironmentLabel(null, true, false, null, null, run);
        Assert.Contains("Environment unavailable — refresh to try again", unavailable);
        Assert.Contains("Run target: Dev", unavailable);
        string manual = ODataResponseMargin.EnvironmentLabel(dev, false, false, null, null, new ODataExecutionResult());
        Assert.DoesNotContain("finished", manual);
    }

    [Fact]
    public void ResultRowsExposeStatusMonikersAndText()
    {
        var row = new ODataExecutionRow { Request = new ODataRequest { Method = "POST", Name = "Create" }, State = "Succeeded" };
        var item = new ODataResponseMargin.ResultItem(row);
        Assert.Equal("POST", item.Method);
        Assert.Equal("Create", item.Name);
        Assert.Equal(Visibility.Visible, item.IconVisibility);
        Assert.Equal(Microsoft.VisualStudio.Imaging.KnownMonikers.StatusOK, item.Icon);
        row.State = "Failed";
        Assert.Equal(Microsoft.VisualStudio.Imaging.KnownMonikers.StatusError, item.Icon);
        Assert.Equal("Failed", item.Outcome);
        row.State = "Skipped";
        Assert.Equal(Visibility.Collapsed, item.IconVisibility);
        Assert.Equal("--", item.Elapsed);
    }

    [Fact]
    public async Task SaveBodyReportsFailureAndKeepsResponseAvailableForRetry()
    {
        string source = Path.Combine(Path.GetTempPath(), "odata-save-" + Guid.NewGuid().ToString("N"));
        string destination = source + ".saved";
        File.WriteAllText(source, "response body");
        var stored = new ODataStoredBody(source, Encoding.UTF8);
        string error = null;
        try
        {
            await ODataResponseMargin.SaveBodyAsync(stored, Path.Combine(source + ".missing", "response"), message =>
            {
                error = message;
                return Task.CompletedTask;
            });
            Assert.StartsWith("Could not save the response: ", error);
            Assert.True(File.Exists(source));
            error = null;
            await ODataResponseMargin.SaveBodyAsync(stored, destination, message =>
            {
                error = message;
                return Task.CompletedTask;
            });
            Assert.Null(error);
            Assert.Equal("response body", File.ReadAllText(destination));
        }
        finally
        {
            stored.Release();
            if (File.Exists(destination)) File.Delete(destination);
        }
    }

    [Fact]
    public Task HeaderResultMenuUsesShellThemeAndPreservesGridClipboardBehavior() => OnStaThread(() =>
    {
        var grid = ODataResponseMargin.ResponseHeaders();
        Assert.True(grid.IsReadOnly);
        Assert.Equal(DataGridClipboardCopyMode.IncludeHeader, grid.ClipboardCopyMode);
        Assert.Equal(DataGridSelectionMode.Single, grid.SelectionMode);
        AssertClipboardMenu(grid);
    });

    [Fact]
    public Task JsonResultMenuUsesShellThemeAndExistingEditorOperations() => OnStaThread(() =>
    {
        var element = new Grid();
        var view = new Mock<IWpfTextView>();
        var selection = new Mock<ITextSelection>();
        selection.SetupGet(s => s.IsEmpty).Returns(true);
        view.SetupGet(v => v.VisualElement).Returns(element);
        view.SetupGet(v => v.Selection).Returns(selection.Object);
        var operations = new Mock<IEditorOperations>();
        var factory = new Mock<IEditorOperationsFactoryService>();
        factory.Setup(f => f.GetEditorOperations(view.Object)).Returns(operations.Object);
        ODataResponseMargin.EnableJsonCopy(view.Object, factory.Object);
        var menu = Assert.IsType<ShellContextMenu>(element.ContextMenu);
        Assert.Equal(2, menu.Items.Count);
        var copy = Assert.IsType<ShellMenuItem>(menu.Items[0]);
        var selectAll = Assert.IsType<ShellMenuItem>(menu.Items[1]);
        menu.RaiseEvent(new RoutedEventArgs(ContextMenu.OpenedEvent));
        Assert.False(copy.IsEnabled);
        selection.SetupGet(s => s.IsEmpty).Returns(false);
        menu.RaiseEvent(new RoutedEventArgs(ContextMenu.OpenedEvent));
        Assert.True(copy.IsEnabled);
        copy.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        selectAll.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        operations.Verify(o => o.CopySelection(), Times.Once);
        operations.Verify(o => o.SelectAll(), Times.Once);
    });

    private static void AssertClipboardMenu(Control target)
    {
        var menu = Assert.IsType<ShellContextMenu>(target.ContextMenu);
        Assert.Equal(2, menu.Items.Count);
        var copy = Assert.IsType<ShellMenuItem>(menu.Items[0]);
        var selectAll = Assert.IsType<ShellMenuItem>(menu.Items[1]);
        Assert.Same(ApplicationCommands.Copy, copy.Command);
        Assert.Same(ApplicationCommands.SelectAll, selectAll.Command);
        Assert.Same(target, copy.CommandTarget);
        Assert.Same(target, selectAll.CommandTarget);
        Assert.Equal("Ctrl+C", copy.InputGestureText);
        Assert.Equal("Ctrl+A", selectAll.InputGestureText);
    }

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

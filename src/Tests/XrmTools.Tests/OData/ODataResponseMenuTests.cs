namespace XrmTools.Tests.OData;

using System;
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

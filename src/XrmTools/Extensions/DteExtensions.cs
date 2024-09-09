#nullable enable
namespace XrmGen.Extensions;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;

internal static class DteExtensions
{
    public static void ExecuteCommandIfAvailable(this DTE2 dte, string commandName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        Command command;

        try
        {
            command = dte.Commands.Item(commandName);
        }
        catch (ArgumentException)
        {
            return;
        }

        if (command.IsAvailable)
        {
            dte.ExecuteCommand(commandName);
        }
    }
}
#nullable restore
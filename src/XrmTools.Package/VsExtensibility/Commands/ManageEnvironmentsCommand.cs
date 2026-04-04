namespace XrmTools.VsExtensibility.Commands;

using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;

[VisualStudioContribution]
internal class ManageEnvironmentsCommand : Command
{
    private readonly TraceSource logger;

    public ManageEnvironmentsCommand(TraceSource traceSource)
    {
        logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new("%XrmTools.Package.ManageEnvironmentsCommand.DisplayName%")
    {
        // Use this object initializer to set optional parameters for the command. The required parameter,
        // displayName, is set above. DisplayName is localized and references an entry in .vsextension\string-resources.json.
        Icon = new(ImageMoniker.Custom("Dataverse"), IconSettings.IconAndText),
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
    };

    [VisualStudioContribution]
    public static ToolbarConfiguration ToolbarConfiguration => new("%XrmTools.Package.ManageEnvironmentToolbar%")
    {
        Children = [ToolbarChild.Group(GroupChild.Combo<ManageEnvironmentsCommand>())]
    };

    /// <inheritdoc />
    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Use InitializeAsync for any one-time setup or initialization.
        return base.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        await Extensibility.Shell().ShowPromptAsync("Hello from an extension!", PromptOptions.OK, cancellationToken);
    }
}

#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Scriban;
using Scriban.Runtime;
using System;
using System.ComponentModel.Composition;
using System.Threading;

/// <summary>A stateless renderer. Existing Custom Tools retain their current renderer and configuration.</summary>
[Export(typeof(ScribanClientRenderer))]
internal sealed class ScribanClientRenderer
{
    public string Render(string templateText, string templateName, object model, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var template = Template.Parse(templateText, templateName);
        if (template.HasErrors)
            throw new InvalidOperationException("Client template could not be parsed: " + string.Join(Environment.NewLine, template.Messages));
        var globals = new ScriptObject { { "model", model } };
        var context = new TemplateContext { StrictVariables = true, LoopLimit = 10000 };
        context.PushGlobal(globals);
        var output = template.Render(context);
        cancellationToken.ThrowIfCancellationRequested();
        return output;
    }
}

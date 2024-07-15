using Microsoft.JSON.Core.Parser;
using Microsoft.JSON.Editor.Completion;
using Microsoft.JSON.Editor.Completion.Def;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmGen;


public abstract class CompletionProviderBase : IJSONCompletionListProvider
{
    public abstract JSONCompletionContextType ContextType { get; }

    public abstract string SupportedFileExtension { get; }

    public IEnumerable<JSONCompletionEntry> GetListEntries(JSONCompletionContext context)
    {
        if (!Validation.IsSupportedFile(SupportedFileExtension)) return [];
        return GetEntries(context);
    }

    protected abstract IEnumerable<JSONCompletionEntry> GetEntries(JSONCompletionContext context);

    protected JSONMember GetMember(JSONCompletionContext context)
    {
        JSONMember member = context.ContextItem.FindType<JSONMember>();
        JSONMember parent = member.Parent.FindType<JSONMember>();

        //if (parent == null || !parent.UnquotedNameText.EndsWith("dependencies", StringComparison.OrdinalIgnoreCase))
        //    return null;

        if (parent == null || !parent.UnquotedNameText.Equals("Steps", StringComparison.OrdinalIgnoreCase))
            return null;

        if (member.UnquotedNameText.Length == 0)
            return null;

        if (!member.UnquotedNameText.Equals("PrimaryEntityName", StringComparison.OrdinalIgnoreCase))
            return null;

        return member;
    }
}
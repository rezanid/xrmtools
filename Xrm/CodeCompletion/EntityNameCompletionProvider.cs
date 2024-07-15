using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSON.Core.Parser;
using Microsoft.JSON.Editor.Completion;
using Microsoft.JSON.Editor.Completion.Def;
using Microsoft.VisualStudio.Utilities;

namespace XrmGen.Xrm.CodeCompletion;

[Export(typeof(IJSONCompletionListProvider))]
[Name("EntityNameCompletionProvider")]
public class EntityNameCompletionProvider : CompletionProviderBase
{
    public override JSONCompletionContextType ContextType
    {
        get { return JSONCompletionContextType.PropertyName; }
    }

    public override string SupportedFileExtension
    {
        get { return ".def.json"; }
    }

    protected override IEnumerable<JSONCompletionEntry> GetEntries(JSONCompletionContext context)
    {
        if (EntityNameCompletionEntry._searchResults != null)
        {
            foreach (string value in EntityNameCompletionEntry._searchResults)
            {
                yield return new EntityNameCompletionEntry(value, context.Session, null);
                //yield return new EntityNameCompletionEntry()
            }

            EntityNameCompletionEntry._searchResults = null;
        }
        else
        {
            JSONMember member = GetMember(context);

            if (member != null)
                yield return new EntityNameCompletionEntry(Resources.Strings.CompletionSearch, context.Session, member.JSONDocument);
        }
    }
}

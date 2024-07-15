using Microsoft.Extensions.Logging;
using Microsoft.JSON.Core.Parser;
using Microsoft.JSON.Editor.Completion;
using Microsoft.VisualStudio.Language.Intellisense;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using XrmGen._Core;
using XrmGen.Xrm;
using XrmGen.Extensions;
using Microsoft.VisualStudio.Shell;

namespace XrmGen.Xrm.CodeCompletion;

public class EntityNameCompletionEntry : JSONCompletionEntry
{
    private readonly JSONDocument _doc;
    internal static IEnumerable<string> _searchResults;

    public EntityNameCompletionEntry(string text, IIntellisenseSession session, JSONDocument doc)
        : base(text, "\"" + text + "\"", null, Constants.Icon, null, false, session as ICompletionSession)
    {
        _doc = doc;
    }

    //public EntityNameCompletionEntry(string text, string description, IIntellisenseSession session)
    //    : base(text, "\"" + text + "\"", description, Constants.Icon, null, false, session as ICompletionSession)
    //{ }

    public override void Commit()
    {
        if (_doc == null)
        {
            base.Commit();
        }
        else
        {
            string searchTerm = _doc.GetMemberName(base.Session);

            if (string.IsNullOrEmpty(searchTerm))
                return;

            ExecuteSearch(searchTerm);
        }
    }

    private void ExecuteSearch(string searchTerm)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        ThreadPool.QueueUserWorkItem(o =>
        {
            //string url = string.Format(Constants.SearchUrl, HttpUtility.UrlEncode(searchTerm));
            //string result = Helper.DownloadText(url);
            //var children = GetChildren(result);
            var schemaProvider = new XrmSchemaProvider(Constants.EnvironmentUrl, Constants.ApplicationId);
            var names = schemaProvider.GetEntityNames();

            if (!names.Any())
            {
                Helper.DTE.StatusBar.Text = string.Format(Resources.Strings.CompletionNoEntityFound, searchTerm);
                base.Session.Dismiss();
                return;
            }

            Helper.DTE.StatusBar.Text = string.Empty;
            _searchResults = names;

            //Helper.ExecuteCommand("Edit.CompleteWord");
            Helper.ExecuteCommand("Edit.ListMembers");
        });
    }
}

﻿using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmGen._Core;
using XrmGen.Xrm;

namespace AsyncCompletionSample.JsonElementCompletion
{
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [Name("Chemical element dictionary completion provider")]
    [ContentType("text")]
    class SampleCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        IDictionary<ITextView, IAsyncCompletionSource> cache = new Dictionary<ITextView, IAsyncCompletionSource>();

        [Import]
        IContentTypeRegistryService ContentTypeRegistry;

        [Import]
        ElementCatalog Catalog;

        [Import]
        ITextStructureNavigatorSelectorService StructureNavigatorSelector;

        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            if (cache.TryGetValue(textView, out var itemSource))
                return itemSource;

            Logger.Log(ContentTypeRegistry.ContentTypes.Select(ct => ct.DisplayName).Aggregate((a, b) => a + ", " + b));

            var source = new SampleCompletionSource(Catalog, StructureNavigatorSelector); // opportunity to pass in MEF parts
            textView.Closed += (o, e) => cache.Remove(textView); // clean up memory as files are closed
            cache.Add(textView, source);
            return source;
        }
    }
}
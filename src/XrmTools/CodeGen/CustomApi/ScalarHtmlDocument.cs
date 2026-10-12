#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

/// <summary>A companion page that reads the saved specification rather than embedding a snapshot.</summary>
internal static class ScalarHtmlDocument
{
    internal static GeneratedClient Create(string specificationFileName)
    {
        if (string.IsNullOrWhiteSpace(specificationFileName) || Path.GetFileName(specificationFileName) != specificationFileName)
            throw new ArgumentException("A specification filename without a directory is required.", nameof(specificationFileName));
        var url = JsonConvert.SerializeObject("./" + Uri.EscapeDataString(specificationFileName),
            new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
        var title = WebUtility.HtmlEncode(Path.GetFileNameWithoutExtension(specificationFileName) + " API Reference");
        var html = $$"""
            <!doctype html>
            <html lang="en">
              <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>{{title}}</title>
              </head>
              <body>
                <!-- Save beside the referenced specification and serve this folder over HTTP.
                     Scalar loads from its CDN; an internet connection is required. -->
                <div id="app"></div>
                <noscript>Enable JavaScript to view the API reference.</noscript>
                <script type="module">
                  import { createApiReference } from 'https://cdn.jsdelivr.net/npm/@scalar/api-reference/esm.js'

                  createApiReference('#app', {
                    url: {{url}},
                  })
                </script>
              </body>
            </html>
            """;
        return new GeneratedClient(Path.ChangeExtension(specificationFileName, ".html"), html);
    }
}

#nullable enable
namespace XrmTools.CodeGen.CustomApi;

using Newtonsoft.Json;
using System;
using System.IO;
using System.ComponentModel;
using System.Net;

public enum OpenApiHtmlMode
{
    [Description("Embedded specification")] Embedded,
    [Description("Linked specification (requires HTTP)")] Linked,
}

/// <summary>A Scalar companion page with an embedded snapshot or a relative specification URL.</summary>
internal static class ScalarHtmlDocument
{
    internal static GeneratedClient Create(GeneratedClient specification, OpenApiHtmlMode mode = OpenApiHtmlMode.Embedded)
    {
        var specificationFileName = specification.FileName;
        if (string.IsNullOrWhiteSpace(specificationFileName) || Path.GetFileName(specificationFileName) != specificationFileName)
            throw new ArgumentException("A specification filename without a directory is required.", nameof(specificationFileName));
        var value = mode switch
        {
            OpenApiHtmlMode.Embedded => specification.Content,
            OpenApiHtmlMode.Linked => "./" + Uri.EscapeDataString(specificationFileName),
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };
        var configuration = (mode == OpenApiHtmlMode.Embedded ? "content: " : "url: ") +
            JsonConvert.SerializeObject(value, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
        var instructions = mode == OpenApiHtmlMode.Embedded
            ? "Embedded specification snapshot. Regenerate this HTML after specification changes."
            : "Save beside the referenced specification and serve this folder over HTTP.";
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
                <!-- {{instructions}}
                     Scalar loads from its CDN; an internet connection is required. -->
                <div id="app"></div>
                <noscript>Enable JavaScript to view the API reference.</noscript>
                <script type="module">
                  import { createApiReference } from 'https://cdn.jsdelivr.net/npm/@scalar/api-reference/esm.js'

                  createApiReference('#app', {
                    {{configuration}},
                  })
                </script>
              </body>
            </html>
            """;
        return new GeneratedClient(Path.ChangeExtension(specificationFileName, ".html"), html);
    }
}

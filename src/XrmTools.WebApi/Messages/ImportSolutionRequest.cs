#nullable enable
namespace XrmTools.WebApi.Messages;

using Newtonsoft.Json;
using System;
using System.Net.Http;
using XrmTools.WebApi.Types;

/// <summary>
/// Contains the data to import a solution
/// </summary>
public sealed class ImportSolutionRequest : HttpRequestMessage
{

    public ImportSolutionRequest(ImportSolutionParameters parameters)
    {
        Method = HttpMethod.Post;
        RequestUri = new Uri(
            uriString: "ImportSolution",
            uriKind: UriKind.Relative);

        Content = new StringContent(
                content: JsonConvert.SerializeObject(parameters,Formatting.Indented),
                encoding: System.Text.Encoding.UTF8,
                mediaType: "application/json");
    }
}
#nullable restore
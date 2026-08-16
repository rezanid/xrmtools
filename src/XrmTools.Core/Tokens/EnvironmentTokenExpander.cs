#nullable enable
namespace XrmTools.Tokens;
using System;
using System.ComponentModel.Composition;

[Export(typeof(ITokenExpander))]
public class EnvironmentTokenExpander : ITokenExpander
{
    public bool CanExpand(string token) => token.StartsWith("env:");

    public string Expand(string token)
    {
        var name = token[4..];
        if (string.IsNullOrEmpty(name)) return token;
        return Environment.GetEnvironmentVariable(name) ?? token;
    }
}
#nullable restore
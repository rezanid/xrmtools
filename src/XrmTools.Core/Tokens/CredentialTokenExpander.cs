#nullable enable
namespace XrmTools.Tokens;

using System;

/// <summary>
/// Can replace tokens of the form {cred:target:key} with the value of the specified key in the specified credential target.
/// For example {cred:MyApp:username} would retrieve the username from the credential named "MyApp".
/// </summary>
public class CredentialTokenExpander(ICredentialManager credentialManager) : ITokenExpander
{
    public bool CanExpand(string token) => token.StartsWith("cred:");

    /// <summary>
    /// Example1: cred:myapp:username
    /// Example2: cred:git:https://github.com:username    /// </summary>
    /// <param name="token"></param>
    /// <returns>the value of credential in clear text.</returns>
    public string Expand(string token)
    {

        var parts = token[5..].Split([':'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return token;
        var target = string.Join(":", parts[0..^1]);
        if (string.IsNullOrEmpty(target)) return token;
        var key = parts[^1];

        return RetrieveCredential(target, key) ?? string.Empty;
    }
    
    /// <summary>
    /// Retrieve credentials from Windows Credential Manager
    /// </summary>
    private string? RetrieveCredential(string target, string key)
    {
        var (username, password) = credentialManager.ReadCredentials(target);
        if (username == null && password == null)
        {
            return null;
        }
        return key switch
        {
            "username" => username,
            "password" => password,
            _ => null
        };
    }
}
#nullable restore
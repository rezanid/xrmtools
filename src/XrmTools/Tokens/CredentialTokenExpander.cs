namespace XrmTools.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

internal class CredentialTokenExpander : ITokenExpander
{
    public bool CanExpand(string token) => token.StartsWith("{cred:") && token.EndsWith("}");

    public string Expand(string token)
    {
        var parts = token.Substring(6, token.Length - 7).Split(':');
        var target = parts[0];  // The credential target (e.g., app name)
        var key = parts[1];     // The specific key (e.g., username)

        return RetrieveCredential(target, key) ?? string.Empty;
    }

    /// <summary>
    /// Retrieve credentials from Windows Credential Manager
    /// </summary>
    private string RetrieveCredential(string target, string key)
    {
        var credential = CredentialManager.ReadCredential(target);
        if (credential == null)
        {
            return null;
        }

        // Convert IntPtr to string for username and password
        var username = Marshal.PtrToStringUni(credential.Value.UserName);
        var password = Marshal.PtrToStringUni(credential.Value.CredentialBlob);

        // Depending on the key (e.g., "username" or "password"), return the appropriate value
        return key == "username" ? username : password;
    }
}

public static class CredentialManager
{
    [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("Advapi32.dll", SetLastError = true)]
    private static extern void CredFree(IntPtr cred);

    public static Credential? ReadCredential(string target)
    {
        if (CredRead(target, 1, 0, out var credPtr))
        {
            try
            {
                return (Credential)Marshal.PtrToStructure(credPtr, typeof(Credential));
            }
            finally
            {
                CredFree(credPtr);
            }
        }
        return null;
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct Credential
{
    public int Flags;
    public int Type;
    public IntPtr TargetName;
    public IntPtr Comment;
    public ulong LastWritten;
    public int CredentialBlobSize;
    public IntPtr CredentialBlob;
    public int Persist;
    public int AttributeCount;
    public IntPtr Attributes;
    public IntPtr UserName;
    public IntPtr Password;
}
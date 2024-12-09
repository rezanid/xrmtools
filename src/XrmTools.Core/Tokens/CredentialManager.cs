#nullable enable
namespace XrmTools.Tokens;

using CredentialManagement;

public interface ICredentialManager
{
    void DeleteCredentials(string target);
    (string username, string password) ReadCredentials(string target);
    void SaveCredentials(string target, string username, string password);
}

public class CredentialManager : ICredentialManager
{
    public void SaveCredentials(string target, string username, string password)
    {
        using var cred = new Credential();
        cred.Target = target;
        cred.Username = username;
        cred.Password = password;
        cred.Type = CredentialType.Generic;
        cred.PersistanceType = PersistanceType.LocalComputer;
        cred.Save();
    }

    public void DeleteCredentials(string target)
    {
        using var cred = new Credential { Target = target };
        cred.Delete();
    }

    public (string username, string password) ReadCredentials(string target)
    {
        using var cred = new Credential { Target = target };
        cred.Load();
        return (cred.Username, cred.Password);
    }
}
#nullable restore
namespace XrmTools.Tokens;

public interface ITokenExpander
{
    bool CanExpand(string token);
    string Expand(string token);
}

namespace XrmTools.Tokens;

internal interface ITokenExpander
{
    bool CanExpand(string token);
    string Expand(string token);
}

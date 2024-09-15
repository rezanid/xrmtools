namespace XrmGen.Tokens;

internal interface ITokenExpander
{
    bool CanExpand(string token);
    string Expand(string token);
}

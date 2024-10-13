#nullable enable
namespace XrmTools.Tokens;

using System.Collections.Generic;
using System.Text;

public record TokenInfo(string OriginalToken, string Value, int StartIndex, int EndIndex);

public interface ITokenExpanderService
{
    string ExpandTokens(string input);
    IEnumerable<TokenInfo> GetTokens(string input);
}

public class TokenExpanderService(IEnumerable<ITokenExpander> expanders) : ITokenExpanderService
{
    public string ExpandTokens(string input)
    {
        var sb = new StringBuilder(input);
        var tokens = GetTokens(input);

        int offset = 0; // To adjust the position in case of length changes

        foreach (var token in tokens)
        {
            string? expandedValue = null;

            foreach (var expander in expanders)
            {
                if (expander.CanExpand(token.Value))
                {
                    expandedValue = expander.Expand(token.Value);
                    break;
                }
            }

            if (expandedValue != null)
            {
                // Replace token with expanded value
                sb.Remove(token.StartIndex + offset, token.OriginalToken.Length);
                sb.Insert(token.StartIndex + offset, expandedValue);

                // Adjust the offset for future replacements
                offset += expandedValue.Length - token.OriginalToken.Length;
            }
        }

        return sb.ToString();
    }

    public IEnumerable<TokenInfo> GetTokens(string input)
    {
        int start = -1;
        bool colonFound = false;

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '{')
            {
                start = i;
                colonFound = false;
            }
            else if (input[i] == ':' && start != -1)
            {
                colonFound = true;
            }
            else if (input[i] == '}' && start != -1 && colonFound)
            {
                string token = input.Substring(start, i - start + 1);
                string value = input.Substring(start + 1, i - start - 1); // Extract the part between { and }
                yield return new TokenInfo(token, value, start, i);
                start = -1;
            }
        }
    }
}
#nullable restore
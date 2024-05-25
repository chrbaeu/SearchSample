using System.Collections.Generic;

namespace SearchSample.QueryParser;

public record class TokenizerConfig
{
    public string OpeningBracketToken { get; init; } = "(";
    public string ClosingBracketToken { get; init; } = ")";
    public string NotToken { get; init; } = "!";
    public string AndToken { get; init; } = "&";
    public string OrToken { get; init; } = "|";
    public string SegmentToken { get; init; } = "\"";
    public string? DefaultOpToken { get; init; } = "&";
    public IReadOnlySet<char> EscapeChars { get; init; } = new HashSet<char>() { '\\' };
    public IReadOnlySet<char> SegmentChars { get; init; } = new HashSet<char>() { '"', '\'' };
    public IReadOnlySet<char> WhiteSpaceChars { get; init; } = new HashSet<char>() { ' ', '\t' };
    public IReadOnlySet<char> OpeningBracketChars { get; init; } = new HashSet<char>() { '(', '{', '[' };
    public IReadOnlySet<char> ClosingBracketChars { get; init; } = new HashSet<char>() { ')', '}', ']' };
    public IReadOnlySet<char> NotOperatorChars { get; init; } = new HashSet<char>() { '!' };
    public IReadOnlySet<char> AndOperatorChars { get; init; } = new HashSet<char>() { '&' };
    public IReadOnlySet<char> OrOperatorChars { get; init; } = new HashSet<char>() { '|' };

    public bool IsBracket(string token) => token == OpeningBracketToken || token == ClosingBracketToken;
    public bool IsOperator(string token) => token == AndToken || token == OrToken || token == NotToken;
}

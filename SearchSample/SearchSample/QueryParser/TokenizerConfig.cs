using System;
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
    public IReadOnlySet<char> WordSeparatorChars { get; init; } = CharsHelper.GetWordSeparatorChars();
    public IReadOnlySet<char> OpeningBracketChars { get; init; } = new HashSet<char>() { '(', '{', '[' };
    public IReadOnlySet<char> ClosingBracketChars { get; init; } = new HashSet<char>() { ')', '}', ']' };
    public IReadOnlySet<char> NotOperatorChars { get; init; } = new HashSet<char>() { '!' };
    public IReadOnlySet<char> AndOperatorChars { get; init; } = new HashSet<char>() { '&' };
    public IReadOnlySet<char> OrOperatorChars { get; init; } = new HashSet<char>() { '|' };
    public IReadOnlyDictionary<string, string> OperatorWords { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "and", "&" },
        { "or", "|" },
    };

    public bool IsBracket(string token) => token == OpeningBracketToken || token == ClosingBracketToken;
    public bool IsOperator(string token) => token == AndToken || token == OrToken || token == NotToken;
}

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SearchSample.QueryProcessing;

public partial class SearchWordHighlighter
{

    private readonly Regex? regex;

    public SearchWordHighlighter(List<string> searchWords, bool highlightEntireWord = false)
    {
        var pattern = ConvertToRegexPattern(searchWords, highlightEntireWord);
        regex = pattern != null ? new Regex(pattern, RegexOptions.IgnoreCase) : null;
    }

    public SearchWordHighlighter(TokenizerConfig config, List<string> postfixTokens, bool highlightEntireWord = false)
    {
        var pattern = ConvertToRegexPattern(GetSearchWords(postfixTokens, config), highlightEntireWord);
        regex = pattern != null ? new Regex(pattern, RegexOptions.IgnoreCase) : null;
    }

    public string HighlightText(string inputText, string startMarker = "<b>", string endMarker = "</b>")
    {
        if (string.IsNullOrEmpty(inputText) || regex is null) { return inputText; }
        return regex.Replace(inputText, $"{startMarker}$1{endMarker}");
    }

    public IEnumerable<(string TextPart, bool IsMatch)> EnumerateTextParts(string inputText)
    {
        if (string.IsNullOrEmpty(inputText) || regex is null) { yield return (inputText, false); yield break; }
        int lastMatchEnd = 0;
        foreach (Match match in regex.Matches(inputText))
        {
            if (match.Index > lastMatchEnd)
            {
                yield return (inputText.Substring(lastMatchEnd, match.Index - lastMatchEnd), false);
            }
            yield return (match.Value, true);
            lastMatchEnd = match.Index + match.Length;
        }
        if (lastMatchEnd < inputText.Length)
        {
            yield return (inputText[lastMatchEnd..], false);
        }
    }

    private static List<string> GetSearchWords(List<string> postfixTokens, TokenizerConfig config)
    {
        List<string> searchTerms = [];
        foreach (var token in postfixTokens)
        {
            if (token == config.NotToken)
            {
                searchTerms.RemoveAt(searchTerms.Count - 1);
            }
            else if (token != config.AndToken && token != config.OrToken)
            {
                searchTerms.Add(token);
            }
        }
        return searchTerms;
    }

    private static string? ConvertToRegexPattern(List<string> searchWords, bool highlightEntireWord)
    {
        if (searchWords.Count == 0) { return null; }
        if (highlightEntireWord)
        {
            return @"\b(" + string.Join("|", searchWords.Select(Regex.Escape).Select(x => $@"(\w*{x}\w*)")) + @")\b";
        }
        else
        {
            return @"(" + string.Join("|", searchWords.Select(Regex.Escape)) + @")";
        }
    }

}

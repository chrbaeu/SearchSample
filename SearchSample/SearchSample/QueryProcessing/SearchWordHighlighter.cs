using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SearchSample.QueryProcessing;

public partial class SearchWordHighlighter
{
    private readonly Regex? regex;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchWordHighlighter"/> class.
    /// </summary>
    /// <param name="searchWords">The list of search words.</param>
    /// <param name="highlightEntireWord">Specifies whether to highlight the entire word or partial matches.</param>
    public SearchWordHighlighter(List<string> searchWords, bool highlightEntireWord = false)
    {
        var pattern = ConvertToRegexPattern(searchWords, highlightEntireWord);
        regex = pattern != null ? new Regex(pattern, RegexOptions.IgnoreCase) : null;
    }

    /// <summary>
    /// Highlights the search words in the input text.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="startMarker">The start marker for highlighting.</param>
    /// <param name="endMarker">The end marker for highlighting.</param>
    /// <returns>The input text with search words highlighted.</returns>
    public string HighlightText(string inputText, string startMarker = "<b>", string endMarker = "</b>")
    {
        if (string.IsNullOrEmpty(inputText) || regex is null) { return inputText; }
        return regex.Replace(inputText, $"{startMarker}$1{endMarker}");
    }

    /// <summary>
    /// Enumerates the text parts in the input text and indicates whether each part is a match or not.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>An enumerable of text parts with match indication.</returns>
    public IEnumerable<(string TextPart, bool IsMatch)> EnumerateTextParts(string inputText)
    {
        if (string.IsNullOrEmpty(inputText) || regex is null) { yield return (inputText, false); yield break; }
        var lastMatchEnd = 0;
        foreach (Match match in regex.Matches(inputText))
        {
            if (match.Index > lastMatchEnd)
            {
                yield return (inputText[lastMatchEnd..match.Index], false);
            }
            yield return (match.Value, true);
            lastMatchEnd = match.Index + match.Length;
        }
        if (lastMatchEnd < inputText.Length)
        {
            yield return (inputText[lastMatchEnd..], false);
        }
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

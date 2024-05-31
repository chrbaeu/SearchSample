using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchSample.QueryParser;

public class CharsHelper
{
    private static readonly Lazy<HashSet<char>> wordSeparatorChars = new(() =>
        Enumerable.Range(0, char.MaxValue)
            .Select(x => (char)x)
            .Where(x => char.IsWhiteSpace(x) || char.IsSeparator(x))
            .Concat(['.', ':', ',', ';', '?', '!', '"'])
            .ToHashSet()
    );

    public static HashSet<char> GetWordSeparatorChars() => wordSeparatorChars.Value;

    public static string NormalizeWhiteSpaces(string text)
    {
        char[] chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsWhiteSpace(chars[i]))
            {
                chars[i] = ' ';
            }
        }
        return new string(chars);
    }


}

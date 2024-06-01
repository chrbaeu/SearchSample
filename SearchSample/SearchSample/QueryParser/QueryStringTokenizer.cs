using System;
using System.Collections.Generic;
using System.Text;

namespace SearchSample.QueryParser;

internal sealed class QueryStringTokenizer(TokenizerConfig config)
{
    private readonly TokenListFixer tokenListFixer = new(config);

    public List<string> GetTokens(string queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString)) { return []; }
        TokenizerState tokenizerState = new(queryString);
        var span = tokenizerState.Query.Span;
        for (; tokenizerState.Index < tokenizerState.Query.Length; tokenizerState.Index++)
        {
            char currentChar = span[tokenizerState.Index];
            if (tokenizerState.IsSegment)
            {
                if (config.SegmentChars.Contains(currentChar))
                {
                    tokenizerState.IsSegment = false;
                    tokenizerState.StringBuilder.Append(config.SegmentToken);
                    FinishToken(tokenizerState);
                }
                else
                {
                    tokenizerState.StringBuilder.Append(currentChar);
                }
            }
            else
            {
                ProcessChar(tokenizerState, currentChar);
            }
        }
        if (tokenizerState.IsSegment)
        {
            tokenizerState.StringBuilder.Append(config.SegmentToken);
        }
        FinishToken(tokenizerState);
        return tokenListFixer.FixTokens(tokenizerState.Tokens);
    }

    private void ProcessChar(TokenizerState tokenizerState, char c)
    {
        if (config.EscapeChars.Contains(c))
        {
            tokenizerState.Index++;
            tokenizerState.StringBuilder.Append(tokenizerState.Query.Span[tokenizerState.Index]);
        }
        else if (config.SegmentChars.Contains(c))
        {
            tokenizerState.IsSegment = true;
            tokenizerState.StringBuilder.Append(config.SegmentToken);
        }
        else if (config.OpeningBracketChars.Contains(c))
        {
            FinishToken(tokenizerState);
            tokenizerState.Tokens.Add(config.OpeningBracketToken);
        }
        else if (config.ClosingBracketChars.Contains(c))
        {
            FinishToken(tokenizerState);
            tokenizerState.Tokens.Add(config.ClosingBracketToken);
        }
        else if (config.NotOperatorChars.Contains(c))
        {
            FinishToken(tokenizerState);
            tokenizerState.Tokens.Add(config.NotToken);
        }
        else if (config.AndOperatorChars.Contains(c))
        {
            FinishToken(tokenizerState);
            tokenizerState.Tokens.Add(config.AndToken);
        }
        else if (config.OrOperatorChars.Contains(c))
        {
            FinishToken(tokenizerState);
            tokenizerState.Tokens.Add(config.OrToken);
        }
        else if (config.WordSeparatorChars.Contains(c))
        {
            FinishToken(tokenizerState);
        }
        else
        {
            tokenizerState.StringBuilder.Append(c);
        }
    }

    private static void FinishToken(TokenizerState tokenizerState)
    {
        if (tokenizerState.StringBuilder.Length > 0)
        {
            tokenizerState.Tokens.Add(tokenizerState.StringBuilder.ToString());
            tokenizerState.StringBuilder.Length = 0;
        }
    }

    private class TokenizerState(string query)
    {
        public ReadOnlyMemory<char> Query = query.AsMemory();
        public int Index;
        public bool IsSegment;
        public StringBuilder StringBuilder = new();
        public List<string> Tokens = [];
    }

}

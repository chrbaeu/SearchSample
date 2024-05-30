using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchSample.QueryParser;

internal sealed class QueryStringTokenizer(TokenizerConfig config)
{
    readonly string emptySegment = config.SegmentToken + config.SegmentToken;

    private class TokenizerState(string query)
    {
        public ReadOnlyMemory<char> Query = query.AsMemory();
        public int Index;
        public bool IsSegment;
        public StringBuilder StringBuilder = new();
        public List<string> Tokens = [];
    }

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
        return FixTokens(tokenizerState.Tokens);
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
        else if (config.WhiteSpaceChars.Contains(c))
        {
            FinishToken(tokenizerState);
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

    private List<string> FixTokens(IList<string> srcTokens)
    {
        List<string> tokens = [];
        int openBrackets = 0;
        bool lastWasOperator = true;
        var openingBracketTokensCount = srcTokens.Where(x => x == config.OpeningBracketToken).Count();
        var closingBracketTokensCount = srcTokens.Where(x => x == config.ClosingBracketToken).Count();
        var missingBrackets = Enumerable.Range(0, Math.Max(0, openingBracketTokensCount - closingBracketTokensCount))
            .Select(x => config.ClosingBracketToken);
        foreach (var srcToken in srcTokens.Concat(missingBrackets))
        {
            var token = srcToken;
            if (config.OperatorWords.TryGetValue(token, out var opToken))
            {
                token = opToken;
            }
            if (token == config.OpeningBracketToken)
            {
                if (!lastWasOperator && config.DefaultOpToken is not null)
                {
                    tokens.Add(config.DefaultOpToken);
                }
                openBrackets++;
                lastWasOperator = true;
                tokens.Add(token);
            }
            else if (token == config.ClosingBracketToken)
            {
                if (openBrackets == 0)
                {
                    // Skip closing brackets without matching opening bracket
                    continue;
                }
                if (lastWasOperator)
                {
                    if (tokens[^1] == config.OpeningBracketToken)
                    {
                        // Skip empty brackets
                        tokens.RemoveAt(tokens.Count - 1);
                        openBrackets--;
                        lastWasOperator = tokens.Count == 0 || config.IsOperator(tokens[^1]) || tokens[^1] == config.OpeningBracketToken;
                        continue;
                    }
                    tokens.RemoveAt(tokens.Count - 1);
                    if (tokens.Count > 0 && tokens[^1] == config.OpeningBracketToken)
                    {
                        // Skip empty brackets
                        tokens.RemoveAt(tokens.Count - 1);
                        openBrackets--;
                        lastWasOperator = config.IsOperator(tokens[^1]);
                        continue;
                    }
                }
                if (tokens.Count > 1 && tokens[^2] == config.OpeningBracketToken)
                {
                    // Skip not needed brackets
                    tokens.RemoveAt(tokens.Count - 2);
                    openBrackets--;
                    lastWasOperator = tokens.Count == 0 || config.IsOperator(tokens[^1]) || tokens[^1] == config.OpeningBracketToken;
                    continue;
                }
                openBrackets--;
                lastWasOperator = false;
                tokens.Add(token);

            }
            else if (token == config.NotToken)
            {
                if (lastWasOperator && tokens.Count > 0 && tokens[^1] == config.NotToken)
                {
                    continue; // Skip consecutive not operators
                }
                lastWasOperator = true;
                tokens.Add(token);
            }
            else if (token == config.AndToken || token == config.OrToken)
            {
                if (lastWasOperator)
                {
                    continue; // Skip consecutive operators
                }
                lastWasOperator = true;
                tokens.Add(token);
            }
            else
            {
                if (token == emptySegment)
                {
                    continue; // Skip empty segment
                }
                if (!lastWasOperator && config.DefaultOpToken is not null)
                {
                    tokens.Add(config.DefaultOpToken);
                }
                lastWasOperator = false;
                tokens.Add(token);
            }
        }
        while (tokens.Count > 0 && (config.IsOperator(tokens[^1]) || tokens[^1] == config.OpeningBracketToken))
        {
            // Remove last operator because it's not valid
            tokens.RemoveAt(tokens.Count - 1);
        }
        return tokens;
    }

}

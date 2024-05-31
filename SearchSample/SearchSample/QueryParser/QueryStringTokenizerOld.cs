using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchSample.QueryParser;

internal sealed class QueryStringTokenizerOld(TokenizerConfig config)
{

    public List<string> GetTokens(string queryString)
    {
        List<string> tokens = [];
        if (string.IsNullOrWhiteSpace(queryString)) { return tokens; }
        var span = queryString.AsSpan();
        bool isSegment = false, lastWasNoOp = false;
        StringBuilder sb = new(queryString.Length);
        for (var i = 0; i < span.Length; i++)
        {
            var c = span[i];
            if (isSegment)
            {
                if (config.SegmentChars.Contains(c))
                {
                    isSegment = false;
                    if (sb.Length == 1)
                    {
                        sb.Length = 0;
                        continue;
                    }
                    sb.Append(config.SegmentToken);
                    FinishToken();
                }
                else
                {
                    sb.Append(c);
                }
            }
            else if (config.EscapeChars.Contains(c))
            {
                i++;
            }
            else if (config.SegmentChars.Contains(c))
            {
                isSegment = true;
                sb.Append(config.SegmentToken);
            }
            else if (config.WordSeparatorChars.Contains(c))
            {
                FinishToken();
            }
            else if (config.OpeningBracketChars.Contains(c))
            {
                FinishToken();
                InsertDefaultOpIfNeeded();
                tokens.Add(config.OpeningBracketToken);
                lastWasNoOp = false;
            }
            else if (config.ClosingBracketChars.Contains(c))
            {
                FinishToken();
                if (tokens.Where(x => x == config.OpeningBracketToken).Count() > tokens.Where(x => x == config.ClosingBracketToken).Count())
                {
                    if (!lastWasNoOp)
                    {
                        if (tokens.Count > 0 && tokens[^1] == config.OpeningBracketToken)
                        {
                            tokens.RemoveAt(tokens.Count - 1);
                            continue;
                        }
                        tokens.RemoveAt(tokens.Count - 1);
                    }
                    tokens.Add(config.ClosingBracketToken);
                    lastWasNoOp = false;
                }
            }
            else if (config.NotOperatorChars.Contains(c))
            {
                FinishToken();
                tokens.Add(config.NotToken);
                lastWasNoOp = false;
            }
            else if (config.AndOperatorChars.Contains(c))
            {
                FinishToken();
                RemoveLastIfNoOp();
                tokens.Add(config.AndToken);
                lastWasNoOp = false;
            }
            else if (config.OrOperatorChars.Contains(c))
            {
                FinishToken();
                RemoveLastIfNoOp();
                tokens.Add(config.OrToken);
                lastWasNoOp = false;
            }
            else
            {
                sb.Append(c);
            }
        }
        FinishToken();
        RemoveLastIfNoOp();
        return tokens;
        void FinishToken()
        {
            if (sb.Length > 0)
            {
                InsertDefaultOpIfNeeded();
                tokens.Add(sb.ToString());
                sb.Length = 0;
                lastWasNoOp = true;
            }
        }
        void InsertDefaultOpIfNeeded()
        {
            if (config.DefaultOpToken is null)
            {
                return;
            }
            if (lastWasNoOp || (tokens.Count > 0 && tokens[^1] == config.ClosingBracketToken))
            {
                tokens.Add(config.DefaultOpToken);
            }
        }
        void RemoveLastIfNoOp()
        {
            if (!lastWasNoOp && tokens.Count > 0 && tokens[^1] != config.ClosingBracketToken)
            {
                tokens.RemoveAt(tokens.Count - 1);
            }
        }
    }

}

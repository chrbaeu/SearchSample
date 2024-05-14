using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchSample.QueryParser;

internal sealed class QueryStringTokenizer(TokenizerConfig config)
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
            }
            else if (config.WhiteSpaceChars.Contains(c))
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
                tokens.Add(config.AndToken);
                lastWasNoOp = false;
            }
            else if (config.OrOperatorChars.Contains(c))
            {
                FinishToken();
                tokens.Add(config.OrToken);
                lastWasNoOp = false;
            }
            else
            {
                sb.Append(c);
            }
        }
        FinishToken();
        if (!lastWasNoOp && tokens.Count > 0 && tokens[^1] != config.ClosingBracketToken)
        {
            tokens.RemoveAt(tokens.Count - 1);
        }
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
            if (lastWasNoOp && config.DefaultOpToken is not null)
            {
                tokens.Add(config.DefaultOpToken);
            }
        }
    }

}

using System.Collections.Generic;

namespace SearchSample.QueryParser;
internal sealed class TokenListFixer(TokenizerConfig config)
{
    readonly string emptySegment = config.SegmentToken + config.SegmentToken;

    public List<string> FixTokens(IEnumerable<string> srcTokens)
    {
        List<string> tokens = [];
        int openBrackets = 0;
        bool lastWasOperator = true;
        foreach (string srcToken in srcTokens)
        {
            var token = config.OperatorWords.TryGetValue(srcToken, out var opToken) ? opToken : srcToken;
            ProcessToken(tokens, ref openBrackets, ref lastWasOperator, token);
        }
        for (int i = openBrackets; i > 0; i--)
        {
            ProcessToken(tokens, ref openBrackets, ref lastWasOperator, config.ClosingBracketToken);
        }
        RemoveInvalidOperatorsFromEnd(tokens);
        return tokens;
    }

    private void ProcessToken(List<string> tokens, ref int openBrackets, ref bool lastWasOperator, string token)
    {
        if (token == config.OpeningBracketToken)
        {
            ProcessOpeningBracketToken(tokens, ref openBrackets, ref lastWasOperator, token);
        }
        else if (token == config.ClosingBracketToken)
        {
            ProcessClosingBracketToken(tokens, ref openBrackets, ref lastWasOperator, token);
        }
        else if (token == config.NotToken)
        {
            ProcessNotToken(tokens, ref lastWasOperator, token);
        }
        else if (token == config.AndToken)
        {
            ProcessAndToken(tokens, ref lastWasOperator, token);
        }
        else if (token == config.OrToken)
        {
            ProcessOrToken(tokens, ref lastWasOperator, token);
        }
        else
        {
            ProcessTextToken(tokens, ref lastWasOperator, token);
        }
    }

    private void ProcessOpeningBracketToken(List<string> tokens, ref int openBrackets, ref bool lastWasOperator, string token)
    {
        if (!lastWasOperator && config.DefaultOpToken is not null)
        {
            tokens.Add(config.DefaultOpToken);
        }
        openBrackets++;
        lastWasOperator = true;
        tokens.Add(token);
    }

    private void ProcessClosingBracketToken(List<string> tokens, ref int openBrackets, ref bool lastWasOperator, string token)
    {
        if (openBrackets == 0)
        {
            // Skip closing brackets without matching opening bracket
            return;
        }
        if (lastWasOperator)
        {
            if (tokens[^1] == config.OpeningBracketToken)
            {
                // Skip empty brackets
                tokens.RemoveAt(tokens.Count - 1);
                openBrackets--;
                lastWasOperator = tokens.Count == 0 || config.IsOperator(tokens[^1]) || tokens[^1] == config.OpeningBracketToken;
                return;
            }
            tokens.RemoveAt(tokens.Count - 1);
            if (tokens.Count > 0 && tokens[^1] == config.OpeningBracketToken)
            {
                // Skip empty brackets
                tokens.RemoveAt(tokens.Count - 1);
                openBrackets--;
                lastWasOperator = config.IsOperator(tokens[^1]);
                return;
            }
        }
        if (tokens.Count > 1 && tokens[^2] == config.OpeningBracketToken)
        {
            // Skip not needed brackets
            tokens.RemoveAt(tokens.Count - 2);
            openBrackets--;
            lastWasOperator = tokens.Count == 0 || config.IsOperator(tokens[^1]) || tokens[^1] == config.OpeningBracketToken;
            return;
        }
        openBrackets--;
        lastWasOperator = false;
        tokens.Add(token);
    }

    private void ProcessNotToken(List<string> tokens, ref bool lastWasOperator, string token)
    {
        if (lastWasOperator && tokens.Count > 0 && tokens[^1] == config.NotToken)
        {
            return; // Skip consecutive not operators
        }
        lastWasOperator = true;
        tokens.Add(token);
    }

    private static void ProcessAndToken(List<string> tokens, ref bool lastWasOperator, string token)
    {
        if (lastWasOperator)
        {
            return; // Skip consecutive operators
        }
        lastWasOperator = true;
        tokens.Add(token);
    }

    private static void ProcessOrToken(List<string> tokens, ref bool lastWasOperator, string token)
    {
        if (lastWasOperator)
        {
            return; // Skip consecutive operators
        }
        lastWasOperator = true;
        tokens.Add(token);
    }

    private void ProcessTextToken(List<string> tokens, ref bool lastWasOperator, string token)
    {
        if (token == emptySegment)
        {
            return; // Skip empty segment
        }
        if (!lastWasOperator && config.DefaultOpToken is not null)
        {
            tokens.Add(config.DefaultOpToken);
        }
        lastWasOperator = false;
        tokens.Add(token);
    }

    private void RemoveInvalidOperatorsFromEnd(List<string> tokens)
    {
        while (tokens.Count > 0 && (config.IsOperator(tokens[^1]) || tokens[^1] == config.OpeningBracketToken))
        {
            // Remove last operator because it's not valid
            tokens.RemoveAt(tokens.Count - 1);
        }
    }

}

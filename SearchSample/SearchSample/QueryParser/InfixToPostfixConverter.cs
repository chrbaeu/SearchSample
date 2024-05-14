using System.Collections.Generic;

namespace SearchSample.QueryParser;

internal sealed class InfixToPostfixConverter
{

    private readonly TokenizerConfig config;
    private readonly Dictionary<string, int> precedence;
    private readonly Dictionary<string, bool> leftAssociative;

    public InfixToPostfixConverter(TokenizerConfig config)
    {
        this.config = config;
        precedence = new() { { config.NotToken, 3 }, { config.AndToken, 2 }, { config.OrToken, 1 } };
        leftAssociative = new() { { config.NotToken, false }, { config.AndToken, true }, { config.OrToken, true } };
    }

    public IEnumerable<string> InfixToPostfix(IEnumerable<string> tokens)
    {
        Stack<string> stack = new();
        foreach (var token in tokens)
        {
            if (config.IsOperator(token))
            {
                while (stack.Count > 0 && config.IsOperator(stack.Peek()) &&
                       (leftAssociative[token] && precedence[token] <= precedence[stack.Peek()] ||
                        !leftAssociative[token] && precedence[token] < precedence[stack.Peek()]))
                {
                    yield return stack.Pop();
                }
                stack.Push(token);
            }
            else if (token == config.OpeningBracketToken)
            {
                stack.Push(token);
            }
            else if (token == config.ClosingBracketToken)
            {
                while (stack.Count > 0 && stack.Peek() != config.OpeningBracketToken)
                {
                    yield return stack.Pop();
                }
                if (stack.Count > 0) { stack.Pop(); }
            }
            else
            {
                yield return token;
            }
        }
        while (stack.Count > 0)
        {
            yield return stack.Pop();
        }
    }

}

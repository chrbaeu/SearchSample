namespace SearchSample.QueryProcessing;

public class InfixToPostfixConverter
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

    public List<string> InfixToPostfix(List<string> tokens)
    {
        Stack<string> stack = new();
        List<string> output = [];
        foreach (var token in tokens)
        {
            if (IsOperator(token))
            {
                while (stack.Count > 0 && IsOperator(stack.Peek()) &&
                       (leftAssociative[token] && precedence[token] <= precedence[stack.Peek()] ||
                        !leftAssociative[token] && precedence[token] < precedence[stack.Peek()]))
                {
                    output.Add(stack.Pop());
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
                    output.Add(stack.Pop());
                }
                if (stack.Count > 0) { stack.Pop(); }
            }
            else
            {
                output.Add(token);
            }
        }
        while (stack.Count > 0)
        {
            output.Add(stack.Pop());
        }
        return output;
    }

    private bool IsOperator(string token) => precedence.ContainsKey(token);

}

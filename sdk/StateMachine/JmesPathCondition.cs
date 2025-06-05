
public record JmesPathCondition
{
    public JmesPathCondition(string expression)
    {
        ArgumentException.ThrowIfNullOrEmpty(expression);
        this.Expression = expression;
    }

    public string Expression { get; }
}
public record SimpleCondition
{
    private SimpleCondition(string expression)
    {
        this.Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public string Expression { get; }

    /// <summary>
    /// Creates a condition that evaluates whether the specified variable is equal to the given value.
    /// </summary>
    /// <param name="variable">The variable to compare. Cannot be <see langword="null"/>.</param>
    /// <param name="value">The value to compare against. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>A <see cref="SimpleCondition"/> representing the equality comparison.</returns>
    public static SimpleCondition Equals(UserDefinedVariableReference variable, string value)
    {
        ArgumentNullException.ThrowIfNull(variable);
        ArgumentException.ThrowIfNullOrEmpty(value);
        return new SimpleCondition($"{variable.Name}.Equals({value})");
    }

    /// <summary>
    /// Creates a condition that checks if the specified variable contains the given value.
    /// </summary>
    /// <param name="variable">The variable to evaluate. Must be a user-defined variable.</param>
    /// <param name="value">The string value to check for within the variable.</param>
    /// <returns>A <see cref="SimpleCondition"/> representing the containment check.</returns>
    public static SimpleCondition Contains(UserDefinedVariableReference variable, string value)
    {
        return SimpleCondition.Contains((VariableReference)variable, value);
    }

    /// <summary>
    /// Creates a condition that checks if the specified variable contains the given value.
    /// </summary>
    /// <param name="variable">The variable to evaluate for the presence of the specified value.</param>
    /// <param name="value">The value to check for within the variable.</param>
    /// <returns>A <see cref="SimpleCondition"/> representing the condition that evaluates to <see langword="true"/> if the
    /// variable contains the value; otherwise, <see langword="false"/>.</returns>
    public static SimpleCondition Contains(MessagesVariableReference variable, string value)
    {
        return SimpleCondition.Contains((VariableReference)variable, value);
    }

    /// <summary>
    /// Creates a condition that evaluates to <see langword="true"/> if the specified variable does not contain the
    /// given value.
    /// </summary>
    /// <param name="variable">The variable to evaluate. Must not be <see langword="null"/>.</param>
    /// <param name="value">The value to check for absence within the variable. Must not be <see langword="null"/> or empty.</param>
    /// <returns>A <see cref="SimpleCondition"/> representing the "not contains" condition.</returns>
    public static SimpleCondition NotContains(UserDefinedVariableReference variable, string value)
    {
        return SimpleCondition.NotContains((VariableReference)variable, value);
    }

    /// <summary>
    /// Creates a condition that evaluates to <see langword="true"/> if the specified variable does not contain the
    /// given value.
    /// </summary>
    /// <param name="variable">The variable to evaluate. Must not be <see langword="null"/>.</param>
    /// <param name="value">The value to check for absence within the variable. Must not be <see langword="null"/> or empty.</param>
    /// <returns>A <see cref="SimpleCondition"/> representing the "not contains" condition.</returns>
    public static SimpleCondition NotContains(MessagesVariableReference variable, string value)
    {
        return SimpleCondition.NotContains((VariableReference)variable, value);
    }

    /// <summary>
    /// Creates a condition that evaluates whether the specified variable is empty.
    /// </summary>
    /// <param name="variable">The variable to check for emptiness. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="SimpleCondition"/> representing the emptiness check for the specified variable.</returns>
    public static SimpleCondition IsEmpty(UserDefinedVariableReference variable)
    {
        ArgumentNullException.ThrowIfNull(variable);
        return new SimpleCondition($"{variable.Name}.IsEmpty()");
    }

    /// <summary>
    /// Creates a condition that evaluates whether the specified variable is not empty.
    /// </summary>
    /// <param name="variable">The variable to check for non-emptiness. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="SimpleCondition"/> representing the non-emptiness check for the specified variable.</returns>
    public static SimpleCondition IsNotEmpty(UserDefinedVariableReference variable)
    {
        ArgumentNullException.ThrowIfNull(variable);
        return new SimpleCondition($"{variable.Name}.IsNonEmpty()");
    }

    /// <summary>
    /// Creates a condition that checks whether the specified variable contains the given value.
    /// </summary>
    /// <param name="variable">The variable to evaluate. Cannot be <see langword="null"/>.</param>
    /// <param name="value">The value to check for within the variable. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>A <see cref="SimpleCondition"/> representing the containment check.</returns>
    private static SimpleCondition Contains(VariableReference variable, string value)
    {
        ArgumentNullException.ThrowIfNull(variable);
        ArgumentException.ThrowIfNullOrEmpty(value);
        return new SimpleCondition($"{variable.Name}.Contains({value})");
    }

    /// <summary>
    /// Creates a condition that evaluates whether the specified variable does not contain the given value.
    /// </summary>
    /// <param name="variable">The variable to evaluate. Cannot be <see langword="null"/>.</param>
    /// <param name="value">The value to check for absence in the variable. Cannot be <see langword="null"/> or empty.</param>
    /// <returns>A <see cref="SimpleCondition"/> representing the "not contains" condition.</returns>
    private static SimpleCondition NotContains(VariableReference variable, string value)
    {
        ArgumentNullException.ThrowIfNull(variable);
        ArgumentException.ThrowIfNullOrEmpty(value);
        return new SimpleCondition($"{variable.Name}.NotContains({value})");
    }
}
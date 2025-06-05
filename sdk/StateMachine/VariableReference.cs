public record VariableReference
{
    internal VariableReference(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));
        }

        Name = name;
    }
    /// <summary>
    /// Gets the name of the variable.
    /// </summary>
    public string Name { get; }
}
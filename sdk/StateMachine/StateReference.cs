
/// <summary>
/// Represents a reference to a state that can be used in transitions.
/// </summary>
public record StateReference
{
    internal StateReference(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("State name cannot be null or empty", nameof(name));
        }

        this.Name = name;
    }

    /// <summary>
    /// Gets the name of the state.
    /// </summary>
    public string Name { get; }
}
namespace FluentPatcher.Context;

/// <summary>
/// Represents a change to a single property.
/// </summary>
public sealed class PropertyChange
{
    /// <summary>
    /// The name of the property that changed.
    /// </summary>
    public string PropertyName { get; }
        
    /// <summary>
    /// The old value before the patch.
    /// </summary>
    private object? OldValue { get; }
        
    /// <summary>
    /// The new value after the patch.
    /// </summary>
    private object? NewValue { get; }
        
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyChange"/> class with the property name, old value, and new value.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public PropertyChange(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
        
    /// <summary>
    /// Returns a string representation of the property change in the format "PropertyName: 'OldValue' -> 'NewValue'".
    /// </summary>
    /// <returns>A string summarizing the property change.</returns>
    public override string ToString() => $"{PropertyName}: '{OldValue}' -> '{NewValue}'";
}
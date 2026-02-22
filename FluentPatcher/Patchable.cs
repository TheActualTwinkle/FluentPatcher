namespace FluentPatcher
{
    /// <summary>
    /// Represents an optional value that distinguishes between "not set" and "explicitly set to null".
    /// Use this for properties where you need to explicitly set null values.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public readonly struct Patchable<T>
    {
        private readonly T? _value;

        private Patchable(T? value, bool hasValue)
        {
            _value = value;
            HasValue = hasValue;
        }

        /// <summary>
        /// Gets whether this patchable has a value set (including null).
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Gets the value. Throws if HasValue is false.
        /// </summary>
        public T? Value => HasValue 
            ? _value 
            : throw new InvalidOperationException("Patchable has no value set. Check HasValue first.");

        /// <summary>
        /// Creates a Patchable with a value (can be null for reference types).
        /// </summary>
        public static Patchable<T> Set(T? value) => new(value, true);

        /// <summary>
        /// Creates a Patchable that is not set (property will not be updated).
        /// </summary>
        public static Patchable<T> NotSet() => new(default, false);

        /// <summary>
        /// Implicit conversion from T to Patchable{T} with value set.
        /// </summary>
        public static implicit operator Patchable<T>(T? value) => Set(value);

        /// <summary>
        /// Returns a string representation.
        /// </summary>
        public override string ToString() => HasValue ? $"Set({_value})" : "NotSet";
    }

    /// <summary>
    /// Helper class for creating Patchable values.
    /// </summary>
    public static class Patchable
    {
        /// <summary>
        /// Creates a Patchable with the specified value.
        /// </summary>
        public static Patchable<T> Set<T>(T? value) => Patchable<T>.Set(value);

        /// <summary>
        /// Creates a Patchable that is not set.
        /// </summary>
        public static Patchable<T> NotSet<T>() => Patchable<T>.NotSet();

        /// <summary>
        /// Creates a Patchable explicitly set to null.
        /// </summary>
        public static Patchable<T?> Null<T>() where T : class => Patchable<T?>.Set(null);
    }
}


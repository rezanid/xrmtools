namespace XrmTools.Logging.Compatibility;

/// <summary>
/// Identifies a logging event. The primary identifier is the "Id" property, with
/// the "Name" property providing a short description of this type of event.
/// </summary>
public readonly struct EventId
{
    /// <summary>
    /// Gets the numeric identifier for this event.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this event.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Implicitly creates an EventId from the given System.Int32.
    /// </summary>
    /// <param name="i">The System.Int32 to convert to an EventId.</param>
    public static implicit operator EventId(int i) => new(i);

    /// <summary>
    /// Checks if two specified <see cref="EventId"/> instances have the
    /// same value. They are equal if they have the same Id.
    /// </summary>
    /// <param name="left">The <see cref="EventId"/>.</param>
    /// <param name="right">The second <see cref="EventId"/></param>
    /// <returns>true, if the objects are equal.</returns>
    public static bool operator ==(EventId left, EventId right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Checks if two specified <see cref="EventId"/> instances have different
    /// values.
    /// </summary>
    /// <param name="left">The first <see cref="EventId"/>.</param>
    /// <param name="right">The second <see cref="EventId"/>.</param>
    /// <returns>true, if the objects are not equal.</returns>
    public static bool operator !=(EventId left, EventId right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Initializes an instance of the <see cref="EventId"/> struct.
    /// </summary>
    /// <param name="id">The numeric identifier for this event.</param>
    /// <param name="name">The name of this event.</param>
    public EventId(int id, string name = null)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name ?? Id.ToString();

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// Two events are equal if they have the same id.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true,  the current object is equal to the other parameter; otherwise, false</returns>
    public bool Equals(EventId other) => Id == other.Id;

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is EventId other)
        {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode() => Id;
}
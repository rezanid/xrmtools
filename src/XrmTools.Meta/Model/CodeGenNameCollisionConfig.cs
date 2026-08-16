namespace XrmTools.Meta.Model;

public class CodeGenNameCollisionConfig
{
    /// <summary>
    /// Default suffix appended to a generated member name that would otherwise collide with the
    /// name of its enclosing type.
    /// </summary>
    public const string DefaultSuffix = "Value";

    public string Suffix { get; set; } = DefaultSuffix;
}

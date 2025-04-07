#nullable enable
namespace XrmTools.Validation;

using System.Diagnostics.CodeAnalysis;

public sealed class ValidationResult(bool isValid, string? errorMessage)
{
    public static ValidationResult Success { get; } = new(true, null);

    [MemberNotNullWhen(false, [nameof(ErrorMessage)])]
    public bool IsValid { get; } = isValid;

    public string? ErrorMessage { get; } = errorMessage;
}
#nullable restore
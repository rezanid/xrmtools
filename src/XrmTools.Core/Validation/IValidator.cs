#nullable enable
namespace XrmTools.Validation;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

public interface IValidator<in T> : IValidator
{
    ValidationResult Validate(T instance);
}

public interface IValidator
{
    Type TargetType { get; }
}

public interface IAsyncValidator<in T> : IValidator
{
    Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
#nullable restore
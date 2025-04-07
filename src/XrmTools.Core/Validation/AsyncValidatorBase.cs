#nullable enable
namespace XrmTools.Validation;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

public abstract class AsyncValidatorBase<T> : IAsyncValidator<T>
{
    [Import]
    internal protected Lazy<IValidationService> LazyValidationService { get; set; } = null!;

    protected IValidationService ValidationService => LazyValidationService.Value;

    public Type TargetType => typeof(T);

    public abstract Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
#nullable restore
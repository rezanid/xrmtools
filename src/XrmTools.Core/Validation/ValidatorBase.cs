#nullable enable
namespace XrmTools.Validation;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;

public abstract class ValidatorBase<T> : IValidator<T>
{
    [Import]
    internal protected Lazy<IValidationService> LazyValidationService { get; set; } = null!;

    protected IValidationService ValidationService => LazyValidationService.Value;

    public Type TargetType => typeof(T);

    public abstract ValidationResult Validate(T instance);

}
#nullable restore
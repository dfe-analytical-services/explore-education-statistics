#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static class ValidationUtils
{
    public static BadRequestObjectResult ValidationResult<TEnum>(params TEnum[] codes)
        where TEnum : Enum
    {
        return ValidationResult(codes.ToList());
    }

    public static BadRequestObjectResult ValidationResult<TEnum>(IEnumerable<TEnum> codes)
        where TEnum : Enum
    {
        return ValidationResult(
            codes.Select(code => new ErrorViewModel
            {
                // For now, we just output the code, but in the future,
                // it would be good to provide default messages as well.
                Code = code.ToString(),
            })
        );
    }

    public static BadRequestObjectResult ValidationResult(params ErrorViewModel[] errors)
    {
        return ValidationResult(errors.ToList());
    }

    public static BadRequestObjectResult ValidationResult(IEnumerable<ErrorViewModel> errors)
    {
        return new BadRequestObjectResult(new ValidationProblemViewModel { Errors = errors.ToList() });
    }

    /// <summary>
    /// Creates a NotFoundObjectResult with the given error details. Use this method when
    /// failing to find any of the principal entities being referenced in a request (e.g.
    /// a DataSetVersion for a given "dataSetVersionId" parameter in a request, but use
    /// ValidationResult when failing to find any secondary entities (e.g.
    /// a DataSetVersionMapping record) that *should* exist if the primary entity exists.
    /// </summary>
    public static NotFoundObjectResult NotFoundResult<TEntity, TId>(TId id, string path)
    {
        return new NotFoundObjectResult(
            new ValidationProblemViewModel
            {
                Status = StatusCodes.Status404NotFound,
                Errors =
                [
                    new ErrorViewModel
                    {
                        Code = "NotFound",
                        Path = path,
                        Detail = new InvalidErrorDetail<TId>(id),
                        Message = $"{typeof(TEntity).Name} not found",
                    },
                ],
            }
        );
    }
}

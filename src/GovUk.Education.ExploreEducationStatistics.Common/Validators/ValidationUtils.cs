#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static class ValidationUtils
{
    public static BadRequestObjectResult ValidationResult<TEnum>(params TEnum[] codes) where TEnum : Enum
    {
        return ValidationResult(codes.ToList());
    }

    public static BadRequestObjectResult ValidationResult<TEnum>(IEnumerable<TEnum> codes) where TEnum : Enum
    {
        return ValidationResult(
            codes.Select(code => new ErrorViewModel
            {
                // For now, we just output the code, but in the future,
                // it would be good to provide default messages as well.
                Code = code.ToString()
            })
        );
    }

    public static BadRequestObjectResult ValidationResult(params ErrorViewModel[] errors)
    {
        return ValidationResult(errors.ToList());
    }

    public static BadRequestObjectResult ValidationResult(IEnumerable<ErrorViewModel> errors)
    {
        return new BadRequestObjectResult(new ValidationProblemViewModel
        {
            Errors = errors.ToList()
        });
    }
}

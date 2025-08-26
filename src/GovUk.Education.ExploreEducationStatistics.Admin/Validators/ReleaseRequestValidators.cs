#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ReleaseCreateRequestValidator
{
    public static Either<ActionResult, Unit> Validate(ReleaseCreateRequest releaseCreateRequest)
    {
        if (releaseCreateRequest.Type == ReleaseType.ExperimentalStatistics)
        {
            return ValidationActionResult(ReleaseTypeInvalid);
        }

        return Unit.Instance;
    }
}

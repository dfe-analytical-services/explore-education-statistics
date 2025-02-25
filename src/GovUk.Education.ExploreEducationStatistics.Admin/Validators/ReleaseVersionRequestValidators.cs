#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ReleaseVersionUpdateRequestValidator
{
    public static Either<ActionResult, Unit> Validate(ReleaseVersionUpdateRequest releaseVersionUpdateRequest)
    {
        if (releaseVersionUpdateRequest.Type == ReleaseType.ExperimentalStatistics)
        {
            return ValidationActionResult(ReleaseTypeInvalid);
        }

        return Unit.Instance;
    }
}

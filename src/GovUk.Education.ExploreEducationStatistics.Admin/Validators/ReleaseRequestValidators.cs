using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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

public static class ReleaseUpdateRequestValidator
{
    public static Either<ActionResult, Unit> Validate(ReleaseVersionUpdateRequest releaseUpdateRequest)
    {
        if (releaseUpdateRequest.Type == ReleaseType.ExperimentalStatistics)
        {
            return ValidationActionResult(ReleaseTypeInvalid);
        }

        return Unit.Instance;
    }
}
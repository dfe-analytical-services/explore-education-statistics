using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Helpers;

public static class SemVersionParser
{
    public static Either<ActionResult, SemVersion> ParseVersionWithValidation(string version,  LocalizableMessage error, string path)
    {
        var result = SemVersion.TryParse(version,
            SemVersionStyles.OptionalMinorPatch | SemVersionStyles.AllowWhitespace | SemVersionStyles.AllowLowerV,
            out var versionToReplaceParsed)
            ? versionToReplaceParsed
            : new Either<ActionResult, SemVersion>(ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = error.Code,
                Message = error.Message,
                Path = path,
                Detail = null
            }));
        return result;
    }
}

using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using Microsoft.AspNetCore.Mvc;
using Semver;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Helpers;

public static class SemVersionParser
{
    public static Either<ActionResult, SemVersion> ParseVersionWithValidation(string version)
    {
        var result = SemVersion.TryParse(version,
            SemVersionStyles.OptionalMinorPatch | SemVersionStyles.AllowWhitespace | SemVersionStyles.AllowLowerV,
            out var versionToReplaceParsed)
            ? versionToReplaceParsed
            : new Either<ActionResult, SemVersion>(ValidationUtils.ValidationResult(new ErrorViewModel
            {
                Code = ValidationMessages.DataSetVersionToReplaceNotValid.Code,
                Message = ValidationMessages.DataSetVersionToReplaceNotValid.Message,
                Path = nameof(NextDataSetVersionMappingsCreateRequest.DataSetVersionToReplace),
                Detail = null
            }));
        return result;
    }
}

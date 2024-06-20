#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage CannotDeleteApiDataSetReleaseFile = new(
        Code: nameof(CannotDeleteApiDataSetReleaseFile),
        Message: "The file cannot be deleted because it is linked to an API data set."
    );
}

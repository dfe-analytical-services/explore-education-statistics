#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class ValidationMessages
{
    public static readonly LocalizableMessage CannotRemoveDataFileLinkedToApiDataSet = new(
        Code: nameof(CannotRemoveDataFileLinkedToApiDataSet),
        Message: "The data file cannot be deleted because it is linked to an API data set."
    );
}

using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;

public static class MvcOptionsExtensions
{
    public static void AddInvalidRequestInputResultFilter(this MvcOptions options)
    {
        options.ModelBindingMessageProvider
            .SetMissingRequestBodyRequiredValueAccessor(() => ValidationMessages.NotEmptyBody.Message);

        options.Filters.Add<InvalidRequestInputFilter>();
    }
}

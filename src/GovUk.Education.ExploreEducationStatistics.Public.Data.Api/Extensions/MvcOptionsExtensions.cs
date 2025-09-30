using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;

public static class MvcOptionsExtensions
{
    public static void AddInvalidRequestInputResultFilter(this MvcOptions options)
    {
        options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(() =>
            ValidationMessages.NotEmptyBody.Message
        );

        options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(_ =>
            ValidationMessages.InvalidValue.Message
        );

        options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
            (_, _) => ValidationMessages.InvalidValue.Message
        );

        options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(_ =>
            ValidationMessages.InvalidValue.Message
        );

        options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(_ =>
            ValidationMessages.InvalidValue.Message
        );

        options.Filters.Add<InvalidRequestInputFilter>();
    }
}

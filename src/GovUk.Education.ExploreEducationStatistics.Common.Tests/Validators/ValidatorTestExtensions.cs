using System;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;

public static class ValidatorTestExtensions
{
    public static ITestValidationWith WithCustomState<T>(
        this ITestValidationContinuation failures,
        Func<T, bool> predicate)
    {
        return failures.When(
            failure => failure.CustomState is T customState && predicate(customState),
            $"Expected custom state to be of type '{typeof(T).GetPrettyFullName()}' and match the predicate."
        );
    }
}
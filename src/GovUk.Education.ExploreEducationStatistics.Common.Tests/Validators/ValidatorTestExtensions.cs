using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;

public static class ValidatorTestExtensions
{
    public static ITestValidationWith WithAttemptedValue<T>(
        this ITestValidationContinuation failures,
        T value
    )
    {
        return failures.When(
            failure => failure.AttemptedValue is T attemptedValue && attemptedValue.Equals(value),
            $"Expected attempted value of '{value}'. Actual attempted value was '{{MessageArgument:PropertyValue}}'."
        );
    }

    public static ITestValidationWith WithCustomState<T>(
        this ITestValidationContinuation failures,
        Func<T, bool> predicate
    )
    {
        return failures.When(
            failure => failure.CustomState is T customState && predicate(customState),
            $"Expected custom state to be of type '{typeof(T).GetPrettyFullName()}' and match the predicate."
        );
    }
}

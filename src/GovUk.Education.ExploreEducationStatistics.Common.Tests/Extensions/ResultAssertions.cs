#nullable enable
using CSharpFunctionalExtensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class ResultAssertions
{
    public static T AssertSuccess<T>(this Result<T> result)
    {
        Assert.True(result.IsSuccess, "Expected Success but got Failure");
        Assert.NotNull(result.Value);
        return result.Value;
    }

    public static T AssertSuccess<T, TE>(this Result<T, TE> result)
    {
        Assert.True(result.IsSuccess, "Expected Success but got Failure");
        Assert.NotNull(result.Value);
        return result.Value;
    }

    public static void AssertFailure<T>(this Result<T> result)
    {
        Assert.True(result.IsFailure, "Expected Failure but got Success");
    }

    public static void AssertFailure<T>(this Result<T> result, string expectedError)
    {
        Assert.True(result.IsFailure, "Expected Failure but got Success");
        Assert.Equal(expectedError, result.Error);
    }

    public static void AssertFailure<T, TE>(this Result<T, TE> result, TE expectedError)
    {
        Assert.True(result.IsFailure, "Expected Failure but got Success");
        Assert.Equal(expectedError, result.Error);
    }
}

#nullable enable
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using KellermanSoftware.CompareNetObjects;
using Xunit;
using Xunit.Sdk;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class AssertExtensions
{
    public const int TimeWithinMillis = 10000;

    /**
        * Calling this method causes a Test to fail with the given message.  The equivalent of `Assert.Fail()` in
        * other testing frameworks.
        */
    public static XunitException AssertFail(string message)
    {
        throw new XunitException(message);
    }

    public static bool AssertDeepEqualTo<T>(
        this T actual,
        T expected,
        bool ignoreCollectionOrders = false,
        Expression<Func<T, object?>>[]? ignoreProperties = null
    )
    {
        var compareLogic = new CompareLogic();
        ignoreProperties?.ForEach(compareLogic.Config.IgnoreProperty);
        compareLogic.Config.MaxDifferences = 100;
        compareLogic.Config.IgnoreCollectionOrder = ignoreCollectionOrders;
        var comparison = compareLogic.Compare(expected, actual);
        Assert.True(comparison.AreEqual, comparison.DifferencesString);
        return true;
    }

    public static bool IsDeepEqualTo<T>(
        this T actual,
        T expected,
        Expression<Func<T, object>>[]? ignoreProperties = null
    )
    {
        var compareLogic = new CompareLogic();
        ignoreProperties?.ForEach(compareLogic.Config.IgnoreProperty);
        compareLogic.Config.MaxDifferences = 100;
        var comparison = compareLogic.Compare(expected, actual);
        return comparison.AreEqual;
    }

    /// <summary>
    /// Assert that the given DateTime is effectively "now", within a given tolerance of milliseconds.
    /// </summary>
    public static void AssertUtcNow(this DateTime dateTime, int withinMillis = TimeWithinMillis)
    {
        Assert.Equal(DateTime.UtcNow, dateTime, TimeSpan.FromMilliseconds(withinMillis));
    }

    /// <summary>
    /// Assert that the given DateTime is effectively "now", within a given tolerance of milliseconds.
    /// </summary>
    public static void AssertUtcNow(this DateTime? dateTime, int withinMillis = TimeWithinMillis)
    {
        Assert.NotNull(dateTime);
        dateTime.Value.AssertUtcNow(withinMillis: withinMillis);
    }

    /// <summary>
    /// Assert that the given DateTimeOffset is effectively "now", within a given tolerance of milliseconds.
    /// </summary>
    public static void AssertUtcNow(
        this DateTimeOffset dateTimeOffset,
        int withinMillis = TimeWithinMillis
    )
    {
        Assert.Equal(
            DateTimeOffset.UtcNow,
            dateTimeOffset,
            TimeSpan.FromMilliseconds(withinMillis)
        );
    }

    /// <summary>
    /// Assert that the given DateTimeOffset is effectively "now", within a given tolerance of milliseconds.
    /// </summary>
    public static void AssertUtcNow(
        this DateTimeOffset? dateTimeOffset,
        int withinMillis = TimeWithinMillis
    )
    {
        Assert.NotNull(dateTimeOffset);
        dateTimeOffset.Value.AssertUtcNow(withinMillis: withinMillis);
    }

    /// <summary>
    /// Assert that the given DateTime is effectively "now", within a given tolerance of milliseconds.
    /// </summary>
    public static void AssertEqual(
        this DateTime dateTime,
        DateTime expectedDateTime,
        int withinMillis = TimeWithinMillis
    )
    {
        Assert.Equal(expectedDateTime, dateTime, TimeSpan.FromMilliseconds(withinMillis));
    }

    /// <summary>
    /// Assert that the given DateTimeOffset is effectively "now", within a given tolerance of milliseconds.
    /// </summary>
    public static void AssertEqual(
        this DateTimeOffset dateTimeOffset,
        DateTimeOffset expectedDateTimeOffset,
        int withinMillis = TimeWithinMillis
    )
    {
        Assert.Equal(
            expectedDateTimeOffset,
            dateTimeOffset,
            TimeSpan.FromMilliseconds(withinMillis)
        );
    }

    /// <summary>
    /// Assert that the given DateTimeOffset is effectively "<paramref name="expectedDateTimeOffset"/>", within a given tolerance of milliseconds.
    /// </summary>
    /// <param name="expectedDateTimeOffset">the expected timestamp.</param>
    public static void AssertEqual(
        this DateTimeOffset? dateTimeOffset,
        DateTimeOffset expectedDateTimeOffset,
        int withinMillis = TimeWithinMillis
    )
    {
        Assert.NotNull(dateTimeOffset);
        dateTimeOffset.Value.AssertEqual(
            expectedDateTimeOffset: expectedDateTimeOffset,
            withinMillis: withinMillis
        );
    }
}

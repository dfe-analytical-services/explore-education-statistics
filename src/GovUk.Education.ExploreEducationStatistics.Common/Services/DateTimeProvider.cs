namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

/// <summary>
///
/// Note that .NET provides a TimeProvider and a FakeTimeProvider that is preferable to using this class for providing
/// dates and times to services.
///
/// A service that can be used to provide a DateTime to services. A fixed date can be provided during testing to
/// provide a mock DateTime.
///
/// </summary>
public class DateTimeProvider
{
    private readonly DateTime? _fixedDateTimeUtc;

    /// <summary>
    /// Allows a fixed DateTime to be provided for testing.
    /// </summary>
    /// <param name="fixedDateTimeUtc"></param>
    public DateTimeProvider(DateTime? fixedDateTimeUtc = null)
    {
        _fixedDateTimeUtc = fixedDateTimeUtc;
    }

    public DateTime UtcNow => _fixedDateTimeUtc ?? DateTime.UtcNow;
}

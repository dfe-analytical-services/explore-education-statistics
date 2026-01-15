#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using Microsoft.Extensions.Time.Testing;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests;

public class ReleaseVersionPublishedDisplayDateUpdateRequestTests
{
    private static FakeTimeProvider GetTimeProvider(DateTimeOffset? startDateTime = null) =>
        new(startDateTime ?? DateTimeOffset.UtcNow);

    [Theory]
    [InlineData("2000-01-01T00:00:00 +00:00")] // Inclusive range start
    [InlineData("2026-01-01T14:59:59 +00:00")] // Just inside the range
    [InlineData("2026-01-01T15:00:00 +00:00")] // Inclusive range end (time now)
    public void WhenPublishedDisplayDateIsInRange_ShouldNotHaveValidationError(string publishedDisplayDateString)
    {
        // Arrange
        var now = DateTimeOffset.Parse("2026-01-01T15:00:00 +00:00");
        var validator = new ReleaseVersionPublishedDisplayDateUpdateRequest.Validator(GetTimeProvider(now));
        var publishedDisplayDate = DateTimeOffset.Parse(publishedDisplayDateString);

        var request = new ReleaseVersionPublishedDisplayDateUpdateRequest
        {
            PublishedDisplayDate = publishedDisplayDate,
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.PublishedDisplayDate);
    }

    [Theory]
    [InlineData("1999-12-31T23:59:59 +00:00")] // Just before the range start
    [InlineData("2026-01-01T15:00:01 +00:00")] // Just after the range end
    public void WhenPublishedDisplayDateIsOutOfRange_ShouldHaveValidationError(string publishedDisplayDateString)
    {
        // Arrange
        var now = DateTimeOffset.Parse("2026-01-01T15:00:00 +00:00");
        var validator = new ReleaseVersionPublishedDisplayDateUpdateRequest.Validator(GetTimeProvider(now));
        var publishedDisplayDate = DateTimeOffset.Parse(publishedDisplayDateString);
        var request = new ReleaseVersionPublishedDisplayDateUpdateRequest
        {
            PublishedDisplayDate = publishedDisplayDate,
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result
            .ShouldHaveValidationErrorFor(r => r.PublishedDisplayDate)
            .WithErrorCode(FluentValidationKeys.InclusiveBetweenValidator);
    }
}

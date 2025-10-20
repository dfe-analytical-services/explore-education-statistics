using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using Microsoft.Extensions.Time.Testing;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Validators;

public class PreviewTokenCreateRequestValidatorTests
{
    private static readonly DateTimeOffset DefaultUtcNow = new(2025, 10, 1, 14, 0, 0, TimeSpan.Zero);

    private static FakeTimeProvider GetTimeProvider(DateTimeOffset? startDateTime = null) =>
        new(startDateTime ?? DefaultUtcNow);

    [Theory]
    [InlineData("2025-09-30T14:00:00 +00:00")]
    [InlineData("2025-10-01T13:59:49 +00:00")] // 1 second outside the 10-second tolerance
    public void Activates_InPast_Fails(string activates)
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = DateTimeOffset.Parse(activates),
            Expires = null,
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Activates)
            .WithErrorMessage("Activates date must not be in the past.");
    }

    [Theory]
    [InlineData("2025-10-01T14:00:00 +00:00")]
    [InlineData("2025-10-01T14:00:01 +00:00")]
    [InlineData("2025-10-08T13:59:59 +00:00")]
    [InlineData("2025-10-08T14:00:00 +00:00")]
    public void Activates_Within7Days_Passes(string activates)
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = DateTimeOffset.Parse(activates),
            Expires = null,
        };

        var result = validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(r => r.Activates);
    }

    [Fact]
    public void Activates_After7Days_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = DefaultUtcNow.AddDays(7).AddSeconds(1),
            Expires = null,
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Activates)
            .WithErrorMessage("Activates date must be within the next 7 days.");
    }

    [Fact]
    public void Expires_MoreThan7DaysAfterActivates_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = DefaultUtcNow,
            Expires = DefaultUtcNow.AddDays(8),
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must be no more than 7 days after the activates date.");
    }

    [Theory]
    // Expires cannot be same as activates (this should probably be in a different test?)
    [InlineData("2025-10-03T00:00:00 +01:00", "2025-10-03T00:00:00 +01:00", false)]
    // *Activates and expires both fall outside daylight savings time*
    // Activates at start of day:
    [InlineData("2025-01-01T00:00:00 +00:00", "2025-01-07T23:59:59 +00:00", true)] // <-- lt 7 days
    [InlineData("2025-01-01T00:00:00 +00:00", "2025-01-08T23:59:59 +00:00", true)] // <-- eq 7 days
    [InlineData("2025-01-01T00:00:00 +00:00", "2025-01-09T00:00:00 +00:00", false)] // <-- but not beyond that
    // Activates at 2pm:
    [InlineData("2025-01-01T14:00:00 +00:00", "2025-01-08T13:59:59 +00:00", true)] // <-- lt 7 days
    [InlineData("2025-01-01T14:00:00 +00:00", "2025-01-08T14:00:00 +00:00", true)] // <-- eq 7 days
    [InlineData("2025-01-01T14:00:00 +00:00", "2025-01-08T23:59:59 +00:00", true)] // <-- valid through to the end of the 7th day
    [InlineData("2025-01-01T14:00:00 +00:00", "2025-01-09T00:00:00 +00:00", false)] // <-- but not beyond that
    // *Activates and expires both fall within daylight savings time*
    // Activates at start of day:
    [InlineData("2025-10-03T00:00:00 +01:00", "2025-10-09T23:59:59 +01:00", true)] // <-- lt 7 days
    [InlineData("2025-10-03T00:00:00 +01:00", "2025-10-10T23:59:59 +01:00", true)] // <-- eq 7 days
    [InlineData("2025-10-03T00:00:00 +01:00", "2025-10-11T00:00:00 +01:00", false)] // <-- but not beyond that
    // Activates at 2pm:
    [InlineData("2025-10-03T14:00:00 +01:00", "2025-10-10T13:59:59 +01:00", true)] // <-- lt 7 days
    [InlineData("2025-10-03T14:00:00 +01:00", "2025-10-10T14:00:00 +01:00", true)] // <-- eq 7 days
    [InlineData("2025-10-03T14:00:00 +01:00", "2025-10-10T23:59:59 +01:00", true)] // <-- valid through to the end of the 7th day
    [InlineData("2025-10-03T14:00:00 +01:00", "2025-10-11T00:00:00 +01:00", false)] // <-- but not beyond that
    // *Activates outside daylight savings time, expires within daylight savings time*
    // Activates at start of day:
    [InlineData("2025-03-28T00:00:00 +00:00", "2025-04-04T23:59:59 +01:00", true)] // <-- lt or eq to 7 days
    [InlineData("2025-03-28T00:00:00 +00:00", "2025-04-05T00:00:00 +01:00", false)] // <-- but not beyond that
    // Activates at 2pm:
    [InlineData("2025-03-28T14:00:00 +00:00", "2025-04-04T13:59:59 +01:00", true)] // <-- lt 7 days
    [InlineData("2025-03-28T14:00:00 +00:00", "2025-04-04T14:00:00 +01:00", true)] // <-- eq 7 days
    [InlineData("2025-03-28T14:00:00 +00:00", "2025-04-04T23:59:59 +01:00", true)] // <-- vlt 7 days (valid through to the end of the 7th day)
    [InlineData("2025-03-28T14:00:00 +00:00", "2025-04-05T00:00:00 +01:00", false)] // but not beyond that
    // *Activates within daylight savings time, expires outside daylight savings time*
    // Activates at start of day:
    [InlineData("2025-10-20T00:00:00 +01:00", "2025-10-26T23:59:59 +00:00", true)] // <-- lt 7 days
    [InlineData("2025-10-20T00:00:00 +01:00", "2025-10-27T23:59:59 +00:00", true)] // <-- eq 7 days
    [InlineData("2025-10-20T00:00:00 +01:00", "2025-10-28T00:00:00 +00:00", false)] // but not beyond that
    // Activates at 2pm:
    [InlineData("2025-10-20T14:00:00 +01:00", "2025-10-27T13:59:59 +00:00", true)] // <-- lt 7 days
    [InlineData("2025-10-20T14:00:00 +01:00", "2025-10-27T14:00:00 +00:00", true)] // <-- eq 7 days
    [InlineData("2025-10-20T14:00:00 +01:00", "2025-10-27T23:59:59 +00:00", true)] // <-- valid through to the end of the 7th day
    [InlineData("2025-10-20T14:00:00 +01:00", "2025-10-28T00:00:00 +00:00", false)] // <-- but not beyond that
    public void Expires_WhenActivatesIsNotNull_ValidUpTo7DaysAfterActivates(
        string activates,
        string expires,
        bool passes
    )
    {
        // Parse activates to use as the "UTC now" time for the test TimeProvider to ensure "activates" is valid
        var timeProvider = GetTimeProvider(DateTimeOffset.Parse(activates));
        var validator = new PreviewTokenCreateRequest.Validator(timeProvider);

        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = DateTimeOffset.Parse(activates),
            Expires = DateTimeOffset.Parse(expires),
        };

        var result = validator.TestValidate(request);
        if (passes)
        {
            result.ShouldNotHaveValidationErrorFor(r => r.Expires);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(r => r.Expires);
        }
    }

    [Fact]
    public void Expires_BeforeActivates_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var activates = DefaultUtcNow.AddDays(2);
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = activates,
            Expires = activates.AddSeconds(-1),
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must be after the activates date.");
    }

    [Theory]
    [InlineData("2025-10-01T14:00:00 +00:00")]
    [InlineData("2025-10-01T14:00:01 +00:00")]
    [InlineData("2025-10-08T13:59:59 +00:00")]
    [InlineData("2025-10-08T14:00:00 +00:00")]
    public void Expires_NoActivates_Within7Days_Passes(string expires)
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = null,
            Expires = DateTimeOffset.Parse(expires),
        };

        var result = validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(r => r.Expires);
    }

    [Fact]
    public void Expires_NoActivates_InPast_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = null,
            Expires = DefaultUtcNow.AddSeconds(-1),
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Expires).WithErrorMessage("Expires date must not be in the past.");
    }

    [Fact]
    public void Expires_NoActivates_After7Days_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = null,
            Expires = DefaultUtcNow.AddDays(7).AddSeconds(1),
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must be no more than 7 days from today.");
    }
}

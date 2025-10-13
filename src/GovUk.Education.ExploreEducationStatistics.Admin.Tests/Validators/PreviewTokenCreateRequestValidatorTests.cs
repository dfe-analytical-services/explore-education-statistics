using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using Microsoft.Extensions.Time.Testing;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Validators;

public class PreviewTokenCreateRequestValidatorTests
{
    // Fixed "now" for all tests: 2025-10-01T14:00:00Z
    private static readonly DateTimeOffset FixedUtcNow = new(2025, 10, 1, 14, 0, 0, TimeSpan.Zero);

    private static TimeProvider GetTimeProvider() => new FakeTimeProvider(FixedUtcNow);

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
            Activates = FixedUtcNow.AddDays(7).AddSeconds(1),
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
        var testDate = FixedUtcNow;
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = testDate,
            Expires = testDate.AddDays(8),
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must be no more than 7 days after the activates date.");
    }

    [Theory]
    [InlineData("2025-10-01T14:00:00 +00:00", "2025-10-01T14:00:00 +00:00", false)]
    [InlineData("2025-10-01T14:00:00 +00:00", "2025-10-01T14:00:01 +00:00", true)]
    [InlineData("2025-10-01T14:00:00 +00:00", "2025-10-07T22:59:00 +00:00", true)] // The expires date converts to October 7, 2025 at 23:59 BST (British Summer Time)
    public void Expires_BetweenActivatesAndActivatesPlus7Days_Passes(string activates, string expires, bool passes)
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
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
            result
                .ShouldHaveValidationErrorFor(r => r.Expires)
                .WithErrorMessage("Activates date must be before the expires date.");
        }
    }

    [Fact]
    public void Expires_BeforeActivates_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var activates = FixedUtcNow.AddDays(2);
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
            .WithErrorMessage("Activates date must be before the expires date.");
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
            Expires = FixedUtcNow.AddSeconds(-1),
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
            Expires = FixedUtcNow.AddDays(7).AddSeconds(1),
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must be no more than 7 days from today.");
    }
}

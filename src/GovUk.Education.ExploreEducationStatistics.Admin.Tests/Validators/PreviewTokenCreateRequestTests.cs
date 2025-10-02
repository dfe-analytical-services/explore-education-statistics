using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Validators;

public class PreviewTokenCreateRequestTests
{
    // Fixed "now" for all tests: 2025-10-01T14:00:00Z
    private static readonly DateTimeOffset FixedUtcNow = new(2025, 10, 1, 14, 0, 0, TimeSpan.Zero);

    private static TimeProvider GetTimeProvider() => new FakeTimeProvider(FixedUtcNow);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Activates_NotInPast_Passes(int daysAdded)
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = FixedUtcNow.AddDays(daysAdded),
            Expires = FixedUtcNow.AddDays(1 + daysAdded)
        };

        var result = validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(r => r.Activates);
    }

    [Fact]
    public void Activates_InPast_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = FixedUtcNow.AddSeconds(-11),
            Expires = FixedUtcNow.AddDays(1)
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Activates)
            .WithErrorMessage("Activates date must not be in the past.");
    }

    [Fact]
    public void Activates_Within7Days_Passes()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = FixedUtcNow.AddDays(6),
            Expires = FixedUtcNow.AddDays(7)
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
            Expires = FixedUtcNow.AddDays(9)
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Activates)
            .WithErrorMessage("Activates date must be within the next 7 days.");
    }

    [Fact]
    public void Expires_BetweenActivatesAndActivatesPlus7Days_Passes()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = FixedUtcNow.AddDays(1),
            Expires = FixedUtcNow.AddDays(5)
        };

        var result = validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(r => r.Expires);
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
            Expires = activates.AddSeconds(-1)
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r)
            .WithErrorMessage("Activates date must be before or equal to the expires date.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Expires_MoreThan7DaysAfterActivates_FailsIfOverBySecond(int second)
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var testDate = FixedUtcNow;
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = testDate,
            Expires = testDate.AddDays(7).AddSeconds(second)
        };

        var result = validator.TestValidate(request);
        if (second > 0)
        {
            result.ShouldHaveValidationErrorFor(r => r)
                .WithErrorMessage("Expires date must be no more than 7 days after the activates date.");
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(r => r.Expires);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(7)]
    public void Expires_NoActivates_NotInPast_Passes(int daysAdded)
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = null,
            Expires = FixedUtcNow.AddDays(daysAdded)
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
            Expires = FixedUtcNow.AddDays(-1)
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must not be in the past.");
    }

    [Fact]
    public void Expires_NoActivates_MoreThan7DaysFromToday_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = null,
            Expires = FixedUtcNow.AddDays(7).AddSeconds(1)
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must be no more than 7 days from today.");
    }

    // Helper for mocking TimeProvider
    private class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}

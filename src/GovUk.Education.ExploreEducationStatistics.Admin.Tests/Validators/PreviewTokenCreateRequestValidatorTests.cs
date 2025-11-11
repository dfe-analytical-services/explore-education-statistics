using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Time.Testing;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Validators;

public class PreviewTokenCreateRequestValidatorTests
{
    private static readonly DateTimeOffset DefaultBritishTimeNow = new(2025, 10, 1, 14, 0, 0, TimeSpan.FromHours(1));

    private static FakeTimeProvider GetTimeProvider(DateTimeOffset? startDateTime = null) =>
        new(startDateTime ?? DefaultBritishTimeNow);

    [Theory]
    [InlineData("2025-09-30T14:00:00 +01:00")]
    [InlineData("2025-10-01T13:59:49 +01:00")] // 1 second outside the 10-second tolerance
    public void Activates_WithoutExpires_InPast_Fails(string activates)
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
    [InlineData("2025-10-01T14:00:00 +01:00")]
    [InlineData("2025-10-01T13:59:55 +01:00")]
    public void Activates_WithoutExpires_ActivatesIsNow_Passes(string activates)
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
    [InlineData("2025-10-07T23:00:00 +00:00")] // This, in BST is 2025-10-08T00:00:00
    [InlineData("2025-10-08T00:00:00 +01:00")]
    [InlineData("2025-10-08T13:59:59 +00:00")]
    [InlineData("2025-10-08T14:00:00 +00:00")]
    public void Activates_WithoutExpires_Within7Days_Passes(string activates)
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

        Assert.False(ContainsUnexpectedErrors(result));
    }

    [Theory]
    [InlineData("2025-10-08T14:00:00 +01:00", true)]
    [InlineData("2025-10-08T23:59:59 +01:00", true)]
    [InlineData("2025-10-09T00:00:00 +01:00", false)]
    [InlineData("2025-10-08T14:00:00 +00:00", true)]
    [InlineData("2025-10-08T23:59:59 +00:00", true)]
    [InlineData("2025-10-09T00:00:00 +00:00", false)]
    public void Activates_WithoutExpires_After7Days_PassesWithinBoundary(string activatesInput, bool shouldPass)
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var activates = DateTimeOffset.Parse(activatesInput);

        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = new DateTimeOffset(activates.Year, activates.Month, activates.Day, 0, 0, 0, activates.Offset),
            Expires = null,
        };

        var result = validator.TestValidate(request);

        if (shouldPass)
        {
            Assert.False(ContainsUnexpectedErrors(result));
        }
        else
        {
            result
                .ShouldHaveValidationErrorFor(r => r.Activates)
                .WithErrorMessage("Activates date must be within the next 7 days.");
        }
    }

    [Fact]
    public void Activates_WithoutExpires_NotTodayAndAtMidnightBstTime_IsValid()
    {
        var timeProvider = GetTimeProvider();
        var validator = new PreviewTokenCreateRequest.Validator(timeProvider);

        var activates = new DateTimeOffset(2025, 10, 4, 0, 0, 0, TimeSpan.FromHours(1)); // Must set offset to +01:00
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test Label",
            Activates = activates,
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r.Activates);
    }

    [Fact]
    public void Activates_WithoutExpires_NotTodayAndAtMidnightNonBstTime_IsValid()
    {
        var currentTime = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = GetTimeProvider(currentTime);
        var validator = new PreviewTokenCreateRequest.Validator(timeProvider);

        // Activates date is GMT (not BST) and has the offset 0 based on the time and place (UK) this test is based on.
        var activates = new DateTimeOffset(2025, 1, 16, 0, 0, 0, TimeSpan.Zero);
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test Label",
            Activates = activates,
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r.Activates);
    }

    [Fact]
    public void Activates_WithoutExpires_TodayAndNotAtMidnight_IsValid()
    {
        var timeProvider = GetTimeProvider();
        var validator = new PreviewTokenCreateRequest.Validator(timeProvider);

        // Activates date is same as current date
        var activates = DefaultBritishTimeNow.AddHours(2);
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test Label",
            Activates = activates,
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r.Activates);
    }

    [Fact]
    public void Activates_WithoutExpires_NotTodayButNotMidnight_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());

        var activates = DefaultBritishTimeNow.AddDays(1).AddMinutes(20);
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test Label",
            Activates = activates,
        };

        var result = validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(r => r.Activates)
            .WithErrorMessage("Activates time must be set to midnight UK local time when it's not today's date.");
    }

    [Fact]
    public void Expires_WithActivates_MoreThan7DaysAfterActivates_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = DefaultBritishTimeNow,
            Expires = DefaultBritishTimeNow.AddDays(8),
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must be no more than 7 days after the activates date.");
    }

    [Fact]
    public void Expires_WithActivates_SameTimeStampAsActivates_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = DefaultBritishTimeNow,
            Expires = DefaultBritishTimeNow,
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must not be the same dates as the activates date.");
    }

    [Theory]
    // === 1. GMT -> GMT (Winter):
    // Activates: 2025-01-01. Max Valid Date: 2025-01-08 (until 23:59:59 +00:00).
    [InlineData(
        "2025-01-01T00:00:00 +00:00",
        "2025-01-08T23:59:59 +00:00",
        true,
        "GMT->GMT (Boundary): Last moment of the 7th day (Valid)"
    )]
    [InlineData(
        "2025-01-01T00:00:00 +00:00",
        "2025-01-09T00:00:00 +00:00",
        false,
        "GMT->GMT (Boundary): First moment of Day 8 (Invalid)"
    )]
    // === 2. BST -> BST (Summer):
    // Activates: 2025-10-03. Max Valid Date: 2025-10-10 (until 23:59:59 +01:00).
    [InlineData(
        "2025-10-03T00:00:00 +01:00",
        "2025-10-10T23:59:59 +01:00",
        true,
        "BST->BST (Boundary): Last moment of the 7th day (Valid)"
    )]
    [InlineData(
        "2025-10-03T00:00:00 +01:00",
        "2025-10-11T00:00:00 +01:00",
        false,
        "BST->BST (Boundary): First moment of Day 8 (Invalid)"
    )]
    // === 3. Spring forward (GMT -> BST):
    // Activates: 2025-03-28 (GMT). Max Valid Date: 2025-04-04 (BST).
    // Day 8 starts on 2025-04-05, which is in BST (+01:00).
    [InlineData(
        "2025-03-28T00:00:00 +00:00",
        "2025-04-04T23:59:59 +01:00",
        true,
        "GMT->BST (Boundary): Last moment of the 7th day (Valid)"
    )]
    [InlineData(
        "2025-03-28T00:00:00 +00:00",
        "2025-04-05T00:00:00 +01:00",
        false,
        "GMT->BST (Boundary): First moment of Day 8 (Invalid in BST)"
    )]
    // === 4. Autumn back (BST -> GMT):
    // Activates: 2025-10-20 (BST). Max Valid Date: 2025-10-27 (GMT).
    [InlineData(
        "2025-10-20T00:00:00 +01:00",
        "2025-10-27T23:59:59 +00:00",
        true,
        "BST->GMT (Correction): Exactly 7 days is on Day 7, so it is valid (TRUE)"
    )]
    [InlineData(
        "2025-10-20T00:00:00 +01:00",
        "2025-10-28T00:00:00 +00:00",
        false,
        "BST->GMT (Boundary): First moment of Day 8 (Invalid in GMT)"
    )]
    public void Expires_WithActivates_ValidUpTo7DaysAfterActivates(
        string activates,
        string expires,
        bool passes,
        string _
    )
    {
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
            result
                .ShouldHaveValidationErrorFor(r => r.Expires)
                .WithErrorMessage("Expires date must be no more than 7 days after the activates date.");
            ;
        }
    }

    [Fact]
    public void Expires_WithActivates_BeforeActivates_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var activates = DefaultBritishTimeNow.AddDays(2);
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
    [InlineData(null, "2025-10-01T14:00:00 +00:00", true)]
    [InlineData(null, "2025-10-01T14:00:01 +00:00", true)]
    [InlineData(null, "2025-10-08T13:59:59 +00:00", true)]
    [InlineData(null, "2025-10-08T14:00:00 +00:00", true)]
    [InlineData(null, "2025-10-08T23:00:00 +00:00", false)] // 2025-10-08T23:00:00 == 2025-10-09T00:00:00. End of day of 025-10-09 is 025-10-09T23:59:59 local time.
    [InlineData("2025-01-01T00:00:00 +00:00", "2025-01-09T00:00:00 +00:00", false)]
    [InlineData("2025-01-01T00:00:00 +00:00", "2025-01-08T23:59:59 +00:00", true)]
    public void Expires_WithoutActivates_Within7Days_PassesIfInBoundary(
        [CanBeNull] string mockNow,
        string expires,
        bool passes
    )
    {
        var timeProvider = mockNow is null ? GetTimeProvider() : GetTimeProvider(DateTimeOffset.Parse(mockNow));
        var validator = new PreviewTokenCreateRequest.Validator(timeProvider);
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = null,
            Expires = DateTimeOffset.Parse(expires),
        };

        var result = validator.TestValidate(request);
        Assert.Equal(passes, !ContainsUnexpectedErrors(result));
    }

    [Fact]
    public void Expires_WithoutActivates_InPast_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = null,
            Expires = DefaultBritishTimeNow.AddSeconds(-1),
        };

        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.Expires).WithErrorMessage("Expires date must not be in the past.");
    }

    [Fact]
    public void Expires_WithoutActivates_After7Days_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var eightDaysFromNow = DefaultBritishTimeNow.AddDays(8);
        var expires = new DateTimeOffset(
            eightDaysFromNow.Year,
            eightDaysFromNow.Month,
            eightDaysFromNow.Day,
            0,
            0,
            0,
            eightDaysFromNow.Offset
        );
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = null,
            Expires = expires,
        };

        var result = validator.TestValidate(request);
        result
            .ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires date must be no more than 7 days from today.");
    }

    [Fact]
    public void Expires_EndOfDayBstTime_IsValid()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var activates = new DateTimeOffset(2025, 10, 5, 23, 0, 0, TimeSpan.Zero); // Midnight BST tomorrow

        // End of day BST (22:59:59 UTC = 23:59:59 BST)
        var expires = new DateTimeOffset(2025, 10, 6, 22, 59, 59, TimeSpan.Zero);
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test Label",
            Activates = activates,
            Expires = expires,
        };

        var result = validator.TestValidate(request);

        Assert.False(ContainsUnexpectedErrors(result));
    }

    [Fact]
    public void Expires_EndOfDayNonBstTime_IsValid()
    {
        var currentTime = new DateTimeOffset(2025, 1, 14, 10, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(currentTime);
        var validator = new PreviewTokenCreateRequest.Validator(timeProvider);
        var activates = new DateTimeOffset(2025, 1, 15, 23, 0, 0, TimeSpan.Zero);

        var expires = new DateTimeOffset(2025, 1, 16, 23, 59, 59, TimeSpan.Zero);
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test Label",
            Activates = activates,
            Expires = expires,
        };

        var result = validator.TestValidate(request);

        Assert.False(ContainsUnexpectedErrors(result));
    }

    private static bool ContainsUnexpectedErrors(TestValidationResult<PreviewTokenCreateRequest> result)
    {
        var unexpectedErrors = result.Errors.Count(e =>
            e.ErrorMessage != "Activates time must be set to midnight UK local time when it's not today's date."
            && e.ErrorMessage != "Expires time must be at 23:59:59 UK local time for that date."
        );
        return unexpectedErrors > 0;
    }

    [Fact]
    public void Expires_NotEndOfDayBstTime_Fails()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var activates = new DateTimeOffset(2025, 10, 6, 23, 0, 0, TimeSpan.Zero); // Midnight BST tomorrow

        // End at 10 PM BST (21:00 UTC) instead of end of day
        var expires = new DateTimeOffset(2025, 10, 7, 21, 0, 0, TimeSpan.Zero);
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test Label",
            Activates = activates,
            Expires = expires,
        };

        var result = validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(r => r.Expires)
            .WithErrorMessage("Expires time must be at 23:59:59 UK local time for that date.");
    }

    [Fact]
    public void Success()
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var activates = DefaultBritishTimeNow.AddHours(1);
        var expires = DefaultBritishTimeNow.AddDays(3);
        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test Label",
            Activates = activates,
            Expires = expires,
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

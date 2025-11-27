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
    [InlineData("2025-09-30T17:00:00 +04:00")]
    [InlineData("2025-09-30T09:00:00 -04:00")]
    [InlineData("2025-10-01T13:59:49 +01:00")] // 1 second outside the 10-second tolerance
    [InlineData("2025-10-01T16:59:49 +04:00")]
    [InlineData("2025-10-01T08:59:49 -04:00")]
    public void ActivatesInPast_Fails(string activates)
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
    [InlineData("2025-10-01T17:00:00 +04:00")]
    [InlineData("2025-10-01T09:00:00 -04:00")]
    [InlineData("2025-10-01T13:59:55 +01:00")] // inside 10 second tolerance
    [InlineData("2025-10-01T16:59:55 +04:00")]
    [InlineData("2025-10-01T08:59:55 -04:00")]
    public void ActivatesIsNow_Passes(string activates)
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

    [Theory]
    [InlineData("2025-10-09T00:00:00 +01:00")]
    [InlineData("2025-10-09T03:00:00 +04:00")] // same instant as +01:00
    [InlineData("2025-10-08T19:00:00 -04:00")] // same instant as +01:00
    public void ActivatesAfter7Days_Fails(string activatesInput)
    {
        var validator = new PreviewTokenCreateRequest.Validator(GetTimeProvider());
        var activates = DateTimeOffset.Parse(activatesInput);

        var request = new PreviewTokenCreateRequest
        {
            DataSetVersionId = Guid.NewGuid(),
            Label = "Test",
            Activates = activates,
            Expires = null,
        };

        var result = validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(r => r.Activates)
            .WithErrorMessage("Activates date must be within the next 7 days.");
    }

    public class ActivatesWithin7DaysTests
    {
        [Theory]
        [InlineData("2025-10-01T14:00:00 +00:00")]
        [InlineData("2025-10-01T18:00:00 +04:00")]
        [InlineData("2025-10-01T10:00:00 -04:00")]
        [InlineData("2025-10-01T14:00:01 +00:00")]
        [InlineData("2025-10-01T18:00:01 +04:00")]
        [InlineData("2025-10-01T10:00:01 -04:00")]
        [InlineData("2025-10-07T23:00:00 +00:00")] // This, in BST is 2025-10-08T00:00:00
        [InlineData("2025-10-08T00:00:00 +01:00")]
        [InlineData("2025-10-08T03:00:00 +04:00")]
        [InlineData("2025-10-07T19:00:00 -04:00")]
        [InlineData("2025-10-08T13:59:59 +00:00")]
        [InlineData("2025-10-08T17:59:59 +04:00")]
        [InlineData("2025-10-08T09:59:59 -04:00")]
        [InlineData("2025-10-08T14:00:00 +00:00")]
        [InlineData("2025-10-08T18:00:00 +04:00")]
        [InlineData("2025-10-08T10:00:00 -04:00")]
        [InlineData("2025-10-01T16:00:00 +01:00")]
        [InlineData("2025-10-01T19:00:00 +04:00")]
        [InlineData("2025-10-01T11:00:00 -04:00")]
        public void ActivatesWithin7Days_Passes(string activates)
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

            Assert.False(ContainsUnexpectedErrors(result)); // Testing that Activates without expires passes at the boundary level (disregarding error 'Activates time must be set to midnight UK local time when it's not today's date.')
        }

        [Fact]
        public void ActivatesNotTodayAndAtMidnightBstTime_IsValid()
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
        public void ActivatesNotTodayAndAtMidnightNonBstTime_IsValid()
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
        public void ActivatesNotTodayButNotMidnight_Fails()
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
    }

    [Fact]
    public void ExpiresInPast_Fails()
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
    public void ExpiresBeforeActivates_Fails()
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

    [Fact]
    public void ExpiresSameTimeStampAsActivates_Fails()
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
            .WithErrorMessage("Expires time must always be at 23:59:59 UK local time.");
    }

    public class ExpiresWithin7DaysOfNowWithoutActivatesTests
    {
        [Theory]
        [InlineData(null, "2025-10-01T14:00:00 +00:00", true)]
        [InlineData(null, "2025-10-01T18:00:00 +04:00", true)] // 14:00Z == 18:00(+04)
        [InlineData(null, "2025-10-01T10:00:00 -04:00", true)] // 14:00Z == 10:00(-04)
        [InlineData(null, "2025-10-01T14:00:01 +00:00", true)]
        [InlineData(null, "2025-10-01T18:00:01 +04:00", true)] // 14:00:01Z == 18:00:01(+04)
        [InlineData(null, "2025-10-01T10:00:01 -04:00", true)] // 14:00:01Z == 10:00:01(-04)
        [InlineData(null, "2025-10-08T13:59:59 +00:00", true)]
        [InlineData(null, "2025-10-08T17:59:59 +04:00", true)] // 13:59:59Z == 17:59:59(+04)
        [InlineData(null, "2025-10-08T09:59:59 -04:00", true)] // 13:59:59Z == 09:59:59(-04)
        [InlineData(null, "2025-10-08T14:00:00 +00:00", true)]
        [InlineData(null, "2025-10-08T18:00:00 +04:00", true)] // 14:00Z == 18:00(+04)
        [InlineData(null, "2025-10-08T10:00:00 -04:00", true)] // 14:00Z == 10:00(-04)
        [InlineData(null, "2025-10-08T23:00:00 +00:00", false)] // Expires at 23:00 UTC, which is 00:00 on 9 Oct in UK (BST).
        // Since this falls at the start of 9 Oct, it is *not* within the end-of-day window for 8 Oct.
        [InlineData(null, "2025-10-09T03:00:00 +04:00", false)] // 23:00Z == 03:00(+04) on 9 Oct
        [InlineData(null, "2025-10-08T19:00:00 -04:00", false)] // 23:00Z == 19:00(-04) on 8 Oct
        [InlineData("2025-01-01T00:00:00 +00:00", "2025-01-09T00:00:00 +00:00", false)]
        // Same instants in +04:00 / -04:00
        [InlineData("2025-01-01T04:00:00 +04:00", "2025-01-09T04:00:00 +04:00", false)] // 00:00Z == 04:00(+04)
        [InlineData("2024-12-31T20:00:00 -04:00", "2025-01-08T20:00:00 -04:00", false)] // 00:00Z == 20:00(-04) prev day
        [InlineData("2025-01-01T00:00:00 +00:00", "2025-01-08T23:59:59 +00:00", true)]
        // Same instants in +04:00 / -04:00
        [InlineData("2025-01-01T04:00:00 +04:00", "2025-01-09T03:59:59 +04:00", true)] // 23:59:59Z == 03:59:59(+04) on 9 Jan
        [InlineData("2024-12-31T20:00:00 -04:00", "2025-01-08T19:59:59 -04:00", true)] // 23:59:59Z == 19:59:59(-04) on 8 Jan
        public void ExpiresWithin7Days_PassesIfInBoundary([CanBeNull] string mockNow, string expires, bool passes)
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
        public void ExpiresAfter7Days_Fails()
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
        public void ExpiresAfter7DaysOfActivates_Fails()
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
    }

    public class ExpiresWithin7DaysOfActivatesWithActivatesTests
    {
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
        // +04:00 / -04:00 variants of the same instants
        [InlineData(
            "2025-01-01T04:00:00 +04:00",
            "2025-01-09T03:59:59 +04:00",
            true,
            "GMT->GMT as +04:00 offset: Last moment of the 7th day (Valid, same instant)"
        )]
        [InlineData(
            "2025-01-01T04:00:00 +04:00",
            "2025-01-09T04:00:00 +04:00",
            false,
            "GMT->GMT as +04:00 offset: First moment of Day 8 (Invalid, same instant)"
        )]
        [InlineData(
            "2024-12-31T20:00:00 -04:00",
            "2025-01-08T19:59:59 -04:00",
            true,
            "GMT->GMT as -04:00 offset: Last moment of the 7th day (Valid, same instant)"
        )]
        [InlineData(
            "2024-12-31T20:00:00 -04:00",
            "2025-01-08T20:00:00 -04:00",
            false,
            "GMT->GMT as -04:00 offset: First moment of Day 8 (Invalid, same instant)"
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
        // +04:00 / -04:00 variants of the same instants
        [InlineData(
            "2025-10-03T03:00:00 +04:00",
            "2025-10-11T02:59:59 +04:00",
            true,
            "BST->BST as +04:00 offset: Last moment of the 7th day (Valid, same instant)"
        )]
        [InlineData(
            "2025-10-03T03:00:00 +04:00",
            "2025-10-11T03:00:00 +04:00",
            false,
            "BST->BST as +04:00 offset: First moment of Day 8 (Invalid, same instant)"
        )]
        [InlineData(
            "2025-10-02T19:00:00 -04:00",
            "2025-10-10T18:59:59 -04:00",
            true,
            "BST->BST as -04:00 offset: Last moment of the 7th day (Valid, same instant)"
        )]
        [InlineData(
            "2025-10-02T19:00:00 -04:00",
            "2025-10-10T19:00:00 -04:00",
            false,
            "BST->BST as -04:00 offset: First moment of Day 8 (Invalid, same instant)"
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
        // +04:00 / -04:00 variants of the same instants
        [InlineData(
            "2025-03-28T04:00:00 +04:00",
            "2025-04-05T02:59:59 +04:00",
            true,
            "GMT->BST as +04:00 offset: Last moment of the 7th day (Valid, same instant)"
        )]
        [InlineData(
            "2025-03-28T04:00:00 +04:00",
            "2025-04-05T03:00:00 +04:00",
            false,
            "GMT->BST as +04:00 offset: First moment of Day 8 (Invalid, same instant)"
        )]
        [InlineData(
            "2025-03-27T20:00:00 -04:00",
            "2025-04-04T18:59:59 -04:00",
            true,
            "GMT->BST as -04:00 offset: Last moment of the 7th day (Valid, same instant)"
        )]
        [InlineData(
            "2025-03-27T20:00:00 -04:00",
            "2025-04-04T19:00:00 -04:00",
            false,
            "GMT->BST as -04:00 offset: First moment of Day 8 (Invalid, same instant)"
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
        // +04:00 / -04:00 variants of the same instants
        [InlineData(
            "2025-10-20T03:00:00 +04:00",
            "2025-10-28T03:59:59 +04:00",
            true,
            "BST->GMT as +04:00 offset: Last moment of the 7th day (Valid, same instant)"
        )]
        [InlineData(
            "2025-10-20T03:00:00 +04:00",
            "2025-10-28T04:00:00 +04:00",
            false,
            "BST->GMT as +04:00 offset: First moment of Day 8 (Invalid, same instant)"
        )]
        [InlineData(
            "2025-10-19T19:00:00 -04:00",
            "2025-10-27T19:59:59 -04:00",
            true,
            "BST->GMT as -04:00 offset: Last moment of the 7th day (Valid, same instant)"
        )]
        [InlineData(
            "2025-10-19T19:00:00 -04:00",
            "2025-10-27T20:00:00 -04:00",
            false,
            "BST->GMT as -04:00 offset: First moment of Day 8 (Invalid, same instant)"
        )]
        public void ExpiresValidUpTo7DaysAfterActivates(string activates, string expires, bool passes, string _)
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
                Assert.False(
                    ContainsUnexpectedErrors(
                        result,
                        "Expires date must be no more than 7 days after the activates date."
                    )
                );
            }
        }
    }

    [Fact]
    public void ExpiresEndOfDayBstTime_IsValid()
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
    public void ExpiresEndOfDayNonBstTime_IsValid()
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

    [Fact]
    public void ExpiresNotEndOfDayBstTime_Fails()
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
            .WithErrorMessage("Expires time must always be at 23:59:59 UK local time.");
    }

    private static bool ContainsUnexpectedErrors(
        TestValidationResult<PreviewTokenCreateRequest> result,
        string expectedErrorMessage = null
    )
    {
        var unexpectedErrors = result.Errors.Count(e =>
            e.ErrorMessage != "Activates time must be set to midnight UK local time when it's not today's date."
            && e.ErrorMessage != "Expires time must always be at 23:59:59 UK local time."
            && e.ErrorMessage != expectedErrorMessage
        );
        return unexpectedErrors > 0;
    }
}

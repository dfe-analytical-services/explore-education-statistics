#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ReleaseVersionsMigration.Dtos;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public class ReleaseVersionsMigrationReportWarningsDtoTests
{
    public static TheoryData<ReleaseVersionsMigrationReportWarningsDto> ReportsWithSingleWarnings
    {
        get
        {
            var baseReport = new ReleaseVersionsMigrationReportWarningsDto
            {
                NoSuccessfulPublishingAttempts = null,
                MultipleSuccessfulPublishingAttempts = null,
                UpdatesCountDoesNotMatchVersion = null,
                ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate = null,
                ProposedPublishedDateIsNotSimilarToScheduledTriggerDate = null,
                ScheduledTriggerDateIsNotMidnightUkLocalTime = null,
            };

            return
            [
                baseReport with
                {
                    NoSuccessfulPublishingAttempts = true,
                },
                baseReport with
                {
                    MultipleSuccessfulPublishingAttempts = true,
                },
                baseReport with
                {
                    UpdatesCountDoesNotMatchVersion = true,
                },
                baseReport with
                {
                    ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate = true,
                },
                baseReport with
                {
                    ProposedPublishedDateIsNotSimilarToScheduledTriggerDate = true,
                },
                baseReport with
                {
                    ScheduledTriggerDateIsNotMidnightUkLocalTime = true,
                },
            ];
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public void WhenReportHasNullWarnings_ThenHasWarningsIsFalse(bool? warning)
    {
        var report = new ReleaseVersionsMigrationReportWarningsDto
        {
            NoSuccessfulPublishingAttempts = warning,
            MultipleSuccessfulPublishingAttempts = warning,
            UpdatesCountDoesNotMatchVersion = warning,
            ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate = warning,
            ProposedPublishedDateIsNotSimilarToScheduledTriggerDate = warning,
            ScheduledTriggerDateIsNotMidnightUkLocalTime = warning,
        };

        Assert.False(report.HasWarnings);
    }

    [Theory]
    [MemberData(nameof(ReportsWithSingleWarnings))]
    public void WhenReportHasAnyWarnings_ThenHasWarningsIsTrue(ReleaseVersionsMigrationReportWarningsDto report)
    {
        // The member data provides a set of Warning reports with each one having an individual warning property set to
        // true, with the remaining warnings all set to null.

        // This test ensures HasWarnings is true when any of the warnings are true,
        // and that it is covering all the individual warning properties.
        Assert.True(report.HasWarnings);
    }
}

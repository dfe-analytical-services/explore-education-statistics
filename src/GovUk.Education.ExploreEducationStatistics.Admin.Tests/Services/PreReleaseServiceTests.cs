#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PreReleaseServiceTests
{
    private static readonly DateTime DefaultScheduledPublishDate = new (2003, 11, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime DefaultActuallyPublishedDate = new (2003, 11, 15, 9, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime DateWithinPreReleaseAccessWindow = new (2003, 11, 14, 10, 24, 0, DateTimeKind.Utc);

    private readonly PreReleaseService _service = new (new PreReleaseAccessOptions
    {
        AccessWindow = new AccessWindowOptions
        {
            MinutesBeforeReleaseTimeStart = 1440,
        }
    }.ToOptionsWrapper());


    [Theory]
    [InlineData("2003-11-10T00:00:00.00Z", false, PreReleaseAccess.Before)]
    [InlineData("2003-11-13T23:59:00.00Z", false,PreReleaseAccess.Before)]
    [InlineData("2003-11-14T00:00:00.00Z", false,PreReleaseAccess.Within)]
    [InlineData("2003-11-14T10:24:00.00Z", false,PreReleaseAccess.Within)]
    [InlineData("2003-11-14T23:58:00.00Z", false,PreReleaseAccess.Within)]
    [InlineData("2003-11-14T23:59:00.00Z", false,PreReleaseAccess.Within)]
    [InlineData("2003-11-15T00:00:00.00Z", false,PreReleaseAccess.Within)]
    [InlineData("2003-11-15T02:30:00.00Z", false,PreReleaseAccess.Within)]
    [InlineData("2003-11-15T02:30:00.00Z", true,PreReleaseAccess.After)]
    [InlineData("2003-11-15T08:45:00.00Z", false,PreReleaseAccess.Within)]
    [InlineData("2003-11-15T09:32:00.00Z", false,PreReleaseAccess.Within)]
    [InlineData("2003-11-15T09:32:00.00Z", true,PreReleaseAccess.After)]
    public void CalculatesAccessRangeAsExpected(string referenceTimeString, bool releaseHasBeenPublished, PreReleaseAccess expectedAccess)
    {
        var releaseVersion = new ReleaseVersion
        {
            PublishScheduled = DefaultScheduledPublishDate,
            Published = releaseHasBeenPublished ? DefaultActuallyPublishedDate : null,
            ApprovalStatus = ReleaseApprovalStatus.Approved
        };

        var referenceTime = DateTime.Parse(referenceTimeString, styles: DateTimeStyles.AdjustToUniversal);

        var result = _service.GetPreReleaseWindowStatus(releaseVersion, referenceTime);

        Assert.Equal(expectedAccess, result.Access);
    }

    [Fact]
    public void CalculatesStartAndScheduledPublishDatesAsExpected()
    {
        var testReleaseVersion = new ReleaseVersion
        {
            PublishScheduled = DefaultScheduledPublishDate,
            Published = null,
            ApprovalStatus = ReleaseApprovalStatus.Approved
        };

        var result = _service.GetPreReleaseWindowStatus(testReleaseVersion, DateWithinPreReleaseAccessWindow);

        Assert.Equal(PreReleaseAccess.Within, result.Access);
        Assert.Equal(new DateTime(2003, 11, 14, 0, 0, 0, DateTimeKind.Utc), result.Start);
        Assert.Equal(DefaultScheduledPublishDate, result.ScheduledPublishDate);
    }

    [Fact]
    public void AccessIsNoneSetIfReleaseIsStillInReview()
    {
        var testReleaseVersionInReview = new ReleaseVersion
        {
            PublishScheduled = DefaultScheduledPublishDate,
            Published = null,
            ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview
        };

        var preReleaseWindowStatus =
            _service.GetPreReleaseWindowStatus(testReleaseVersionInReview, DateWithinPreReleaseAccessWindow);
        Assert.Equal(PreReleaseAccess.NoneSet, preReleaseWindowStatus.Access);
    }

    [Fact]
    public void AccessIsNoneSetIfPublishedScheduleIsStillInDraft()
    {
        var testReleaseVersionInDraft = new ReleaseVersion
        {
            PublishScheduled = DefaultScheduledPublishDate,
            Published = null,
            ApprovalStatus = ReleaseApprovalStatus.Draft
        };

        var preReleaseWindowStatus =
            _service.GetPreReleaseWindowStatus(testReleaseVersionInDraft, DateWithinPreReleaseAccessWindow);
        Assert.Equal(PreReleaseAccess.NoneSet, preReleaseWindowStatus.Access);
    }

    [Fact]
    public void AccessIsAfterIfReleaseIsLiveButPublishedScheduleHasNoValue()
    {
        var releaseVersionWithNoScheduledPublishDate = new ReleaseVersion
        {
            PublishScheduled = null,
            Published = DefaultActuallyPublishedDate
        };

        var preReleaseWindowStatus =
            _service.GetPreReleaseWindowStatus(releaseVersionWithNoScheduledPublishDate, DateTime.UtcNow);

        Assert.Equal(PreReleaseAccess.After, preReleaseWindowStatus.Access);
    }
}

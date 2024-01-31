using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using Microsoft.Extensions.Options;
using Xunit;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PreReleaseServiceTests
    {
        private static readonly DateTime DefaultScheduledPublishDate =
            DateTime.Parse("2003-11-15T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal);

        private static readonly DateTime DefaultActuallyPublishedDate =
            DateTime.Parse("2003-11-15T09:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal);

        private static readonly DateTime DateWithinPreReleaseAccessWindow =
            DateTime.Parse("2003-11-14T10:24:00.00Z", styles: DateTimeStyles.AdjustToUniversal);

        private readonly PreReleaseService _sut = new (Options.Create(new PreReleaseOptions
        {
            PreReleaseAccess = new PreReleaseAccessOptions
            {
                AccessWindow = new AccessWindowOptions
                {
                    MinutesBeforeReleaseTimeStart = 1440,
                }
            }
        }));


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
            var release  = new Release()
            {
                PublishScheduled = DefaultScheduledPublishDate,
                Published = releaseHasBeenPublished ? DefaultActuallyPublishedDate : null,
                ApprovalStatus = ReleaseApprovalStatus.Approved
            };

            var referenceTime = DateTime.Parse(referenceTimeString, styles: DateTimeStyles.AdjustToUniversal);

            var result = _sut.GetPreReleaseWindowStatus(release, referenceTime);

            Assert.Equal(expectedAccess, result.Access);
        }

        [Fact]
        public void CalculatesStartAndScheduledPublishDatesAsExpected()
        {
            var testRelease = new Release()
            {
                PublishScheduled = DefaultScheduledPublishDate,
                Published = null,
                ApprovalStatus = ReleaseApprovalStatus.Approved
            };

            var result = _sut.GetPreReleaseWindowStatus(testRelease, DateTime.Parse("2003-11-14T10:24:00.00Z", styles: DateTimeStyles.AdjustToUniversal));

            Assert.Equal(PreReleaseAccess.Within, result.Access);
            Assert.Equal(DateTime.Parse("2003-11-14T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.Start);
            Assert.Equal(DefaultScheduledPublishDate, result.ScheduledPublishDate);
        }

        [Fact]
        public void AccessIsNoneSetIfPublishedScheduleIsStillInReview()
        {
            var testReleaseInReview = new Release()
            {
                PublishScheduled = DefaultScheduledPublishDate,
                Published = null,
                ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview
            };

            var preReleaseWindowStatus = _sut.GetPreReleaseWindowStatus(testReleaseInReview, DateWithinPreReleaseAccessWindow);
            Assert.Equal(PreReleaseAccess.NoneSet, preReleaseWindowStatus.Access);
        }

        [Fact]
        public void AccessIsNoneSetIfPublishedScheduleIsStillInDraft()
        {
            var testReleaseInDraft = new Release()
            {
                PublishScheduled = DefaultScheduledPublishDate,
                Published = null,
                ApprovalStatus = ReleaseApprovalStatus.Draft
            };

            var preReleaseWindowStatus = _sut.GetPreReleaseWindowStatus(testReleaseInDraft, DateWithinPreReleaseAccessWindow);
            Assert.Equal(PreReleaseAccess.NoneSet, preReleaseWindowStatus.Access);
        }

        [Fact]
        public void AccessIsAfterIfReleaseIsLiveButPublishedScheduleHasNoValue()
        {
            var releaseWithNoScheduledPublishDate = new Release()
            {
                PublishScheduled = null,
                Published = DefaultActuallyPublishedDate
            };

            var preReleaseWindowStatus = _sut.GetPreReleaseWindowStatus(releaseWithNoScheduledPublishDate, DateTime.UtcNow);

            Assert.Equal(PreReleaseAccess.After, preReleaseWindowStatus.Access);
        }
    }
}

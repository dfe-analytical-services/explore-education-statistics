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
                    MinutesBeforeReleaseTimeEnd = 1,
                    MinutesBeforeReleaseTimeStart = 1440,
                    MinutesIntoPublishDayByWhichPublishingHasOccurred = 570,
                }
            }
        }));

        private readonly Release _testRelease = new()
        {
            PublishScheduled = DefaultScheduledPublishDate,
            Published = DefaultActuallyPublishedDate,
            ApprovalStatus = ReleaseApprovalStatus.Approved
        };


        [Theory]
        [InlineData("2003-11-10T00:00:00.00Z", PreReleaseAccess.Before)]
        [InlineData("2003-11-13T23:59:00.00Z", PreReleaseAccess.Before)]
        [InlineData("2003-11-14T00:00:00.00Z", PreReleaseAccess.Within)]
        [InlineData("2003-11-14T10:24:00.00Z", PreReleaseAccess.Within)]
        [InlineData("2003-11-14T23:58:00.00Z", PreReleaseAccess.Within)]
        [InlineData("2003-11-14T23:59:00.00Z", PreReleaseAccess.WithinPublishDayLenience)]
        [InlineData("2003-11-15T00:00:00.00Z", PreReleaseAccess.WithinPublishDayLenience)]
        [InlineData("2003-11-15T02:30:00.00Z", PreReleaseAccess.WithinPublishDayLenience)]
        [InlineData("2003-11-15T08:45:00.00Z", PreReleaseAccess.WithinPublishDayLenience)]
        [InlineData("2003-11-15T09:32:00.00Z", PreReleaseAccess.After)]
        public void CalculatesAccessRangeAsExpected(string referenceTimeString, PreReleaseAccess expectedAccess)
        {
            var referenceTime = DateTime.Parse(referenceTimeString, styles: DateTimeStyles.AdjustToUniversal);

            var result = _sut.GetPreReleaseWindowStatus(_testRelease, referenceTime);

            Assert.Equal(expectedAccess, result.Access);
        }

        [Fact]
        public void CalculatesAllAccessRangeFieldsAsExpected()
        {
            var result = _sut.GetPreReleaseWindowStatus(_testRelease, DateTime.Parse("2003-11-14T10:24:00.00Z", styles: DateTimeStyles.AdjustToUniversal));

            Assert.Equal(PreReleaseAccess.Within, result.Access);
            Assert.Equal(DateTime.Parse("2003-11-14T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.Start);
            Assert.Equal(DateTime.Parse("2003-11-14T23:59:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.End);
            Assert.Equal(DateTime.Parse("2003-11-15T09:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.PublishDayLenienceDeadline);
        }

        [Fact]
        public void CalculatesLeniencePeriodWhenDateIsBST()
        {
            var testReleaseWithBSTTimes = new Release()
            {
                PublishScheduled = DateTime.Parse("2003-07-15T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                Published = DateTime.Parse("2003-07-15T09:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                ApprovalStatus = ReleaseApprovalStatus.Approved
            };

            var result = _sut.GetPreReleaseWindowStatus(testReleaseWithBSTTimes, DateTime.Parse("2003-07-14T10:24:00.00Z", styles: DateTimeStyles.AdjustToUniversal));

            Assert.Equal(PreReleaseAccess.Within, result.Access);
            Assert.Equal(DateTime.Parse("2003-07-14T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.Start);
            Assert.Equal(DateTime.Parse("2003-07-14T23:59:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.End);
            Assert.Equal(DateTime.Parse("2003-07-15T08:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.PublishDayLenienceDeadline);
        }

        [Fact]
        public void DoesNotMoveIntoNegativeTimeIfLenienceOffsetIsBelow60Minutes()
        {
            var sut = new PreReleaseService(Options.Create(new PreReleaseOptions
            {
                PreReleaseAccess = new PreReleaseAccessOptions
                {
                    AccessWindow = new AccessWindowOptions
                    {
                        MinutesBeforeReleaseTimeEnd = 1,
                        MinutesBeforeReleaseTimeStart = 1440,
                        MinutesIntoPublishDayByWhichPublishingHasOccurred = 30,
                    }
                }
            }));

            var testReleaseWithBSTTimes = new Release()
            {
                PublishScheduled = DateTime.Parse("2003-07-15T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                Published = DateTime.Parse("2003-07-15T09:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                ApprovalStatus = ReleaseApprovalStatus.Approved
            };

            var result = sut.GetPreReleaseWindowStatus(testReleaseWithBSTTimes, DateTime.Parse("2003-07-14T10:24:00.00Z", styles: DateTimeStyles.AdjustToUniversal));

            Assert.Equal(PreReleaseAccess.Within, result.Access);
            Assert.Equal(DateTime.Parse("2003-07-14T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.Start);
            Assert.Equal(DateTime.Parse("2003-07-14T23:59:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.End);
            Assert.Equal(DateTime.Parse("2003-07-15T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal), result.PublishDayLenienceDeadline);
        }

        [Fact]
        public void AccessIsNoneSetIfPublishedScheduleIsStillInReview()
        {
            var releaseWithNoInformation = new Release()
            {
                PublishScheduled = DefaultScheduledPublishDate,
                Published = DefaultActuallyPublishedDate,
                ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview
            };

            var preReleaseWindowStatus = _sut.GetPreReleaseWindowStatus(releaseWithNoInformation, DateWithinPreReleaseAccessWindow);
            Assert.Equal(PreReleaseAccess.NoneSet, preReleaseWindowStatus.Access);
        }

        [Fact]
        public void AccessIsNoneSetIfPublishedScheduleIsStillInDraft()
        {
            var releaseWithNoInformation = new Release()
            {
                PublishScheduled = DefaultScheduledPublishDate,
                Published = DefaultActuallyPublishedDate,
                ApprovalStatus = ReleaseApprovalStatus.Draft
            };

            var preReleaseWindowStatus = _sut.GetPreReleaseWindowStatus(releaseWithNoInformation, DateWithinPreReleaseAccessWindow);
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

#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class ReleaseTests
    {
        [Fact]
        public void Live_True()
        {
            var releasePublished = new Release
            {
                Published = DateTime.Now.AddDays(-1)
            };

            Assert.True(releasePublished.Live);
        }

        [Fact]
        public void Live_False_PublishedIsNull()
        {
            var releaseNoPublishedDate = new Release
            {
                Published = null
            };

            Assert.False(releaseNoPublishedDate.Live);
        }

        [Fact]
        public void Live_False_PublishedInFuture()
        {
            // Note that this should not happen, but we test the edge case.
            var releasePublishedDateInFuture = new Release
            {
                Published = DateTime.Now.AddDays(1)
            };

            Assert.False(releasePublishedDateInFuture.Live);
        }

        [Fact]
        public void NextReleaseDate_Ok()
        {
            var releaseDate = new PartialDate {Year = "2021", Month = "1"};
            var release = new Release
            {
                NextReleaseDate = releaseDate
            };

            Assert.Equal(releaseDate, release.NextReleaseDate);
        }

        [Fact]
        public void NextReleaseDate_InvalidDate()
        {
            Assert.Throws<FormatException>(
                () => new Release
                {
                    NextReleaseDate = new PartialDate {Day = "45"}
                }
            );
        }

        [Fact]
        public void ReleaseName_Ok()
        {
            // None should throw
            new Release {ReleaseName = "1990"};
            new Release {ReleaseName = "2011"};
            new Release {ReleaseName = "3000"};
            new Release {ReleaseName = ""}; // considered not set
            new Release {ReleaseName = null}; // considered not set
        }

        [Fact]
        public void ReleaseName_InvalidFormats()
        {
            Assert.Throws<FormatException>(
                () => new Release {ReleaseName = "Hello"}
            );

            Assert.Throws<FormatException>(
                () => new Release {ReleaseName = "190"}
            );

            Assert.Throws<FormatException>(
                () => new Release {ReleaseName = "ABC123"}
            );

            Assert.Throws<FormatException>(
                () => new Release {ReleaseName = "20000"}
            );
        }
    }
}
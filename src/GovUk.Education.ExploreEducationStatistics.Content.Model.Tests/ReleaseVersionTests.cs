using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class ReleaseVersionTests
    {
        [Fact]
        public void Live_True()
        {
            var releaseVersion = new ReleaseVersion
            {
                Published = DateTime.Now.AddDays(-1)
            };

            Assert.True(releaseVersion.Live);
        }

        [Fact]
        public void Live_False_PublishedIsNull()
        {
            var releaseVersion = new ReleaseVersion
            {
                Published = null
            };

            Assert.False(releaseVersion.Live);
        }

        [Fact]
        public void Live_False_PublishedInFuture()
        {
            // Note that this should not happen, but we test the edge case.
            var releaseVersion = new ReleaseVersion()
            {
                Published = DateTime.Now.AddDays(1)
            };

            Assert.False(releaseVersion.Live);
        }

        [Fact]
        public void NextReleaseDate_Ok()
        {
            var releaseDate = new PartialDate { Year = "2021", Month = "1" };
            var releaseVersion = new ReleaseVersion
            {
                NextReleaseDate = releaseDate
            };

            Assert.Equal(releaseDate, releaseVersion.NextReleaseDate);
        }

        [Fact]
        public void NextReleaseDate_InvalidDate()
        {
            Assert.Throws<FormatException>(
                () => new ReleaseVersion
                {
                    NextReleaseDate = new PartialDate { Day = "45" }
                }
            );
        }

        [Fact]
        public void ReleaseName_Ok()
        {
            // None should throw
            new ReleaseVersion { ReleaseName = "1990" };
            new ReleaseVersion { ReleaseName = "2011" };
            new ReleaseVersion { ReleaseName = "3000" };
            new ReleaseVersion { ReleaseName = "" }; // considered not set
            new ReleaseVersion { ReleaseName = null }; // considered not set
        }

        [Fact]
        public void ReleaseName_InvalidFormats()
        {
            Assert.Throws<FormatException>(
                () => new ReleaseVersion { ReleaseName = "Hello" }
            );

            Assert.Throws<FormatException>(
                () => new ReleaseVersion { ReleaseName = "190" }
            );

            Assert.Throws<FormatException>(
                () => new ReleaseVersion { ReleaseName = "ABC123" }
            );

            Assert.Throws<FormatException>(
                () => new ReleaseVersion { ReleaseName = "20000" }
            );
        }
    }
}

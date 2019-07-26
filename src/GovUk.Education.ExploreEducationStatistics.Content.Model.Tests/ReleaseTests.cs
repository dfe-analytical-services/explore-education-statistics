using System;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class ReleaseTests
    {
        [Fact]
        public void Release_Not_Live_Published_Null()
        {
            var releaseNoPublishedDate = new Release {Published = null};
            Assert.False(releaseNoPublishedDate.Live);
        }

        [Fact]
        public void Release_Not_Live_Published_In_Future()
        {
            // Note that this should not happen, but we test the edge case.
            var releasePublishedDateInFuture = new Release {Published = DateTime.Now.AddDays(1)};
            Assert.False(releasePublishedDateInFuture.Live);
        }


        [Fact]
        public void Release_Live()
        {
            var releasePublished = new Release {Published = DateTime.Now.AddDays(-1)};
            Assert.True(releasePublished.Live);
        }

        [Fact]
        public void Release_Name_Ok()
        {
            new Release {ReleaseName = "2014"}; // Should not throw
        }


        [Fact]
        public void Release_Name_Not_Ok()
        {
            try
            {
                new Release {ReleaseName = "Hello"}; // Not Ok
                Assert.True(false, "Should have failed validation");
            }
            catch (FormatException e)
            {
            }
        }

        [Fact]
        public void Release_NextReleaseDate_Ok()
        {
            new Release {NextReleaseDate = new PartialDate {Day = "01"}}; // Should not throw
        }

        [Fact]
        public void Release_NextReleaseDate_Not_Ok()
        {
            try
            {
                new Release {NextReleaseDate = new PartialDate {Day = "45"}}; // Not Ok
                Assert.True(false, "Should have failed validation");
            }
            catch (FormatException e)
            {
            }
        }
    }
}
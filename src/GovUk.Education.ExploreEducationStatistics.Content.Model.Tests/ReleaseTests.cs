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

    }
}
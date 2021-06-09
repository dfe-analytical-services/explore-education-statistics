using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class MethodologyTests
    {
        [Fact]
        public void PubliclyVisible_ApprovedAndPublishedImmediately()
        {
            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = Immediately,
            };
            
            Assert.True(methodology.PubliclyAccessible);
        }
        
        [Fact]
        public void PubliclyVisible_ApprovedAndScheduledWithLiveRelease()
        {
            var liveRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow
            };
            
            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = liveRelease,
                ScheduledWithReleaseId = liveRelease.Id
            };
            
            Assert.True(methodology.PubliclyAccessible);
        }
        
        [Fact]
        public void PubliclyVisible_NotApprovedSoNotVisible()
        {
            var methodology = new Methodology
            {
                Status = Draft,
                PublishingStrategy = Immediately,
            };
            
            Assert.False(methodology.PubliclyAccessible);
        }
        
        [Fact]
        public void PubliclyVisible_ApprovedButScheduledWithNonLiveReleaseSoNotVisible()
        {
            var nonLiveRelease = new Release
            {
                Id = Guid.NewGuid()
            };
            
            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = nonLiveRelease,
                ScheduledWithReleaseId = nonLiveRelease.Id
            };
            
            Assert.False(methodology.PubliclyAccessible);
        }

        [Fact] 
        public void PubliclyVisible_ScheduledWithReleaseNotIncluded()
        {
            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid()
            };
            
            Assert.Throws<InvalidOperationException>(() => methodology.PubliclyAccessible);
        }
    }
}
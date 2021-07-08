using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class MethodologyTests
    {
        [Fact]
        public void ScheduledForPublishingImmediately_TrueWhenPublishingStrategyIsImmediately()
        {
            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = Immediately
            };

            Assert.True(methodology.ScheduledForPublishingImmediately);
        }

        [Fact]
        public void ScheduledForPublishingImmediately_FalseWhenPublishingStrategyIsWithRelease()
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
                ScheduledWithReleaseId = liveRelease.Id,
                ScheduledWithRelease = liveRelease
            };

            Assert.False(methodology.ScheduledForPublishingImmediately);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_TrueWhenScheduledWithLiveRelease()
        {
            var liveRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow
            };

            var methodology = new Methodology
            {
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = liveRelease,
                ScheduledWithReleaseId = liveRelease.Id
            };

            Assert.True(methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_FalseWhenScheduledWithNonLiveRelease()
        {
            var nonLiveRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodology = new Methodology
            {
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = nonLiveRelease,
                ScheduledWithReleaseId = nonLiveRelease.Id
            };

            Assert.False(methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_FalseWhenPublishingStrategyIsImmediately()
        {
            var methodology = new Methodology
            {
                PublishingStrategy = Immediately
            };

            Assert.False(methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_ScheduledWithReleaseNotIncluded()
        {
            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid()
            };

            Assert.Throws<InvalidOperationException>(() => methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void GetTitle_NoAlternativeTitleSet()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    OwningPublicationTitle = "Owning Publication Title"
                }
            };
            
            Assert.Equal(methodology.MethodologyParent.OwningPublicationTitle, methodology.Title);
        }

        [Fact]
        public void GetTitle_AlternativeTitleSet()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    OwningPublicationTitle = "Owning Publication Title"
                },
                AlternativeTitle = "Alternative Title"
            };
            
            Assert.Equal(methodology.AlternativeTitle, methodology.Title);
        }

        [Fact]
        public void GetSlug()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    Slug = "owning-publication-slug"
                }
            };
            
            Assert.Equal(methodology.MethodologyParent.Slug, methodology.Slug);
        }
    }
}

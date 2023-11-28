#nullable enable
using System;
using System.Collections.Generic;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class MethodologyVersionTests
    {
        [Fact]
        public void ScheduledForPublishingImmediately_TrueWhenPublishingStrategyIsImmediately()
        {
            var methodology = new MethodologyVersion
            {
                PublishingStrategy = Immediately
            };

            Assert.True(methodology.ScheduledForPublishingImmediately);
        }

        [Fact]
        public void ScheduledForPublishingImmediately_FalseWhenPublishingStrategyIsWithRelease()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodology = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = release.Id,
                ScheduledWithRelease = release
            };

            Assert.False(methodology.ScheduledForPublishingImmediately);
        }

        [Fact]
        public void ScheduledForPublishingWithRelease_FalseWhenPublishingStrategyIsImmediately()
        {
            var methodology = new MethodologyVersion
            {
                PublishingStrategy = Immediately
            };

            Assert.False(methodology.ScheduledForPublishingWithRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithRelease_TrueWhenPublishingStrategyIsWithRelease()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodology = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = release.Id,
                ScheduledWithRelease = release
            };

            Assert.True(methodology.ScheduledForPublishingWithRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_TrueWhenScheduledWithLiveRelease()
        {
            var liveRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow
            };

            var methodology = new MethodologyVersion
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

            var methodology = new MethodologyVersion
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
            var methodology = new MethodologyVersion
            {
                PublishingStrategy = Immediately
            };

            Assert.False(methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void ScheduledForPublishingWithPublishedRelease_ScheduledWithReleaseNotIncluded()
        {
            var methodology = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid()
            };

            Assert.Throws<InvalidOperationException>(() => methodology.ScheduledForPublishingWithPublishedRelease);
        }

        [Fact]
        public void GetTitle_NoAlternativeTitleSet()
        {
            var methodology = new MethodologyVersion
            {
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Owning Publication Title"
                }
            };

            Assert.Equal(methodology.Methodology.OwningPublicationTitle, methodology.Title);
        }

        [Fact]
        public void GetTitle_AlternativeTitleSet()
        {
            var methodology = new MethodologyVersion
            {
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Owning Publication Title"
                },
                AlternativeTitle = "Alternative Title"
            };

            Assert.Equal(methodology.AlternativeTitle, methodology.Title);
        }

        [Fact]
        public void GetSlug_OwningPublicationSlug()
        {
            var methodologyVersion = new MethodologyVersion
            {
                AlternativeSlug = null,
                Methodology = new Methodology
                {
                    OwningPublicationSlug = "owning-publication-slug",
                },
            };

            Assert.Equal(methodologyVersion.Methodology.OwningPublicationSlug, methodologyVersion.Slug);
        }

        [Fact]
        public void GetSlug_AlternativeSlug()
        {
            var methodologyVersion = new MethodologyVersion
            {
                AlternativeSlug = "alternativeSlug",
                Methodology = new Methodology
                {
                    OwningPublicationSlug = "owning-publication-slug",
                },
            };

            Assert.Equal(methodologyVersion.AlternativeSlug, methodologyVersion.Slug);
        }

        [Fact]
        public void AmendmentFlag_NotAmendment_FirstUnpublishedVersion()
        {
            var methodology = new MethodologyVersion
            {
                PreviousVersionId = null,
                Published = null
            };

            Assert.False(methodology.Amendment);
        }

        [Fact]
        public void AmendmentFlag_NotAmendment_FirstPublishedVersion()
        {
            var methodology = new MethodologyVersion
            {
                PreviousVersionId = null,
                Published = DateTime.Now
            };

            Assert.False(methodology.Amendment);
        }

        [Fact]
        public void AmendmentFlag_Amendment_UnpublishedNewVersion()
        {
            var methodology = new MethodologyVersion
            {
                PreviousVersionId = Guid.NewGuid(),
                Published = null
            };

            Assert.True(methodology.Amendment);
        }

        [Fact]
        public void AmendmentFlag_NotAmendment_PublishedNewVersion()
        {
            var methodology = new MethodologyVersion
            {
                PreviousVersionId = Guid.NewGuid(),
                Published = DateTime.Now
            };

            Assert.False(methodology.Amendment);
        }
    }
}

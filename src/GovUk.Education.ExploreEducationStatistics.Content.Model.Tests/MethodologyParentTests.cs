using System;
using System.Collections.Generic;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class MethodologyParentTests
    {
        [Fact]
        public void IsLatestVersionOfMethodology_ArgumentExceptionWithNullVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = null
            };

            Assert.Throws<ArgumentException>(() => methodology.IsLatestVersionOfMethodology(Guid.NewGuid()));
        }

        [Fact]
        public void IsLatestVersionOfMethodology_FalseWithEmptyVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = new List<Methodology>()
            };

            Assert.False(methodology.IsLatestVersionOfMethodology(Guid.NewGuid()));
        }

        [Fact]
        public void IsLatestVersionOfMethodology_TrueForSingleVersion()
        {
            var version = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 0
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(version)
            };

            Assert.True(methodology.IsLatestVersionOfMethodology(version.Id));
        }

        [Fact]
        public void IsLatestVersionOfMethodology_FalseForPreviousVersion()
        {
            var previousVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 1
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(previousVersion, latestVersion)
            };

            Assert.False(methodology.IsLatestVersionOfMethodology(previousVersion.Id));
        }

        [Fact]
        public void IsLatestVersionOfMethodology_TrueForLatestVersion()
        {
            var previousVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 1
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(previousVersion, latestVersion)
            };

            Assert.True(methodology.IsLatestVersionOfMethodology(latestVersion.Id));
        }

        [Fact]
        public void IsLatestPublishedVersionOfMethodology_ArgumentExceptionWithNullVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = null
            };

            Assert.Throws<ArgumentException>(() => methodology.IsLatestPublishedVersionOfMethodology(Guid.NewGuid()));
        }

        [Fact]
        public void IsLatestPublishedVersionOfMethodology_FalseWithEmptyVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = new List<Methodology>()
            };

            Assert.False(methodology.IsLatestPublishedVersionOfMethodology(Guid.NewGuid()));
        }

        [Fact]
        public void IsLatestPublishedVersionOfMethodology_FalseForSingleUnpublishedVersion()
        {
            var version = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 0
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(version)
            };

            Assert.False(methodology.IsLatestPublishedVersionOfMethodology(version.Id));
        }

        [Fact]
        public void IsLatestPublishedVersionOfMethodology_FalseForPreviousVersion()
        {
            var previousVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 1
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(previousVersion, latestPublishedVersion)
            };

            Assert.False(methodology.IsLatestPublishedVersionOfMethodology(previousVersion.Id));
        }

        [Fact]
        public void IsLatestPublishedVersionOfMethodology_TrueForLatestPublishedVersion()
        {
            var previousVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 1
            };

            var latestDraftVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = latestPublishedVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 2
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(previousVersion, latestPublishedVersion, latestDraftVersion)
            };

            Assert.True(methodology.IsLatestPublishedVersionOfMethodology(latestPublishedVersion.Id));
            Assert.False(methodology.IsLatestPublishedVersionOfMethodology(latestDraftVersion.Id));
        }

        [Fact]
        public void LatestVersion_ArgumentExceptionWithNullVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = null
            };

            Assert.Throws<ArgumentException>(() => methodology.LatestVersion());
        }

        [Fact]
        public void LatestVersion_NullWithEmptyVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = new List<Methodology>()
            };

            Assert.Null(methodology.LatestVersion());
        }

        [Fact]
        public void LatestVersion_SingleVersionIsLatest()
        {
            var version = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 0
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(version)
            };

            Assert.Equal(version, methodology.LatestVersion());
        }

        [Fact]
        public void LatestVersion_CorrectVersionIsLatest()
        {
            var previousVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 1
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(previousVersion, latestVersion)
            };

            Assert.Equal(latestVersion, methodology.LatestVersion());
        }

        [Fact]
        public void LatestPublishedVersion_ArgumentExceptionWithNullVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = null
            };

            Assert.Throws<ArgumentException>(() => methodology.LatestPublishedVersion());
        }

        [Fact]
        public void LatestPublishedVersion_NullWithEmptyVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = new List<Methodology>()
            };

            Assert.Null(methodology.LatestPublishedVersion());
        }

        [Fact]
        public void LatestPublishedVersion_NullWithSingleUnpublishedVersion()
        {
            var version = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 0
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(version)
            };

            Assert.Null(methodology.LatestPublishedVersion());
        }

        [Fact]
        public void LatestPublishedVersion_CorrectVersionIsLatest()
        {
            var previousVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 1
            };

            var latestDraftVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = latestPublishedVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 2
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(previousVersion, latestPublishedVersion, latestDraftVersion)
            };

            Assert.Equal(latestPublishedVersion, methodology.LatestPublishedVersion());
        }
    }
}

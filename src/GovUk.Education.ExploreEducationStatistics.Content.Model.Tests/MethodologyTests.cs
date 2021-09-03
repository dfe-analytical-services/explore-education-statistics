#nullable enable
using System;
using System.Collections.Generic;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class MethodologyTests
    {
        [Fact]
        public void IsLatestVersionOfMethodology_FalseWithEmptyVersions()
        {
            var methodology = new Methodology
            {
                Versions = new List<MethodologyVersion>()
            };

            Assert.False(methodology.IsLatestVersionOfMethodology(Guid.NewGuid()));
        }

        [Fact]
        public void IsLatestVersionOfMethodology_TrueForSingleVersion()
        {
            var version = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 0
            };

            var methodology = new Methodology
            {
                Versions = AsList(version)
            };

            Assert.True(methodology.IsLatestVersionOfMethodology(version.Id));
        }

        [Fact]
        public void IsLatestVersionOfMethodology_FalseForPreviousVersion()
        {
            var previousVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 1
            };

            var methodology = new Methodology
            {
                Versions = AsList(previousVersion, latestVersion)
            };

            Assert.False(methodology.IsLatestVersionOfMethodology(previousVersion.Id));
        }

        [Fact]
        public void IsLatestVersionOfMethodology_TrueForLatestVersion()
        {
            var previousVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 1
            };

            var methodology = new Methodology
            {
                Versions = AsList(previousVersion, latestVersion)
            };

            Assert.True(methodology.IsLatestVersionOfMethodology(latestVersion.Id));
        }

        [Fact]
        public void LatestVersion_ArgumentExceptionWithEmptyVersions()
        {
            var methodology = new Methodology
            {
                Versions = new List<MethodologyVersion>()
            };

            Assert.Throws<ArgumentException>(() => methodology.LatestVersion());
        }

        [Fact]
        public void LatestVersion_SingleVersionIsLatest()
        {
            var version = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 0
            };

            var methodology = new Methodology
            {
                Versions = AsList(version)
            };

            Assert.Equal(version, methodology.LatestVersion());
        }

        [Fact]
        public void LatestVersion_CorrectVersionIsLatest()
        {
            var previousVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 1
            };

            var methodology = new Methodology
            {
                Versions = AsList(previousVersion, latestVersion)
            };

            Assert.Equal(latestVersion, methodology.LatestVersion());
        }
    }
}

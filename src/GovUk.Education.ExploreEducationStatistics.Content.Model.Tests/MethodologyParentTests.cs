using System;
using System.Collections.Generic;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class MethodologyParentTests
    {
        [Fact]
        public void IsLatestVersionOfMethodology_FalseWithNullVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = null
            };

            Assert.False(methodology.IsLatestVersionOfMethodology(Guid.NewGuid()));
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
                Version = 0
            };

            var latestVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
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
                Version = 0
            };

            var latestVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                Version = 1
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(previousVersion, latestVersion)
            };

            Assert.True(methodology.IsLatestVersionOfMethodology(latestVersion.Id));
        }

        [Fact]
        public void LatestVersion_NullWithNullVersions()
        {
            var methodology = new MethodologyParent
            {
                Versions = null
            };

            Assert.Null(methodology.LatestVersion());
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
                Version = 0
            };

            var latestVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                Version = 1
            };

            var methodology = new MethodologyParent
            {
                Versions = AsList(previousVersion, latestVersion)
            };

            Assert.Equal(latestVersion, methodology.LatestVersion());
        }
    }
}

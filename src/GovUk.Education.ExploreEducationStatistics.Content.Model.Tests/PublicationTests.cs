#nullable enable
using System;
using System.Collections.Generic;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class PublicationTests
    {
        [Fact]
        public void LatestRelease()
        {
            var release2000Id = Guid.NewGuid();
            var release2000AmendmentId = Guid.NewGuid();
            var release2001Id = Guid.NewGuid();
            var release2001AmendmentId = Guid.NewGuid();
            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = release2000Id,
                        Version = 0,
                        ReleaseName = "2000",
                        TimePeriodCoverage = AcademicYear,
                        Published = DateTime.UtcNow,
                        ApprovalStatus = Approved,
                    },
                    new()
                    {
                        Id = release2000AmendmentId,
                        Version = 1,
                        PreviousVersionId = release2000Id,
                        ReleaseName = "2000",
                        TimePeriodCoverage = AcademicYear,
                        Published = DateTime.UtcNow,
                        ApprovalStatus = Approved,
                    },
                    new()
                    {
                        Id = release2001Id,
                        Version = 0,
                        ReleaseName = "2001",
                        Published = DateTime.UtcNow,
                        TimePeriodCoverage = AcademicYear,
                        ApprovalStatus = Approved,
                    },
                    new()
                    {
                        Id = release2001AmendmentId,
                        Version = 1,
                        PreviousVersionId = release2001Id,
                        ReleaseName = "2001",
                        Published = null,
                        TimePeriodCoverage = AcademicYear,
                        ApprovalStatus = Draft,
                    },
                },
            };

            var result = publication.LatestRelease();
            Assert.NotNull(result);
            Assert.Equal(release2001AmendmentId, result!.Id);
        }

        [Fact]
        public void LatestRelease_NoRelease()
        {
            var publication = new Publication
            {
                Releases = new List<Release>()
            };

            var result = publication.LatestRelease();
            Assert.Null(result);
        }

        [Fact]
        public void IsLatestVersionOfRelease_SingleRelease()
        {
            var publicationId = Guid.NewGuid();
            var release2000OriginalId = Guid.NewGuid();
            var release2000Original = new Release
            {
                Id = release2000OriginalId,
                PublicationId = publicationId,
                ReleaseName = "2000",
                PreviousVersionId = release2000OriginalId,
            };
            var publication = new Publication
            {
                Id = publicationId,
                Releases = new List<Release>
                {
                    release2000Original,
                },
            };

            Assert.True(publication.IsLatestVersionOfRelease(release2000OriginalId));
        }

        [Fact]
        public void IsLatestVersionOfRelease_Amendments()
        {
            var publicationId = Guid.NewGuid();

            var release2000OriginalId = Guid.NewGuid();
            var release2000Original = new Release
            {
                Id = release2000OriginalId,
                PublicationId = publicationId,
                ReleaseName = "2000",
                PreviousVersionId = release2000OriginalId,
            };
            var release2000Latest = new Release
            {
                PublicationId = publicationId,
                ReleaseName = "2000",
                PreviousVersionId = release2000OriginalId,
            };

            var release2001OriginalId = Guid.NewGuid();
            var release2001Original = new Release
            {
                Id = release2001OriginalId,
                PublicationId = publicationId,
                ReleaseName = "2001",
                PreviousVersionId = release2000OriginalId,
            };
            var release2001Amendment = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publicationId,
                ReleaseName = "2001",
                PreviousVersionId = release2001OriginalId,
            };
            var release2001Latest = new Release
            {
                PublicationId = publicationId,
                ReleaseName = "2001",
                PreviousVersionId = release2001Amendment.Id,
            };
            var release2002LatestId = Guid.NewGuid();
            var release2002Latest = new Release
            {
                Id = release2002LatestId,
                PublicationId = publicationId,
                ReleaseName = "2002",
                PreviousVersionId = release2002LatestId,
            };
            var publication = new Publication
            {
                Id = publicationId,
                Releases =
                {
                    release2000Original,
                    release2000Latest,
                    release2001Original,
                    release2001Amendment,
                    release2001Latest,
                    release2002Latest,
                }
            };

            Assert.False(publication.IsLatestVersionOfRelease(release2000Original.Id));
            Assert.True(publication.IsLatestVersionOfRelease(release2000Latest.Id));
            Assert.False(publication.IsLatestVersionOfRelease(release2001Original.Id));
            Assert.False(publication.IsLatestVersionOfRelease(release2001Amendment.Id));
            Assert.True(publication.IsLatestVersionOfRelease(release2001Latest.Id));
            Assert.True(publication.IsLatestVersionOfRelease(release2002Latest.Id));
        }

        [Fact]
        public void IsLatestPublishedVersionOfRelease_PublicationHasSingleUnpublishedRelease()
        {
            var publication = new Publication();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null,
                PreviousVersionId = null
            };

            publication.Releases = new List<Release>
            {
                release
            };

            Assert.False(publication.IsLatestPublishedVersionOfRelease(release));
        }

        [Fact]
        public void IsLatestPublishedVersionOfRelease_PublicationHasSinglePublishedRelease()
        {
            var publication = new Publication();

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = null
            };

            publication.Releases = new List<Release>
            {
                release
            };

            Assert.True(publication.IsLatestPublishedVersionOfRelease(release));
        }

        [Fact]
        public void IsLatestPublishedVersionOfRelease_PublicationHasMultipleReleases()
        {
            var publication = new Publication();

            // (Release 1) Published but superseded by another published Release
            var release1Version1 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = null
            };

            // (Release 1)  Latest published version (superseded by an unpublished Release)
            var release1Version2 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = release1Version1.Id
            };

            // (Release 1) Unpublished
            var release1Version3 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null,
                PreviousVersionId = release1Version2.Id
            };

            // (Release 2) Latest published version
            var release2 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddSeconds(-1),
                PreviousVersionId = null
            };

            // (Release 3) Unpublished
            var release3 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = null,
                PreviousVersionId = null
            };

            publication.Releases = new List<Release>
            {
                release1Version1,
                release1Version2,
                release1Version3,
                release2,
                release3
            };

            Assert.False(publication.IsLatestPublishedVersionOfRelease(release1Version1));
            Assert.True(publication.IsLatestPublishedVersionOfRelease(release1Version2));
            Assert.False(publication.IsLatestPublishedVersionOfRelease(release1Version3));
            Assert.True(publication.IsLatestPublishedVersionOfRelease(release2));
            Assert.False(publication.IsLatestPublishedVersionOfRelease(release3));
        }
    }
}

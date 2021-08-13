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
        public void LatestPublishedRelease_NewDraftAmendment()
        {
            var originalReleaseId = Guid.NewGuid();
            var amendedRelease1Id = Guid.NewGuid();
            var amendedRelease2Id = Guid.NewGuid();
            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = originalReleaseId,
                        Version = 0,
                        ReleaseName = "2000",
                        TimePeriodCoverage = AcademicYear,
                        Published = DateTime.UtcNow.AddDays(-2),
                        PreviousVersion = null,
                        ApprovalStatus = Approved,

                    },
                    new()
                    {
                        Id = amendedRelease1Id,
                        Version = 1,
                        ReleaseName = "2000",
                        Published = DateTime.UtcNow.AddDays(-1),
                        TimePeriodCoverage = AcademicYear,
                        PreviousVersionId = originalReleaseId,
                        ApprovalStatus = Approved,
                    },
                    new()
                    {
                        Id = amendedRelease2Id,
                        Version = 2,
                        ReleaseName = "2000",
                        Published = null,
                        TimePeriodCoverage = AcademicYear,
                        PreviousVersionId = amendedRelease1Id,
                        ApprovalStatus = Draft,
                    },
                },
            };

            var result = publication.LatestPublishedRelease();
            Assert.Equal(amendedRelease1Id, result.Id);
        }

        [Fact]
        public void LatestPublishedRelease_NewDraft()
        {
            var release2000Id = Guid.NewGuid();
            var release2001Id = Guid.NewGuid();
            var release2002Id = Guid.NewGuid();
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
                        Id = release2001Id,
                        Version = 0,
                        ReleaseName = "2001",
                        Published = DateTime.UtcNow,
                        TimePeriodCoverage = AcademicYear,
                        ApprovalStatus = Approved,
                    },
                    new()
                    {
                        Id = release2002Id,
                        Version = 0,
                        ReleaseName = "2002",
                        Published = null,
                        TimePeriodCoverage = AcademicYear,
                        ApprovalStatus = Draft,
                    },
                },
            };

            var result = publication.LatestPublishedRelease();
            Assert.Equal(release2001Id, result.Id);
        }

        [Fact]
        public void LatestPublishedRelease_NoPublishedRelease()
        {
            var originalReleaseId = Guid.NewGuid();
            var publication = new Publication
            {
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = originalReleaseId,
                        Version = 0,
                        ReleaseName = "2000",
                        TimePeriodCoverage = AcademicYear,
                        Published = null,
                        PreviousVersion = null,
                        ApprovalStatus = Draft,

                    },
                },
            };

            var result = publication.LatestPublishedRelease();
            Assert.Null(result);

        }

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
            Assert.Equal(release2001AmendmentId, result.Id);
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
    }
}

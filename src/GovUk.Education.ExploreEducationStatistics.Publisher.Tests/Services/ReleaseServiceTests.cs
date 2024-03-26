#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class ReleaseServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task GetFiles()
    {
        var releaseVersion = new ReleaseVersion();
        var releaseFiles = new List<ReleaseFile>
        {
            new()
            {
                ReleaseVersion = releaseVersion,
                Name = "Ancillary Test File",
                File = new File
                {
                    Filename = "ancillary.pdf",
                    ContentLength = 10240,
                    Type = Ancillary
                }
            },
            new()
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Filename = "chart.png",
                    Type = Chart
                }
            },
            new()
            {
                ReleaseVersion = releaseVersion,
                Name = "Data Test File",
                File = new File
                {
                    Filename = "data.csv",
                    ContentLength = 20480,
                    Type = FileType.Data
                }
            },
            new()
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Filename = "data.meta.csv",
                    Type = Metadata
                }
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext);

            var result = await service.GetFiles(releaseVersion.Id,
                Ancillary,
                Chart);

            Assert.Equal(2, result.Count);
            Assert.Equal(releaseFiles[0].File.Id, result[0].Id);
            Assert.Equal(releaseFiles[1].File.Id, result[1].Id);
        }
    }

    [Fact]
    public async Task GetLatestRelease()
    {
        var publication = new Publication();

        var release1V0 = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            ReleaseName = "2017",
            TimePeriodCoverage = AcademicYearQ4,
            Slug = "2017-18-q4",
            Published = new DateTime(2019, 4, 1),
            ApprovalStatus = Approved
        };

        var release2V0 = new ReleaseVersion
        {
            Id = new Guid("e7e1aae3-a0a1-44b7-bdf3-3df4a363ce20"),
            Publication = publication,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            Slug = "2018-19-q1",
            Published = new DateTime(2019, 3, 1),
            ApprovalStatus = Approved,
        };

        var release3V0 = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ2,
            Slug = "2018-19-q2",
            Published = new DateTime(2019, 1, 1),
            ApprovalStatus = Approved,
            Version = 0,
            PreviousVersionId = null
        };

        var release3V1 = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ2,
            Slug = "2018-19-q2",
            Published = new DateTime(2019, 2, 1),
            ApprovalStatus = Approved,
            Version = 1,
            PreviousVersionId = release3V0.Id
        };

        var release3V2Deleted = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ2,
            Slug = "2018-19-q2",
            Published = null,
            ApprovalStatus = Approved,
            Version = 2,
            PreviousVersionId = release3V1.Id,
            SoftDeleted = true
        };

        var release3V3NotPublished = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ2,
            Slug = "2018-19-q2",
            Published = null,
            ApprovalStatus = Approved,
            Version = 3,
            PreviousVersionId = release3V1.Id
        };

        var release4V0 = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ3,
            Slug = "2018-19-q3",
            Published = null,
            ApprovalStatus = Approved
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(publication);
            await contentDbContext.AddRangeAsync(release1V0,
                release2V0,
                release3V0,
                release3V1,
                release3V2Deleted,
                release3V3NotPublished,
                release4V0);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext);

            var result = await service.GetLatestReleaseVersion(publication.Id, Enumerable.Empty<Guid>());

            Assert.Equal(release3V1.Id, result.Id);
            Assert.Equal("Academic year Q2 2018/19", result.Title);
        }
    }

    [Fact]
    public async Task CompletePublishing_FirstVersion()
    {
        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .Generate();

        var originalDataBlockParents = _fixture
            .DefaultDataBlockParent()
            .WithLatestDraftVersion(() => _fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(releaseVersion)
                .Generate())
            .GenerateList(2);

        var actualPublishedDate = DateTime.UtcNow;

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext: contentDbContext);
            await service.CompletePublishing(releaseVersion.Id, actualPublishedDate);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actualRelease = await contentDbContext
                .ReleaseVersions
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Expect the published date to have been updated with the actual published date
            Assert.Equal(actualPublishedDate, actualRelease.Published);

            var actualDataBlockParents = await contentDbContext
                .DataBlockParents
                .Include(dataBlockParent => dataBlockParent.LatestDraftVersion)
                .Include(dataBlockParent => dataBlockParent.LatestPublishedVersion)
                .ToListAsync();

            Assert.Equal(2, actualDataBlockParents.Count);

            // Assert that the original DataBlockParents did not point to a LatestPublishedVersion.
            originalDataBlockParents.ForEach(parent => Assert.Null(parent.LatestPublishedVersionId));

            // Assert that all DataBlockParents have had their LatestPublishedVersion pointers updated to
            // reference the newly published DataBlockVersion.
            actualDataBlockParents.ForEach(parent =>
            {
                var originalParent = originalDataBlockParents.Single(p => p.Id == parent.Id);

                // The LatestPublishedVersion version is now the one that was previously the LatestDraftVersion.
                // Its "Published" date should have just been set as well.
                Assert.Equal(originalParent.LatestDraftVersionId, parent.LatestPublishedVersionId);
                Assert.Null(originalParent.LatestDraftVersion!.Published);
                parent.LatestPublishedVersion!.Published.AssertUtcNow();

                // The LatestDraftVersion is now set to null, until a Release amendment is created in the future.
                Assert.Null(parent.LatestDraftVersionId);
            });
        }
    }

    [Fact]
    public async Task CompletePublishing_AmendedRelease()
    {
        var previousReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-1),
            PreviousVersionId = null,
            Version = 0
        };

        var releaseVersion = new ReleaseVersion
        {
            PreviousVersionId = previousReleaseVersion.Id,
            Version = 1
        };

        // Generate Data Blocks for both the previous Release version and for the new Amendment.
        var originalDataBlockParents = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(() => _fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(previousReleaseVersion)
                .Generate())
            .WithLatestDraftVersion(() => _fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(releaseVersion)
                .Generate())
            .GenerateList(2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(previousReleaseVersion, releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext: contentDbContext);
            await service.CompletePublishing(releaseVersion.Id, DateTime.UtcNow);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actual = await contentDbContext
                .ReleaseVersions
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Expect the published date to have been copied from the previous version
            Assert.Equal(previousReleaseVersion.Published, actual.Published);

            var actualDataBlockParents = await contentDbContext
                .DataBlockParents
                .Include(dataBlockParent => dataBlockParent.LatestDraftVersion)
                .Include(dataBlockParent => dataBlockParent.LatestPublishedVersion)
                .ToListAsync();

            Assert.Equal(2, actualDataBlockParents.Count);

            // Assert that the original DataBlockParents pointed to a LatestPublishedVersion and LatestDraftVersion.
            originalDataBlockParents.ForEach(parent => Assert.NotNull(parent.LatestDraftVersionId));
            originalDataBlockParents.ForEach(parent => Assert.NotNull(parent.LatestPublishedVersionId));

            // Assert that all DataBlockParents have had their LatestPublishedVersion pointers updated to
            // reference the newly published DataBlockVersion.
            actualDataBlockParents.ForEach(parent =>
            {
                var originalParent = originalDataBlockParents.Single(p => p.Id == parent.Id);

                // The LatestPublishedVersion is now the one that was previously the LatestDraftVersion.
                // Its "Published" date should have just been set as well, and we'll double-check that it didn't
                // just inherit that date from the original LatestDraftVersion DataBlockVersion, as it should not
                // have been set until it was published.
                Assert.Equal(originalParent.LatestDraftVersionId, parent.LatestPublishedVersionId);
                Assert.Null(originalParent.LatestDraftVersion!.Published);
                parent.LatestPublishedVersion!.Published.AssertUtcNow();

                // The LatestDraftVersion is now set to null, until a Release amendment is created in the future.
                Assert.Null(parent.LatestDraftVersionId);
            });
        }
    }

    [Fact]
    public async Task CompletePublishing_AmendedRelease_DataBlockRemoved()
    {
        var previousReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-1),
            PreviousVersionId = null,
            Version = 0
        };

        var releaseVersion = new ReleaseVersion
        {
            PreviousVersionId = previousReleaseVersion.Id,
            Version = 1
        };

        // Generate Data Blocks for both the previous Release version and for the new Amendment.
        _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(() => _fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(previousReleaseVersion)
                .Generate())
            // This time Data Blocks have been removed from the latest Release amendment, and so they now have no
            // "latest" version.
            .WithLatestDraftVersion((DataBlockVersion) null!)
            .GenerateList(2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(previousReleaseVersion, releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext: contentDbContext);
            await service.CompletePublishing(releaseVersion.Id, DateTime.UtcNow);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actual = await contentDbContext
                .ReleaseVersions
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Expect the published date to have been copied from the previous version
            Assert.Equal(previousReleaseVersion.Published, actual.Published);

            var actualDataBlockParents = await contentDbContext
                .DataBlockParents
                .Include(dataBlockParent => dataBlockParent.LatestDraftVersion)
                .Include(dataBlockParent => dataBlockParent.LatestPublishedVersion)
                .ToListAsync();

            // Assert that all DataBlockParents have had their LatestPublishedVersion pointers updated to
            // be null so that this Data Block is effectively no longer publicly visible. Their LatestDraftVersions
            // are also null for this DataBlockParent as this Data Block no longer has a Draft version being worked
            // on as a part of this Release / amendment.
            actualDataBlockParents.ForEach(parent =>
            {
                Assert.Null(parent.LatestPublishedVersionId);
                Assert.Null(parent.LatestDraftVersionId);
            });
        }
    }

    [Fact]
    public async Task CompletePublishing_AmendedReleaseAndUpdatePublishedDateIsTrue()
    {
        var previousReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow.AddDays(-1),
            PreviousVersionId = null,
            Version = 0
        };

        var releaseVersion = new ReleaseVersion
        {
            Published = null,
            PreviousVersionId = previousReleaseVersion.Id,
            Version = 1,
            UpdatePublishedDate = true
        };

        var actualPublishedDate = DateTime.UtcNow;

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(previousReleaseVersion, releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext: contentDbContext);
            await service.CompletePublishing(releaseVersion.Id, actualPublishedDate);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actual = await contentDbContext
                .ReleaseVersions
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Expect the published date to have been updated with the actual published date
            Assert.Equal(actualPublishedDate, actual.Published);
        }
    }

    private static ReleaseService BuildReleaseService(
        ContentDbContext? contentDbContext = null)
    {
        contentDbContext ??= InMemoryContentDbContext();

        return new(
            contentDbContext,
            releaseVersionRepository: new ReleaseVersionRepository(contentDbContext)
        );
    }
}

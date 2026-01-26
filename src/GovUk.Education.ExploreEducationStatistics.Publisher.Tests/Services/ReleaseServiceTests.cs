using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

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
                    Type = Ancillary,
                },
            },
            new()
            {
                ReleaseVersion = releaseVersion,
                File = new File { Filename = "chart.png", Type = Chart },
            },
            new()
            {
                ReleaseVersion = releaseVersion,
                Name = "Data Test File",
                File = new File
                {
                    Filename = "data.csv",
                    ContentLength = 20480,
                    Type = FileType.Data,
                },
            },
            new()
            {
                ReleaseVersion = releaseVersion,
                File = new File { Filename = "data.meta.csv", Type = Metadata },
            },
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

            var result = await service.GetFiles(releaseVersion.Id, Ancillary, Chart);

            Assert.Equal(2, result.Count);
            Assert.Equal(releaseFiles[0].File.Id, result[0].Id);
            Assert.Equal(releaseFiles[1].File.Id, result[1].Id);
        }
    }

    [Fact]
    public async Task GetLatestPublishedReleaseVersion_Success()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases(_ =>
                [
                    _fixture.DefaultRelease(publishedVersions: 1, year: 2020),
                    _fixture.DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2021),
                    _fixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2022),
                ]
            );

        var release2021 = publication.Releases.Single(r => r.Year == 2021);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext);

            var result = await service.GetLatestPublishedReleaseVersion(
                publication.Id,
                includeUnpublishedVersionIds: []
            );

            Assert.Equal(release2021.Versions[1].Id, result.Id);
        }
    }

    [Fact]
    public async Task GetLatestPublishedReleaseVersion_NonDefaultReleaseOrder()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases(_ =>
                [
                    _fixture.DefaultRelease(publishedVersions: 1, year: 2020),
                    _fixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021),
                    _fixture.DefaultRelease(publishedVersions: 1, year: 2022),
                ]
            )
            .FinishWith(p =>
            {
                // Adjust the generated LatestPublishedReleaseVersion to make 2020 the latest published release
                var release2020Version0 = p.Releases.Single(r => r.Year == 2020).Versions[0];
                p.LatestPublishedReleaseVersion = release2020Version0;
                p.LatestPublishedReleaseVersionId = release2020Version0.Id;

                // Apply a different release series order rather than using the default
                p.ReleaseSeries = [.. GenerateReleaseSeries(p.Releases, 2021, 2020, 2022)];
            });

        var release2020 = publication.Releases.Single(r => r.Year == 2020);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext);

            var result = await service.GetLatestPublishedReleaseVersion(
                publication.Id,
                includeUnpublishedVersionIds: []
            );

            // Check the 2020 release version is considered to be the latest published release version,
            // since 2020 is the first release in the release series with a published version
            Assert.Equal(release2020.Versions[0].Id, result.Id);
        }
    }

    [Fact]
    public async Task GetLatestPublishedReleaseVersion_IncludeUnpublishedReleaseVersion()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases(_ =>
                [
                    _fixture.DefaultRelease(publishedVersions: 1, year: 2020),
                    _fixture.DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2021),
                    _fixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2022),
                ]
            );

        var release2021 = publication.Releases.Single(r => r.Year == 2021);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext);

            // Include the unpublished 2021 release version id in the call
            // to test the scenario where this version is about to be published
            var result = await service.GetLatestPublishedReleaseVersion(
                publication.Id,
                includeUnpublishedVersionIds: [release2021.Versions.Single(rv => rv.Published == null).Id]
            );

            // Check the unpublished 2021 release version is considered to be the latest published release version,
            // despite the fact that it is not published yet
            Assert.Equal(release2021.Versions.Single(rv => rv.Published == null).Id, result.Id);
        }
    }

    [Fact]
    public async Task GetLatestPublishedReleaseVersion_IncludeUnpublishedReleaseVersions()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases(_ =>
                [
                    _fixture.DefaultRelease(publishedVersions: 1, year: 2020),
                    _fixture.DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2021),
                    _fixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2022),
                ]
            );

        var release2021 = publication.Releases.Single(r => r.Year == 2021);
        var release2022 = publication.Releases.Single(r => r.Year == 2022);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext);

            // Include the unpublished 2021 and 2022 release version id's in the call
            // to test the scenario where both versions are about to be published together
            var result = await service.GetLatestPublishedReleaseVersion(
                publication.Id,
                includeUnpublishedVersionIds:
                [
                    release2021.Versions.Single(rv => rv.Published == null).Id,
                    release2022.Versions.Single(rv => rv.Published == null).Id,
                ]
            );

            // Check the unpublished 2022 release version is considered to be the latest published release version,
            // despite the fact that it is not published yet
            Assert.Equal(release2022.Versions.Single(rv => rv.Published == null).Id, result.Id);
        }
    }

    [Fact]
    public async Task GetLatestPublishedReleaseVersion_IgnoresIncludedUnpublishedReleaseVersions()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases(_ =>
                [
                    _fixture.DefaultRelease(publishedVersions: 1, year: 2020),
                    _fixture.DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2021),
                    _fixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2022),
                ]
            )
            .FinishWith(p =>
            {
                // Adjust the generated LatestPublishedReleaseVersion to make 2020 the latest published release
                var release2020Version0 = p.Releases.Single(r => r.Year == 2020).Versions[0];
                p.LatestPublishedReleaseVersion = release2020Version0;
                p.LatestPublishedReleaseVersionId = release2020Version0.Id;

                // Apply a different release series order rather than using the default
                p.ReleaseSeries = [.. GenerateReleaseSeries(p.Releases, 2020, 2021, 2022)];
            });

        var release2020 = publication.Releases.Single(r => r.Year == 2020);
        var release2021 = publication.Releases.Single(r => r.Year == 2021);
        var release2022 = publication.Releases.Single(r => r.Year == 2022);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext);

            // Include the unpublished 2021 and 2022 release version id's in the call
            // to test the scenario where both versions are about to be published together
            var result = await service.GetLatestPublishedReleaseVersion(
                publication.Id,
                includeUnpublishedVersionIds:
                [
                    release2021.Versions.Single(rv => rv.Published == null).Id,
                    release2022.Versions.Single(rv => rv.Published == null).Id,
                ]
            );

            // Check the 2020 release version is considered to be the latest published release version,
            // despite the fact that versions for 2021 and 2022 are about to be published,
            // since 2020 is the first release in the release series and has a published version
            Assert.Equal(release2020.Versions[0].Id, result.Id);
        }
    }

    [Fact]
    public async Task CompletePublishing_FirstVersion()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var originalDataBlockParents = _fixture
            .DefaultDataBlockParent()
            .WithLatestDraftVersion(() =>
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .GenerateList(2);

        var actualPublishedDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var releaseFile = new ReleaseFile
        {
            Published = actualPublishedDate.UtcDateTime,
            ReleaseVersionId = releaseVersion.Id,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext: contentDbContext);
            await service.CompletePublishing(releaseVersion.Id, actualPublishedDate);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actualReleaseVersion = await contentDbContext.ReleaseVersions.SingleAsync(rv =>
                rv.Id == releaseVersion.Id
            );

            Assert.Equal(actualPublishedDate, actualReleaseVersion.Published);

            // Expect the published display date to be the actual published date for the first published release version
            Assert.Equal(actualPublishedDate, actualReleaseVersion.PublishedDisplayDate);

            var actualDataBlockParents = await contentDbContext
                .DataBlockParents.Include(dataBlockParent => dataBlockParent.LatestDraftVersion)
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

            Assert.Equal(actualPublishedDate.UtcDateTime, contentDbContext.ReleaseFiles.First().Published);
        }
    }

    [Fact]
    public async Task CompletePublishing_AmendedRelease()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1, draftVersion: true)]);

        var previousReleaseVersion = publication
            .Releases.Single()
            .Versions.Single(rv => rv is { Published: not null, Version: 0 });

        var releaseVersion = publication.Releases.Single().Versions.Single(rv => rv is { Published: null });

        var actualPublishedDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var previousPublishedDate = previousReleaseVersion.Published;

        var amendedReleaseFileId = Guid.NewGuid();
        var amendedFileId = Guid.NewGuid();
        var unamendedReleaseFileId = Guid.NewGuid();
        var unamendedFileId = Guid.NewGuid();

        // Generate Data Blocks for both the previous Release version and for the new Amendment.
        var originalDataBlockParents = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(() =>
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(previousReleaseVersion).Generate()
            )
            .WithLatestDraftVersion(() =>
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(releaseVersion).Generate()
            )
            .GenerateList(2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(previousReleaseVersion, releaseVersion);

            var amendedFile = new File { Id = amendedFileId, Type = FileType.Data };

            var amendedReleaseFile = new ReleaseFile
            {
                Id = amendedReleaseFileId,
                ReleaseVersionId = releaseVersion.Id,
                Name = "file.csv",
                Summary = "Summary text",
                FileId = amendedFileId,
                File = amendedFile,
            };

            var unamendedFile = new File { Id = unamendedFileId, Type = FileType.Data };

            var unamendedReleaseFile = new ReleaseFile
            {
                Id = unamendedReleaseFileId,
                Published = previousPublishedDate?.UtcDateTime,
                ReleaseVersionId = releaseVersion.Id,
                Name = "file.csv",
                Summary = "Summary text",
                FileId = unamendedFileId,
                File = unamendedFile,
            };

            contentDbContext.ReleaseFiles.AddRange(amendedReleaseFile, unamendedReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildReleaseService(contentDbContext: contentDbContext);
            await service.CompletePublishing(releaseVersion.Id, actualPublishedDate);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actualReleaseVersion = await contentDbContext.ReleaseVersions.SingleAsync(rv =>
                rv.Id == releaseVersion.Id
            );

            // TODO EES-6831 This should change to actualPublishedDate, but for now still expect the old behaviour
            // where the published date should have been copied from the previous version
            Assert.Equal(previousReleaseVersion.Published, actualReleaseVersion.Published);

            // Expect the published display date to have been copied from the previous version when 'UpdatePublishedDisplayDate' is false
            Assert.Equal(previousReleaseVersion.PublishedDisplayDate, actualReleaseVersion.PublishedDisplayDate);

            var actualDataBlockParents = await contentDbContext
                .DataBlockParents.Include(dataBlockParent => dataBlockParent.LatestDraftVersion)
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

            Assert.Equal(
                DateTime.UtcNow,
                contentDbContext.ReleaseFiles.Find(amendedReleaseFileId)!.Published!.Value,
                TimeSpan.FromMinutes(1)
            );
            Assert.Equal(
                previousReleaseVersion.Published,
                contentDbContext.ReleaseFiles.Find(unamendedReleaseFileId)!.Published
            );
        }
    }

    [Fact]
    public async Task CompletePublishing_AmendedRelease_DataBlockRemoved()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1, draftVersion: true)]);

        var previousReleaseVersion = publication
            .Releases.Single()
            .Versions.Single(rv => rv is { Published: not null, Version: 0 });

        var releaseVersion = publication.Releases.Single().Versions.Single(rv => rv is { Published: null });

        // Generate Data Blocks for both the previous Release version and for the new Amendment.
        _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(() =>
                _fixture.DefaultDataBlockVersion().WithReleaseVersion(previousReleaseVersion).Generate()
            )
            // This time Data Blocks have been removed from the latest Release amendment, and so they now have no
            // "latest" version.
            .WithLatestDraftVersion((DataBlockVersion)null!)
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
            await service.CompletePublishing(releaseVersion.Id, DateTimeOffset.UtcNow);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var actualDataBlockParents = await contentDbContext
                .DataBlockParents.Include(dataBlockParent => dataBlockParent.LatestDraftVersion)
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
    public async Task CompletePublishing_AmendedReleaseAndUpdatePublishedDisplayDateIsTrue()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases([_fixture.DefaultRelease(publishedVersions: 1, draftVersion: true)]);

        var previousReleaseVersion = publication
            .Releases.Single()
            .Versions.Single(rv => rv is { Published: not null, Version: 0 });

        var releaseVersion = publication.Releases.Single().Versions.Single(rv => rv is { Published: null });
        releaseVersion.UpdatePublishedDisplayDate = true;

        var actualPublishedDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

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
            var actualReleaseVersion = await contentDbContext.ReleaseVersions.SingleAsync(rv =>
                rv.Id == releaseVersion.Id
            );

            Assert.Equal(actualPublishedDate, actualReleaseVersion.Published);

            // Expect the published display date to be the actual published date when 'UpdatePublishedDisplayDate' is true
            Assert.Equal(actualPublishedDate, actualReleaseVersion.PublishedDisplayDate);
        }
    }

    private List<ReleaseSeriesItem> GenerateReleaseSeries(IReadOnlyList<Release> releases, params int[] years)
    {
        return years
            .Select(year =>
            {
                var release = releases.Single(r => r.Year == year);
                return _fixture.DefaultReleaseSeriesItem().WithReleaseId(release.Id).Generate();
            })
            .ToList();
    }

    private static ReleaseService BuildReleaseService(ContentDbContext? contentDbContext = null)
    {
        contentDbContext ??= InMemoryContentDbContext();

        return new ReleaseService(contentDbContext);
    }
}

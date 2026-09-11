#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Bau;

public class DataSetFileVersionGeographicLevelsMigrationControllerTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task WhenCsvHasLevelThatWasNotImported_AddsCsvOnlyLevel()
    {
        File file = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country, GeographicLevel.Region]);

        var contentDbContextId = await SeedFiles(file);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupGetDownloadStream(
            PrivateReleaseFiles,
            file.Path(),
            BuildCsv("National", "Regional", "School")
        );

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var result = await BuildController(contentDbContext, privateBlobStorageService.Object)
                .MigrateCsvOnlyGeographicLevels(dryRun: false);

            Assert.Equal(1, result.Processed);
            Assert.Equal(0, result.Remaining);
            Assert.Empty(result.Errors);
            Assert.Equal(["School"], result.Added[file.Id]);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var geographicLevels = await GetGeographicLevels(contentDbContext, file.Id);

            Assert.Equal(
                [(GeographicLevel.Country, false), (GeographicLevel.Region, false), (GeographicLevel.School, true)],
                geographicLevels
            );
        }
    }

    [Fact]
    public async Task WhenCsvLevelsMatchImportedLevels_SetsAllToNotCsvOnly()
    {
        File file = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country, GeographicLevel.Region]);

        var contentDbContextId = await SeedFiles(file);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupGetDownloadStream(
            PrivateReleaseFiles,
            file.Path(),
            BuildCsv("National", "Regional")
        );

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var result = await BuildController(contentDbContext, privateBlobStorageService.Object)
                .MigrateCsvOnlyGeographicLevels(dryRun: false);

            Assert.Equal(1, result.Processed);
            Assert.Empty(result.Added);
            Assert.Empty(result.Errors);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var geographicLevels = await GetGeographicLevels(contentDbContext, file.Id);

            Assert.Equal([(GeographicLevel.Country, false), (GeographicLevel.Region, false)], geographicLevels);
        }
    }

    [Fact]
    public async Task WhenDryRun_PersistsNothing()
    {
        File file = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country]);

        var contentDbContextId = await SeedFiles(file);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupGetDownloadStream(
            PrivateReleaseFiles,
            file.Path(),
            BuildCsv("National", "School")
        );

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var result = await BuildController(contentDbContext, privateBlobStorageService.Object)
                .MigrateCsvOnlyGeographicLevels();

            Assert.True(result.IsDryRun);
            Assert.Equal(1, result.Processed);
            Assert.Equal(["School"], result.Added[file.Id]);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var geographicLevels = await GetGeographicLevels(contentDbContext, file.Id);

            Assert.Equal([(GeographicLevel.Country, (bool?)null)], geographicLevels);
        }
    }

    [Fact]
    public async Task WhenFileHasNoUnsetLevels_IsNotProcessed()
    {
        File file = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country]);
        file.DataSetFileVersionGeographicLevels[0].CsvOnly = false;

        var contentDbContextId = await SeedFiles(file);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);

        await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

        var result = await BuildController(contentDbContext, privateBlobStorageService.Object)
            .MigrateCsvOnlyGeographicLevels(dryRun: false);

        Assert.Equal(0, result.Processed);
        Assert.Equal(0, result.Remaining);
        privateBlobStorageService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task WhenMoreFilesThanNum_ProcessesBatchAndReportsRemaining()
    {
        var files = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country])
            .GenerateList(3);

        var contentDbContextId = await SeedFiles([.. files]);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        files.ForEach(file =>
            privateBlobStorageService.SetupGetDownloadStream(PrivateReleaseFiles, file.Path(), BuildCsv("National"))
        );

        await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

        var result = await BuildController(contentDbContext, privateBlobStorageService.Object)
            .MigrateCsvOnlyGeographicLevels(dryRun: false, num: 2);

        Assert.Equal(2, result.Processed);
        Assert.Equal(1, result.Remaining);
    }

    [Fact]
    public async Task WhenSkipIsSet_StepsPastFiles()
    {
        var files = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country])
            .GenerateList(3);

        var contentDbContextId = await SeedFiles([.. files]);

        var expectedFile = files.OrderBy(f => f.Id).Last();

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupGetDownloadStream(
            PrivateReleaseFiles,
            expectedFile.Path(),
            BuildCsv("National", "School")
        );

        await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

        var result = await BuildController(contentDbContext, privateBlobStorageService.Object)
            .MigrateCsvOnlyGeographicLevels(dryRun: false, skip: 2);

        Assert.Equal(1, result.Processed);
        Assert.Equal(["School"], result.Added[expectedFile.Id]);
    }

    [Fact]
    public async Task WhenBlobIsMissing_ReportsErrorAndLeavesFileUnset()
    {
        File file = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country]);

        var contentDbContextId = await SeedFiles(file);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService
            .Setup(s => s.GetDownloadStream(PrivateReleaseFiles, file.Path(), true, default))
            .ReturnsAsync(new Either<ActionResult, Stream>(new NotFoundResult()));

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var result = await BuildController(contentDbContext, privateBlobStorageService.Object)
                .MigrateCsvOnlyGeographicLevels(dryRun: false);

            Assert.Equal(0, result.Processed);
            Assert.Equal(1, result.Remaining);
            var error = Assert.Single(result.Errors);
            Assert.Contains(file.Id.ToString(), error);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var geographicLevels = await GetGeographicLevels(contentDbContext, file.Id);

            Assert.Equal([(GeographicLevel.Country, (bool?)null)], geographicLevels);
        }
    }

    [Fact]
    public async Task WhenCsvHasNoGeographicLevelColumn_ReportsErrorAndLeavesFileUnset()
    {
        File file = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country]);

        var contentDbContextId = await SeedFiles(file);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupGetDownloadStream(
            PrivateReleaseFiles,
            file.Path(),
            "time_period,time_identifier\n2020,Calendar year"
        );

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var result = await BuildController(contentDbContext, privateBlobStorageService.Object)
                .MigrateCsvOnlyGeographicLevels(dryRun: false);

            Assert.Equal(0, result.Processed);
            var error = Assert.Single(result.Errors);
            Assert.Contains("geographic_level", error);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var geographicLevels = await GetGeographicLevels(contentDbContext, file.Id);

            Assert.Equal([(GeographicLevel.Country, (bool?)null)], geographicLevels);
        }
    }

    [Fact]
    public async Task WhenImportedLevelIsNotInCsv_ReportsErrorAndLeavesThatLevelUnset()
    {
        File file = _dataFixture
            .DefaultFile(FileType.Data)
            .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country, GeographicLevel.Region]);

        var contentDbContextId = await SeedFiles(file);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupGetDownloadStream(
            PrivateReleaseFiles,
            file.Path(),
            BuildCsv("National", "School")
        );

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var result = await BuildController(contentDbContext, privateBlobStorageService.Object)
                .MigrateCsvOnlyGeographicLevels(dryRun: false);

            Assert.Equal(1, result.Processed);
            var error = Assert.Single(result.Errors);
            Assert.Contains("Regional", error);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var geographicLevels = await GetGeographicLevels(contentDbContext, file.Id);

            Assert.Equal(
                [
                    (GeographicLevel.Country, false),
                    (GeographicLevel.Region, (bool?)null),
                    (GeographicLevel.School, true),
                ],
                geographicLevels
            );
        }
    }

    private async Task<string> SeedFiles(params File[] files)
    {
        var contentDbContextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
        contentDbContext.Files.AddRange(files);
        await contentDbContext.SaveChangesAsync();

        return contentDbContextId;
    }

    private static async Task<List<(GeographicLevel, bool?)>> GetGeographicLevels(
        ContentDbContext contentDbContext,
        Guid fileId
    ) =>
        await contentDbContext
            .DataSetFileVersionGeographicLevels.Where(gl => gl.DataSetFileVersionId == fileId)
            .OrderBy(gl => gl.GeographicLevel)
            .Select(gl => new ValueTuple<GeographicLevel, bool?>(gl.GeographicLevel, gl.CsvOnly))
            .ToListAsync();

    private static string BuildCsv(params string[] geographicLevels) =>
        string.Join(
            "\n",
            [
                "time_period,time_identifier,geographic_level",
                .. geographicLevels.Select(gl => $"2020,Calendar year,{gl}"),
            ]
        );

    private static DataSetFileVersionGeographicLevelsMigrationController BuildController(
        ContentDbContext contentDbContext,
        IPrivateBlobStorageService privateBlobStorageService
    ) =>
        new(
            contentDbContext,
            privateBlobStorageService,
            Mock.Of<ILogger<DataSetFileVersionGeographicLevelsMigrationController>>()
        );
}

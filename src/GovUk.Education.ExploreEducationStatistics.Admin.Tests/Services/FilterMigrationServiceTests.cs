#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

/// <summary>
/// TODO EES-4372 Remove after the EES-4364 filter migration is complete
/// </summary>
public class FilterMigrationServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task MigrateGroupCsvColumns()
    {
        var subject = _fixture.DefaultSubject()
            // Filters have non-default filter groups and no group csv column values are set
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .Generate(3))
            .Generate();

        var file = new File
        {
            RootPath = Guid.NewGuid(),
            SubjectId = subject.Id,
            Type = FileType.Metadata
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.Files.AddRangeAsync(file);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources", "FilterMigrationFiles", "test-data.meta.csv");

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupStreamBlob(
            container: BlobContainers.PrivateReleaseFiles,
            expectedBlobPath: file.Path(),
            filePathToStream: metaFilePath);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var expectedFiltersUpdated = new List<Guid>
            {
                subject.Filters[0].Id,
                subject.Filters[1].Id,
                subject.Filters[2].Id
            };

            Assert.Empty(report.Errors);
            Assert.Empty(report.Information);
            Assert.Equal(expectedFiltersUpdated, report.FiltersUpdated);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Equal(3, await statisticsDbContext.Filter.CountAsync());

            var filter0 = await statisticsDbContext.Filter.SingleAsync(f => f.Id == subject.Filters[0].Id);
            Assert.Equal("filter_0_group", filter0.GroupCsvColumn);

            var filter1 = await statisticsDbContext.Filter.SingleAsync(f => f.Id == subject.Filters[1].Id);
            Assert.Equal("filter_1_group", filter1.GroupCsvColumn);

            var filter2 = await statisticsDbContext.Filter.SingleAsync(f => f.Id == subject.Filters[2].Id);
            Assert.Equal("filter_2_group", filter2.GroupCsvColumn);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_MetaBlobNotFound()
    {
        var subject = _fixture.DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .Generate(2))
            .Generate();

        var file = new File
        {
            RootPath = Guid.NewGuid(),
            SubjectId = subject.Id,
            Type = FileType.Metadata
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.Files.AddRangeAsync(file);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupStreamBlobNotFound(
            container: BlobContainers.PrivateReleaseFiles,
            expectedBlobPath: file.Path());

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var report = result.AssertRight();

            var expectedErrors = new List<FilterMigrationReportErrorItem>
            {
                FilterMigrationReportErrorItem.ExceptionGettingMetaFilters(
                    subject.Id, file, $"Could not find file at {BlobContainers.PrivateReleaseFiles}/{file.Path()}")
            };

            Assert.Equal(expectedErrors, report.Errors);
            Assert.Empty(report.Information);
            Assert.Empty(report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_MetaFilterHasBlankName()
    {
        var subject = _fixture.DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .Generate(2))
            .Generate();

        var file = new File
        {
            RootPath = Guid.NewGuid(),
            SubjectId = subject.Id,
            Type = FileType.Metadata
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.Files.AddRangeAsync(file);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources", "FilterMigrationFiles", "blank-filter-name.meta.csv");

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupStreamBlob(
            container: BlobContainers.PrivateReleaseFiles,
            expectedBlobPath: file.Path(),
            filePathToStream: metaFilePath);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var expectedErrors = new List<FilterMigrationReportErrorItem>
            {
                FilterMigrationReportErrorItem.ExceptionGettingMetaFilters(
                    subject.Id, file, $"Csv filter name is null (Parameter 'col_name')")
            };

            Assert.Equal(expectedErrors, report.Errors);
            Assert.Empty(report.Information);
            Assert.Empty(report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_MetaFilterHasBlankLabel()
    {
        var subject = _fixture.DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .Generate(2))
            .Generate();

        var file = new File
        {
            RootPath = Guid.NewGuid(),
            SubjectId = subject.Id,
            Type = FileType.Metadata
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.Files.AddRangeAsync(file);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources", "FilterMigrationFiles", "blank-filter-label.meta.csv");

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupStreamBlob(
            container: BlobContainers.PrivateReleaseFiles,
            expectedBlobPath: file.Path(),
            filePathToStream: metaFilePath);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var expectedErrors = new List<FilterMigrationReportErrorItem>
            {
                FilterMigrationReportErrorItem.ExceptionGettingMetaFilters(
                    subject.Id, file, $"Csv filter label is null (Parameter 'label')")
            };

            Assert.Equal(expectedErrors, report.Errors);
            Assert.Empty(report.Information);
            Assert.Empty(report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_MetaFilterNotFound()
    {
        var subject = _fixture.DefaultSubject()
            // Last filter is not in meta csv file
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .Generate(4))
            .Generate();

        var file = new File
        {
            RootPath = Guid.NewGuid(),
            SubjectId = subject.Id,
            Type = FileType.Metadata
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.Files.AddRangeAsync(file);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources", "FilterMigrationFiles", "test-data.meta.csv");

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupStreamBlob(
            container: BlobContainers.PrivateReleaseFiles,
            expectedBlobPath: file.Path(),
            filePathToStream: metaFilePath);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var expectedErrors = new List<FilterMigrationReportErrorItem>
            {
                FilterMigrationReportErrorItem.MetaFilterNotFound(
                    subject.Id, file, subject.Filters[3])
            };

            var expectedFiltersUpdated = new List<Guid>
            {
                subject.Filters[0].Id,
                subject.Filters[1].Id,
                subject.Filters[2].Id
            };

            Assert.Equal(expectedErrors, report.Errors);
            Assert.Empty(report.Information);
            Assert.Equal(expectedFiltersUpdated, report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_FilterLabelMismatch()
    {
        var subject = _fixture.DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .Generate(3))
            .Generate();

        // Set the label of one of the filters to be different to the label in the csv
        // Meta filter name "name_of_filter_1" has label "Label of Filter 1"
        subject.Filters[1].Label = "Label of Filter 1 modified";

        var file = new File
        {
            RootPath = Guid.NewGuid(),
            SubjectId = subject.Id,
            Type = FileType.Metadata
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.Files.AddRangeAsync(file);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources", "FilterMigrationFiles", "test-data.meta.csv");

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupStreamBlob(
            container: BlobContainers.PrivateReleaseFiles,
            expectedBlobPath: file.Path(),
            filePathToStream: metaFilePath);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var expectedErrors = new List<FilterMigrationReportErrorItem>
            {
                FilterMigrationReportErrorItem.FilterLabelMismatch(
                    subject.Id, file, subject.Filters[1])
            };

            var expectedFiltersUpdated = new List<Guid>
            {
                subject.Filters[0].Id,
                subject.Filters[2].Id
            };

            Assert.Equal(expectedErrors, report.Errors);
            Assert.Empty(report.Information);
            Assert.Equal(expectedFiltersUpdated, report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_MetaFileNotFound()
    {
        var subject = _fixture.DefaultSubject()
            // Filters have non-default filter groups and no group csv column values are set
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .Generate(2))
            .Generate();

        var contextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            var expectedErrors = new List<FilterMigrationReportErrorItem>
            {
                FilterMigrationReportErrorItem.MetaFileNotFound(subject.Id)
            };

            Assert.Equal(expectedErrors, report.Errors);
            Assert.Empty(report.Information);
            Assert.Empty(report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_SomeFiltersHaveGroupCsvColumnValues()
    {
        var subject = _fixture.DefaultSubject()
            // Filters have non-default filter groups but two out of three have group csv column values are already set
            .WithFilters(_fixture.DefaultFilter()
                .ForIndex(0, s => s.SetGroupCsvColumn("filter_0_group"))
                .ForIndex(2, s => s.SetGroupCsvColumn("filter_2_group"))
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup(filterItemCount: 2)
                    .Generate(2))
                .Generate(3))
            .Generate();

        var file = new File
        {
            RootPath = Guid.NewGuid(),
            SubjectId = subject.Id,
            Type = FileType.Metadata
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.Files.AddRangeAsync(file);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var metaFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources", "FilterMigrationFiles", "test-data.meta.csv");

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        privateBlobStorageService.SetupStreamBlob(
            container: BlobContainers.PrivateReleaseFiles,
            expectedBlobPath: file.Path(),
            filePathToStream: metaFilePath);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var expectedInformation = new List<FilterMigrationReportInfoItem>
            {
                FilterMigrationReportInfoItem.FilterAlreadyHasGroupCsvColumnValue(subject.Id, file, subject.Filters[0]),
                FilterMigrationReportInfoItem.FilterAlreadyHasGroupCsvColumnValue(subject.Id, file, subject.Filters[2])
            };

            var expectedFiltersUpdated = new List<Guid>
            {
                subject.Filters[1].Id
            };

            Assert.Empty(report.Errors);
            Assert.Equal(expectedInformation, report.Information);
            Assert.Equal(expectedFiltersUpdated, report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_AllFiltersHaveGroupCsvColumnValues()
    {
        var subject = _fixture.DefaultSubject()
            // Filters have non-default filter groups but all group csv column values are already set
            .WithFilters(_fixture.DefaultFilter()
                .ForIndex(0, s => s.SetGroupCsvColumn("filter_0_group"))
                .ForIndex(1, s => s.SetGroupCsvColumn("filter_1_group"))
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup(filterItemCount: 2).Generate(2))
                .Generate(2))
            .Generate();

        var contextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            var expectedInformation = new List<FilterMigrationReportInfoItem>
            {
                FilterMigrationReportInfoItem.AllFiltersHaveGroupCsvColumnValues(subject.Id)
            };

            Assert.Empty(report.Errors);
            Assert.Equal(expectedInformation, report.Information);
            Assert.Empty(report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_FiltersOnlyHaveDefaultFilterGroups()
    {
        var subject = _fixture.DefaultSubject()
            // Filters have only one default filter group each
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
                .Generate(2))
            .Generate();

        var contextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            Assert.Empty(report.Errors);
            Assert.Empty(report.Information);
            Assert.Empty(report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_EmptyFilters()
    {
        var subject = _fixture.DefaultSubject()
            // Filters have no filter groups
            .WithFilters(_fixture.DefaultFilter()
                .Generate(2))
            .Generate();

        var contextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Subject.AddRangeAsync(subject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.MigrateGroupCsvColumns(dryRun: false);

            var report = result.AssertRight();

            Assert.Empty(report.Errors);
            Assert.Empty(report.Information);
            Assert.Empty(report.FiltersUpdated);
        }
    }

    [Fact]
    public async Task MigrateGroupCsvColumns_NoFilters()
    {
        await using var contentDbContext = InMemoryContentDbContext();
        await using var statisticsDbContext = InMemoryStatisticsDbContext();

        var service = SetupService(contentDbContext: contentDbContext,
            statisticsDbContext: statisticsDbContext);

        var result = await service.MigrateGroupCsvColumns();
        var report = result.AssertRight();

        Assert.Empty(report.Errors);
        Assert.Empty(report.Information);
        Assert.Empty(report.FiltersUpdated);
    }

    private static FilterMigrationService SetupService(
        ContentDbContext? contentDbContext = null,
        StatisticsDbContext? statisticsDbContext = null,
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IUserService? userService = null,
        ILogger<FilterMigrationService>? logger = null)
    {
        return new FilterMigrationService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(MockBehavior.Strict),
            userService ?? MockUtils.AlwaysTrueUserService().Object,
            logger ?? Mock.Of<ILogger<FilterMigrationService>>()
        );
    }
}

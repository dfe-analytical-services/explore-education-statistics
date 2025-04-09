#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using System;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetValidatorTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task ValidateDataSet_Valid_ReturnsDataSetObject()
    {
        // Arrange
        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateDataSet(
            Guid.NewGuid(),
            "Data set title",
            [dataFile, metaFile],
            replacingFile: null);

        // Assert
        var dataSet = result.AssertRight();
        Assert.Equal("Data set title", dataSet.Title);
        Assert.Equal("test-data.csv", dataSet.DataFile.FileName);
        Assert.Equal(dataFile.FileStream.Length, dataSet.DataFile.FileSize);
        Assert.Equal("test-data.meta.csv", dataSet.MetaFile.FileName);
        Assert.Equal(metaFile.FileStream.Length, dataSet.MetaFile.FileSize);
    }

    [Fact]
    public async Task ValidateDataSet_ValidWithReplacement_ReturnsDataSetObject()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var toBeReplacedDataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName("Data set title")
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test-data.csv"))
            .Generate();

        var toBeReplacedMetaReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Metadata)
                .WithFilename("test-data.meta.csv"))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(toBeReplacedDataReleaseFile, toBeReplacedMetaReleaseFile);
            await context.SaveChangesAsync();
        }

        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateDataSet(
                Guid.NewGuid(),
                "Data set title",
                [dataFile, metaFile],
                toBeReplacedDataReleaseFile.File);

            // Assert
            var dataSet = result.AssertRight();
            Assert.Equal("Data set title", dataSet.Title);
            Assert.Equal("test-data.csv", dataSet.DataFile.FileName);
            Assert.Equal(dataFile.FileStream.Length, dataSet.DataFile.FileSize);
            Assert.Equal("test-data.meta.csv", dataSet.MetaFile.FileName);
            Assert.Equal(metaFile.FileStream.Length, dataSet.MetaFile.FileSize);
        }
    }

    [Fact]
    public async Task ValidateDataSet_DataSetFilesAreEmpty_ReturnsErrorDetails()
    {
        // Arrange
        var dataFile = await new DataSetFileBuilder().WhereFileSizeIsZero().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().WhereFileSizeIsZero().Build(FileType.Metadata);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateDataSet(
            Guid.NewGuid(),
            "Data set title",
            [dataFile, metaFile],
            replacingFile: null);

        // Assert
        var errors = result.AssertLeft();
        Assert.Equal(2, errors.Count);
        Assert.Equal(ValidationMessages.FileSizeMustNotBeZero.Code, errors[0].Code);
        Assert.Equal(ValidationMessages.FileSizeMustNotBeZero.Code, errors[1].Code);
    }

    [Fact]
    public async Task ValidateDataSet_DataSetAlreadyExists_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var dataSetTitle = "Data set title";

        var existingReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test-data.csv"))
            .Generate();

        var existingMetaReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Metadata)
                .WithFilename("test-data.meta.csv"))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(existingReleaseFile, existingMetaReleaseFile);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
            var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateDataSet(
                releaseVersion.Id,
                dataSetTitle,
                [dataFile, metaFile],
                replacingFile: null);

            // Assert
            var errors = result.AssertLeft();
            Assert.Equal(2, errors.Count);
            Assert.Equal(ValidationMessages.DataSetTitleShouldBeUnique.Code, errors[0].Code);
            Assert.Equal(ValidationMessages.FileNameNotUnique.Code, errors[1].Code);
        }
    }

    [Theory]
    [InlineData(FileType.Data)] // Metadata file is missing
    [InlineData(FileType.Metadata)] // Data file is missing
    public async Task ValidateDataSet_IncompleteDataSetFilePair_ReturnsErrorDetails(FileType includeFileType)
    {
        // Arrange
        var dataSetFile = await new DataSetFileBuilder().Build(includeFileType);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateDataSet(
            Guid.NewGuid(),
            "Data set title",
            [dataSetFile],
            replacingFile: null);

        // Assert
        var errors = result.AssertLeft();
        Assert.Equal(2, errors.Count);
        Assert.Equal(ValidationMessages.DataSetShouldContainTwoFiles.Code, errors[0].Code);
        Assert.Equal(ValidationMessages.DataSetFileNamesShouldMatchConvention.Code, errors[1].Code);
    }

    [Fact]
    public async Task ValidateDataSet_TooManyFilesProvided_ReturnsErrorDetails()
    {
        // Arrange
        var dataFile1 = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile1 = await new DataSetFileBuilder().Build(FileType.Metadata);
        var dataFile2 = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile2 = await new DataSetFileBuilder().Build(FileType.Metadata);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateDataSet(
            Guid.NewGuid(),
            "Data set title",
            [dataFile1, metaFile1, dataFile2, metaFile2],
            replacingFile: null);

        // Assert
        var errors = result.AssertLeft();
        Assert.Single(errors);
        Assert.Equal(ValidationMessages.DataSetShouldContainTwoFiles.Code, errors[0].Code);
    }

    [Fact]
    public async Task ValidateDataSet_ReplacementHasApiDataSet_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), };

        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var existingDataReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName("Data set title")
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename(dataFile.FileName))
            .WithPublicApiDataSetId(Guid.NewGuid())
            .Generate();

        var existingMetaReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Metadata)
                .WithFilename(metaFile.FileName))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var context = InMemoryContentDbContext(contentDbContextId);

        context.ReleaseFiles.AddRange(existingDataReleaseFile, existingMetaReleaseFile);
        await context.SaveChangesAsync();

        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateDataSet(
            releaseVersion.Id,
            "Data set title",
            [dataFile, metaFile],
            existingDataReleaseFile.File);

        // Assert
        var errors = result.AssertLeft();
        Assert.Single(errors);
        Assert.Equal(ValidationMessages.CannotReplaceDataSetWithApiDataSet.Code, errors[0].Code);
    }

    private static DataSetValidator BuildService(ContentDbContext? contentDbContext = null)
    {
        return new DataSetValidator(contentDbContext!);
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Options;
using Moq;
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

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = Guid.NewGuid(),
            Title = "Data set title",
            DataFile = dataFile,
            MetaFile = metaFile,
        };

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateDataSet(dataSetDto);

        // Assert
        var dataSet = result.AssertRight();
        Assert.Equal("Data set title", dataSet.Title);
        Assert.Equal("test-data.csv", dataSet.DataFile.FileName);
        Assert.Equal(dataFile.FileStreamProvider().Length, dataSet.DataFile.FileSize);
        Assert.Equal("test-data.meta.csv", dataSet.MetaFile.FileName);
        Assert.Equal(metaFile.FileStreamProvider().Length, dataSet.MetaFile.FileSize);
    }

    // Filenames that are equal ignoring case to the original filename are valid
    // filenames for replacements.
    [Theory]
    [InlineData("test-data.csv")]
    [InlineData("Test-Data.csv")]
    public async Task ValidateDataSet_ValidWithReplacement_ReturnsDataSetObject()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var dataSetTitle = "Data set title";

        var toBeReplacedDataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
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

        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = releaseVersion.Id,
            Title = dataSetTitle,
            DataFile = dataFile,
            MetaFile = metaFile,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(toBeReplacedDataReleaseFile, toBeReplacedMetaReleaseFile);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateDataSet(dataSetDto);

            // Assert
            var dataSet = result.AssertRight();
            Assert.Equal("Data set title", dataSet.Title);
            Assert.Equal("test-data.csv", dataSet.DataFile.FileName);
            Assert.Equal(dataFile.FileStreamProvider().Length, dataSet.DataFile.FileSize);
            Assert.Equal("test-data.meta.csv", dataSet.MetaFile.FileName);
            Assert.Equal(metaFile.FileStreamProvider().Length, dataSet.MetaFile.FileSize);
        }
    }

    [Fact]
    public async Task ValidateDataSet_ReplacementFileNameMatchesAnotherDataSetsFileName_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var dataSetTitle = "Data set title";

        var toBeReplacedDataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
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

        var existingReleaseFileWithMatchingDataFileName = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName("Other data set title")
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test-data-replacement.csv"))
            .Generate();

        var replacementDataFile = await new DataSetFileBuilder().WhereFileNameIs("test-data-replacement.csv").Build(FileType.Data);
        var replacementMetaFile = await new DataSetFileBuilder().WhereFileNameIs("test-data-replacement.meta.csv").Build(FileType.Metadata);

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = releaseVersion.Id,
            Title = dataSetTitle,
            DataFile = replacementDataFile,
            MetaFile = replacementMetaFile,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(
                toBeReplacedDataReleaseFile,
                toBeReplacedMetaReleaseFile,
                existingReleaseFileWithMatchingDataFileName);

            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateDataSet(dataSetDto);

            // Assert
            var errors = result.AssertLeft();
            Assert.Single(errors);
            Assert.Equal(ValidationMessages.FileNameNotUnique.Code, errors[0].Code);
        }
    }

    [Fact]
    public async Task ValidateDataSet_ToBeReplacedFileHasIncompleteImport_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var dataSetTitle = "Data set title";

        var toBeReplacedDataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
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

        var toBeReplacedDataFileIncompleteImport = new DataImport
        {
            Id = Guid.NewGuid(),
            SubjectId = releaseVersion.Id,
            Status = DataImportStatus.STAGE_1,
            File = toBeReplacedDataReleaseFile.File,
        };

        var replacementDataFile = await new DataSetFileBuilder().WhereFileNameIs("test-data.csv").Build(FileType.Data);
        var replacementMetaFile = await new DataSetFileBuilder().WhereFileNameIs("test-data.meta.csv").Build(FileType.Metadata);

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = releaseVersion.Id,
            Title = dataSetTitle,
            DataFile = replacementDataFile,
            MetaFile = replacementMetaFile,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.AddRange(
                toBeReplacedDataReleaseFile,
                toBeReplacedMetaReleaseFile,
                toBeReplacedDataFileIncompleteImport);

            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateDataSet(dataSetDto);

            // Assert
            var errors = result.AssertLeft();
            Assert.Single(errors);
            Assert.Equal(ValidationMessages.DataSetImportInProgress.Code, errors[0].Code);
        }
    }

    [Fact]
    public async Task ValidateDataSet_DataSetFilesAreEmpty_ReturnsErrorDetails()
    {
        // Arrange
        var dataFile = await new DataSetFileBuilder().WhereFileSizeIsZero().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().WhereFileSizeIsZero().Build(FileType.Metadata);

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = Guid.NewGuid(),
            Title = "Data set title",
            DataFile = dataFile,
            MetaFile = metaFile,
        };

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateDataSet(dataSetDto);

        // Assert
        var errors = result.AssertLeft();
        Assert.Equal(2, errors.Count);
        Assert.Equal(ValidationMessages.FileSizeMustNotBeZero.Code, errors[0].Code);
        Assert.Equal(ValidationMessages.FileSizeMustNotBeZero.Code, errors[1].Code);
    }

    [Fact]
    public async Task ValidateDataSet_DataSetAlreadyExists_IdentifiesDataSetReplacement()
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
            var dataFile = await new DataSetFileBuilder().WhereFileNameIs("test-data-replacement.csv").Build(FileType.Data);
            var metaFile = await new DataSetFileBuilder().WhereFileNameIs("test-data-replacement.meta.csv").Build(FileType.Metadata);

            var dataSetDto = new DataSetDto
            {
                ReleaseVersionId = releaseVersion.Id,
                Title = "Data set title",
                DataFile = dataFile,
                MetaFile = metaFile,
            };

            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateDataSet(dataSetDto);

            // Assert
            var dataSet = result.AssertRight();
            Assert.Equal("Data set title", dataSet.Title);
            Assert.Equal("test-data-replacement.csv", dataSet.DataFile.FileName);
            Assert.Equal("test-data-replacement.meta.csv", dataSet.MetaFile.FileName);
            Assert.NotNull(dataSet.ReplacingFile);
            Assert.Equal("test-data.csv", dataSet.ReplacingFile.Filename);
        }
    }

    [Theory]
    [InlineData(FileType.Data)] // Metadata file is missing
    [InlineData(FileType.Metadata)] // Data file is missing
    public async Task ValidateDataSet_IncompleteDataSetFilePair_ReturnsErrorDetails(FileType includeFileType)
    {
        // Arrange
        var dataFile = await new DataSetFileBuilder().Build(includeFileType);
        var metaFile = await new DataSetFileBuilder().Build(includeFileType);

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = Guid.NewGuid(),
            Title = "Data set title",
            DataFile = includeFileType == FileType.Data ? dataFile : null,
            MetaFile = includeFileType == FileType.Metadata ? metaFile : null,
        };

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateDataSet(dataSetDto);

        // Assert
        var errors = result.AssertLeft();
        Assert.Single(errors);
        Assert.Equal(ValidationMessages.DataSetFileNamesShouldMatchConvention.Code, errors[0].Code);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidateDataSet_ReplacementHasApiDataSet_ReturnsErrorDetails(bool enableFeatureFlagPatchReplacements)
    {
        // Arrange
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), Version = 2 };

        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataSetTitle = "Data set title";

        var existingDataReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
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

        var userService = new Mock<IUserService>(MockBehavior.Strict);
        userService
            .Setup(s => s.MatchesPolicy(SecurityPolicies.IsBauUser))
            .ReturnsAsync(true);

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = releaseVersion.Id,
            Title = dataSetTitle,
            DataFile = dataFile,
            MetaFile = metaFile,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var context = InMemoryContentDbContext(contentDbContextId);

        context.ReleaseFiles.AddRange(existingDataReleaseFile, existingMetaReleaseFile);
        await context.SaveChangesAsync();

        var featureFlagOptions = Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
        {
            EnableReplacementOfPublicApiDataSets = enableFeatureFlagPatchReplacements
        });

        var sut = BuildService(
            context,
            featureFlagOptions,
            userService.Object);

        // Act
        var result = await sut.ValidateDataSet(dataSetDto);

        // Assert
        if (enableFeatureFlagPatchReplacements)
        {
            result.AssertRight();
        }
        else
        {
            var errors = result.AssertLeft();
            Assert.Single(errors);
            Assert.Equal(ValidationMessages.CannotReplaceDataSetWithApiDataSet.Code, errors[0].Code);
        }
    }

    [Fact]
    public async Task ValidateDataSet_AnalystUserPatchReplacement_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), Version = 1 };
        var amendmentReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), Version = 2 };

        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataSetTitle = "Data set title";

        var existingDataReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
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

        var replacementDataReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(amendmentReleaseVersion)
            .WithName(dataSetTitle)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename(dataFile.FileName))
            .WithPublicApiDataSetId(Guid.NewGuid())
            .Generate();

        var replacementMetaReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(amendmentReleaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Metadata)
                .WithFilename(metaFile.FileName))
            .Generate();

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = releaseVersion.Id,
            Title = dataSetTitle,
            DataFile = dataFile,
            MetaFile = metaFile
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var context = InMemoryContentDbContext(contentDbContextId);

        context.ReleaseFiles.AddRange(
            existingDataReleaseFile,
            existingMetaReleaseFile,
            replacementDataReleaseFile,
            replacementMetaReleaseFile);
        await context.SaveChangesAsync();

        var userService = new Mock<IUserService>(MockBehavior.Strict);
        userService
            .Setup(s => s.MatchesPolicy(SecurityPolicies.IsBauUser))
            .ReturnsAsync(false);

        var featureFlagOptions = Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
        {
            EnableReplacementOfPublicApiDataSets = true
        });

        var sut = BuildService(
            context,
            featureFlagOptions,
            userService.Object);

        // Act
        var result = await sut.ValidateDataSet(dataSetDto);

        // Assert
        var errors = result.AssertLeft();
        Assert.Single(errors);
        Assert.Equal(ValidationMessages.AnalystCannotReplaceApiDataSet.Code, errors[0].Code);
    }

    [Fact]
    public async Task ValidateDataSet_MultipleDraftAPIDatasetVersions_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), Version = 1 };
        var amendmentReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), Version = 2 };

        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataSetTitle = "Data set title";

        var existingDataReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
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

        var replacementDataReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(amendmentReleaseVersion)
            .WithName(dataSetTitle)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename(dataFile.FileName))
            .WithPublicApiDataSetId(existingDataReleaseFile.PublicApiDataSetId.Value)
            .Generate();

        var replacementMetaReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(amendmentReleaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Metadata)
                .WithFilename(metaFile.FileName))
            .Generate();

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = releaseVersion.Id,
            Title = dataSetTitle,
            DataFile = dataFile,
            MetaFile = metaFile
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var context = InMemoryContentDbContext(contentDbContextId);

        context.ReleaseFiles.AddRange(
            existingDataReleaseFile,
            existingMetaReleaseFile,
            replacementDataReleaseFile,
            replacementMetaReleaseFile);
        await context.SaveChangesAsync();

        var dataSetServiceMock = new Mock<IDataSetService>(MockBehavior.Strict);
        dataSetServiceMock
            .Setup(s => s.HasDraftVersion(existingDataReleaseFile.PublicApiDataSetId.Value, CancellationToken.None))
            .ReturnsAsync(true);

        var sut = BuildService(
                contentDbContext: context,
                dataSetService: dataSetServiceMock.Object
            );

        // Act
        var result = await sut.ValidateDataSet(dataSetDto);

        // Assert
        var errors = result.AssertLeft();
        Assert.Single(errors);
        Assert.Equal(ValidationMessages.CannotCreateMultipleDraftApiDataSet.Code, errors[0].Code);
    }

    [Fact]
    public async Task ValidateBulkDataZipIndexFile_Valid_ReturnsIndexFileObject()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var indexFile = await new DataSetFileBuilder().Build(FileType.BulkDataZipIndex);
        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateBulkDataZipIndexFile(releaseVersion.Id, indexFile, [dataFile, metaFile]);

        // Assert
        var dataSetIndex = result.AssertRight();
        Assert.Equal(releaseVersion.Id, dataSetIndex.ReleaseVersionId);

        var dataSetItem = Assert.Single(dataSetIndex.DataSetIndexItems);
        Assert.Equal("Data set title", dataSetItem.DataSetTitle);
        Assert.Equal("test-data.csv", dataSetItem.DataFileName);
        Assert.Equal("test-data.meta.csv", dataSetItem.MetaFileName);
    }

    [Fact]
    public async Task ValidateBulkDataZipIndexFile_ValidWithDotCsvExtension_ReturnsIndexFileObject()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var indexFile = await new DataSetFileBuilder()
            .WhereFileNameIs("dataset_names-invalid-filename-contains-extension.csv")
            .Build(FileType.BulkDataZipIndex);

        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateBulkDataZipIndexFile(releaseVersion.Id, indexFile, [dataFile, metaFile]);

        // Assert
        var dataSetIndex = result.AssertRight();
        Assert.Equal(releaseVersion.Id, dataSetIndex.ReleaseVersionId);

        var dataSetItem = Assert.Single(dataSetIndex.DataSetIndexItems);
        Assert.Equal("First data set title", dataSetItem.DataSetTitle);
        Assert.Equal("test-data.csv", dataSetItem.DataFileName);
        Assert.Equal("test-data.meta.csv", dataSetItem.MetaFileName);
    }

    [Fact]
    public async Task ValidateBulkDataZipIndexFile_ValidWithReplacement_ReturnsIndexFileObject()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var dataSetTitle = "Data set title";

        var toBeReplacedDataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
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

        var indexFile = await new DataSetFileBuilder().Build(FileType.BulkDataZipIndex);
        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateBulkDataZipIndexFile(releaseVersion.Id, indexFile, [dataFile, metaFile]);

            // Assert
            var dataSetIndex = result.AssertRight();
            Assert.Equal(releaseVersion.Id, dataSetIndex.ReleaseVersionId);

            var dataSetItem = Assert.Single(dataSetIndex.DataSetIndexItems);
            Assert.Equal("Data set title", dataSetItem.DataSetTitle);
            Assert.Equal("test-data.csv", dataSetItem.DataFileName);
            Assert.Equal("test-data.meta.csv", dataSetItem.MetaFileName);
        }
    }

    [Fact]
    public async Task ValidateBulkDataZipIndexFile_ReplacementInProgress_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var dataSetTitle = "Data set title";

        var originalDataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test-data.csv"))
            .Generate();

        var toBeReplacedDataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test-data.csv"))
            .Generate();

        originalDataReleaseFile.File.ReplacedBy = toBeReplacedDataReleaseFile.File;

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(originalDataReleaseFile, toBeReplacedDataReleaseFile);
            await context.SaveChangesAsync();
        }

        var indexFile = await new DataSetFileBuilder().Build(FileType.BulkDataZipIndex);
        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var sut = BuildService(context);

            // Act
            var result = await sut.ValidateBulkDataZipIndexFile(releaseVersion.Id, indexFile, [dataFile, metaFile]);

            // Assert
            var errors = result.AssertLeft();

            var error = Assert.Single(errors);
            Assert.Equal(ValidationMessages.DataReplacementAlreadyInProgress.Code, error.Code);
        }
    }

    [Fact]
    public async Task ValidateBulkDataZipIndexFile_InvalidHeaders_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var indexFile = await new DataSetFileBuilder()
            .WhereFileNameIs("dataset_names_invalid-headers.csv")
            .Build(FileType.BulkDataZipIndex);

        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateBulkDataZipIndexFile(releaseVersion.Id, indexFile, [dataFile, metaFile]);

        // Assert
        var errors = result.AssertLeft();

        var error = Assert.Single(errors);
        Assert.Equal(ValidationMessages.DataSetNamesCsvIncorrectHeaders.Code, error.Code);
    }

    [Fact]
    public async Task ValidateBulkDataZipIndexFile_DuplicateRecords_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var indexFile = await new DataSetFileBuilder()
            .WhereFileNameIs("dataset_names_duplicate-records.csv")
            .Build(FileType.BulkDataZipIndex);

        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateBulkDataZipIndexFile(releaseVersion.Id, indexFile, [dataFile, metaFile]);

        // Assert
        var errors = result.AssertLeft();

        Assert.Equal(2, errors.Count);
        Assert.Equal(ValidationMessages.DataSetTitleShouldBeUnique.Code, errors[0].Code);
        Assert.Equal(ValidationMessages.DataSetNamesCsvFileNamesShouldBeUnique.Code, errors[1].Code);
    }

    [Fact]
    public async Task ValidateBulkDataZipIndexFile_ReferencedFilesNotFoundInZipFile_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var indexFile = await new DataSetFileBuilder()
            .WhereFileNameIs("dataset_names-invalid-unused-files.csv")
            .Build(FileType.BulkDataZipIndex);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateBulkDataZipIndexFile(releaseVersion.Id, indexFile, []);

        // Assert
        var errors = result.AssertLeft();
        Assert.Equal(2, errors.Count);
        Assert.Equal(ValidationMessages.FileNotFoundInZip.Code, errors[0].Code);
        Assert.Equal(ValidationMessages.FileNotFoundInZip.Code, errors[1].Code);
    }

    [Fact]
    public async Task ValidateBulkDataZipIndexFile_UnusedFiles_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var indexFile = await new DataSetFileBuilder().Build(FileType.BulkDataZipIndex);
        var dataFile1 = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile1 = await new DataSetFileBuilder().Build(FileType.Metadata);

        var dataFile2 = await new DataSetFileBuilder()
            .WhereFileNameIs("test-data-unused.csv")
            .Build(FileType.Data);
        var metaFile2 = await new DataSetFileBuilder()
            .WhereFileNameIs("test-data-unused.meta.csv")
            .Build(FileType.Metadata);

        await using var context = InMemoryContentDbContext();
        var sut = BuildService(context);

        // Act
        var result = await sut.ValidateBulkDataZipIndexFile(releaseVersion.Id, indexFile, [dataFile1, metaFile1, dataFile2, metaFile2]);

        // Assert
        var errors = result.AssertLeft();
        var error = Assert.Single(errors);
        Assert.Equal(ValidationMessages.ZipContainsUnusedFiles.Code, error.Code);
    }

    private static DataSetValidator BuildService(
        ContentDbContext? contentDbContext = null,
        IOptions<FeatureFlagsOptions>? featureFlags = null,
        IUserService? userService = null,
        IDataSetService? dataSetService = null)
    {
        if (userService is null)
        {
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(s => s.MatchesPolicy(SecurityPolicies.IsBauUser))
                .ReturnsAsync(true);
            userService = userServiceMock.Object;
        }

        if (dataSetService is null)
        {

            var dataSetServiceMock = new Mock<IDataSetService>(MockBehavior.Strict);
            dataSetServiceMock
                .Setup(s => s.HasDraftVersion(It.IsAny<Guid>(), CancellationToken.None))
                .ReturnsAsync(false);
            dataSetService = dataSetServiceMock.Object;
        }

        if (featureFlags is null)
        {
            featureFlags = Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
            {
                EnableReplacementOfPublicApiDataSets = true
            });
        }

        return new DataSetValidator(
            contentDbContext!,
            userService!,
            dataSetService!,
            featureFlags!);
    }
}

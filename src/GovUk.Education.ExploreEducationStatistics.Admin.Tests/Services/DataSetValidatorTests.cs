#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Options;
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
    public async Task ValidateDataSet_ValidWithReplacement_ReturnsDataSetObject(string replacementFilename)
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
            ReplacingFile = _fixture
                .DefaultFile(FileType.Data)
                .WithFilename(replacementFilename)
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
            var result = await sut.ValidateDataSet(dataSetDto, performAutoReplacement: true);

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
    public async Task ValidateDataSet_ReplacementFilenameSameAsAnotherDataSetsFilename_ReturnsErrorDetails()
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
        
        // This data set has the same filename as the file that is being chosen as
        // a replacement for a different data set. 
        var otherExistingDataReleaseFile = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName("Other data set title")
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("other-data-filename.csv"))
            .Generate();
    
        var dataFile = await new DataSetFileBuilder().Build(FileType.Data);
        var metaFile = await new DataSetFileBuilder().Build(FileType.Metadata);
    
        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = releaseVersion.Id,
            Title = dataSetTitle,
            DataFile = dataFile,
            MetaFile = metaFile,
            // The filename chosen as the replacement for "toBeReplacedDataReleaseFile"
            // will clash with the filename for "otherExistingDataReleaseFile".
            ReplacingFile = otherExistingDataReleaseFile.File,
        };
    
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(
                toBeReplacedDataReleaseFile,
                toBeReplacedMetaReleaseFile,
                otherExistingDataReleaseFile);
            
            await context.SaveChangesAsync();
        }
    
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var sut = BuildService(context);
    
            // Act
            var result = await sut.ValidateDataSet(dataSetDto, performAutoReplacement: true);
    
            // Assert
            var errors = result.AssertLeft();
            Assert.Single(errors);
            Assert.Equal(ValidationMessages.FileNameNotUnique.Code, errors[0].Code);
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

    [Fact]
    public async Task ValidateDataSet_ReplacementHasApiDataSet_ReturnsErrorDetails()
    {
        // Arrange
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), };

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

        var dataSetDto = new DataSetDto
        {
            ReleaseVersionId = releaseVersion.Id,
            Title = dataSetTitle,
            DataFile = dataFile,
            MetaFile = metaFile,
            ReplacingFile = existingDataReleaseFile.File,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using var context = InMemoryContentDbContext(contentDbContextId);

        context.ReleaseFiles.AddRange(existingDataReleaseFile, existingMetaReleaseFile);
        await context.SaveChangesAsync();

        var featureFlagOptions = Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
        {
            EnableReplacementOfPublicApiDataSets = false
        });

        var sut = BuildService(
            context,
            featureFlagOptions);

        // Act
        var result = await sut.ValidateDataSet(dataSetDto, performAutoReplacement: true);

        // Assert
        var errors = result.AssertLeft();
        Assert.Single(errors);
        Assert.Equal(ValidationMessages.CannotReplaceDataSetWithApiDataSet.Code, errors[0].Code);
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
        Assert.Null(dataSetItem.ReplacingFile);
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
        Assert.Null(dataSetItem.ReplacingFile);
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
            Assert.NotNull(dataSetItem.ReplacingFile);
            Assert.Equal(toBeReplacedDataReleaseFile.File.Id, dataSetItem.ReplacingFile.Id);
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

        var toBeReplacedDataReleaseFiles = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName(dataSetTitle)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test-data.csv"))
            .Generate(2);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(toBeReplacedDataReleaseFiles);
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
        IOptions<FeatureFlagsOptions>? featureFlags = null)
    {
        return new DataSetValidator(
            contentDbContext!,
            featureFlags!);
    }
}

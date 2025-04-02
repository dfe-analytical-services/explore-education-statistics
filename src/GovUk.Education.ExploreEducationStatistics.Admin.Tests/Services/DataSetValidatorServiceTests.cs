#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Moq;
using System;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.ValidationProblemViewModelTestExtensions;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetValidatorServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task ValidateDataSet_Valid()
    {
        await using var context = InMemoryContentDbContext();
        var (service, fileTypeService) = BuildService(context);

        var dataFile = CreateFormFileMock("test.csv").Object;
        var metaFile = CreateFormFileMock("test.meta.csv").Object;

        fileTypeService
            .Setup(s => s.HasValidCsvFileMeta(dataFile.OpenReadStream()))
            .ReturnsAsync(true);
        fileTypeService
            .Setup(s => s.HasValidCsvFileMeta(metaFile.OpenReadStream()))
            .ReturnsAsync(true);

        var result = await service.ValidateDataSet(
            Guid.NewGuid(),
            "Data set title",
            dataFile.FileName,
            dataFile.OpenReadStream(),
            metaFile.FileName,
            metaFile.OpenReadStream());
        VerifyAllMocks(fileTypeService);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateDataSet_Valid_Replacement()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var toBeReplacedSubjectId = Guid.NewGuid();

        var toBeReplacedReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName("Data set title")
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test.csv")
                .WithSubjectId(toBeReplacedSubjectId))
            .Generate();

        var toBeReplacedMetaReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(Metadata)
                .WithFilename("test.meta.csv")
                .WithSubjectId(toBeReplacedSubjectId))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(toBeReplacedReleaseFile, toBeReplacedMetaReleaseFile);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var (service, fileTypeService) = BuildService(context);

            // NOTE: Not necessary, but in this test case new files have same name as the files they're replacing
            var dataFile = CreateFormFileMock(toBeReplacedReleaseFile.File.Filename).Object;
            var metaFile = CreateFormFileMock(toBeReplacedMetaReleaseFile.File.Filename).Object;

            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var result = await service.ValidateDataSet(
                releaseVersion.Id,
                toBeReplacedReleaseFile.Name,
                dataFile.FileName,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.OpenReadStream(),
                toBeReplacedReleaseFile.File);

            VerifyAllMocks(fileTypeService);

            Assert.Empty(result);
        }
    }

    [Theory]
    [InlineData("test/", true)]
    [InlineData("test&", true)]
    [InlineData("test\0", true)]
    [InlineData("test$", false)]
    [InlineData("", false)]
    public void ContainsSpecialChars_WithValue_ReturnsExpectedResult(
        string dataSetTitle,
        bool expectedResult)
    {
        // Act
        var result = DataSetValidatorService.ContainsSpecialChars(dataSetTitle);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task ValidateDataSet_DataSetTitleShouldBeUnique()
    {
        var releaseFile = new ReleaseFile
        {
            Name = "Used data set title",
            ReleaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), },
            File = new File { Type = FileType.Data, },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.Add(releaseFile);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var (service, fileTypeService) = BuildService(context);

            var dataFile = CreateFormFileMock("test.csv").Object;
            var metaFile = CreateFormFileMock("test.meta.csv").Object;

            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var errors = await service.ValidateDataSet(
                releaseFile.ReleaseVersionId,
                "Used data set title",
                dataFile.FileName,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.OpenReadStream());
            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                ValidationMessages.GenerateErrorDataSetTitleShouldBeUnique("Used data set title"),
            ]);
        }
    }

    [Fact]
    public async Task ValidateDataSet_DataAndMetaFilesCannotHaveSameName()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var context = InMemoryContentDbContext(contentDbContextId);
        var (service, fileTypeService) = BuildService(context);

        var dataFile = CreateFormFileMock("samefilename.meta.csv").Object;
        var metaFile = CreateFormFileMock("samefilename.meta.csv").Object;

        fileTypeService
            .Setup(s => s.HasValidCsvFileMeta(dataFile.OpenReadStream()))
            .ReturnsAsync(true);
        fileTypeService
            .Setup(s => s.HasValidCsvFileMeta(metaFile.OpenReadStream()))
            .ReturnsAsync(true);

        var errors = await service.ValidateDataSet(
            Guid.NewGuid(),
            "Data file title",
            dataFile.FileName,
            dataFile.OpenReadStream(),
            metaFile.FileName,
            metaFile.OpenReadStream());
        VerifyAllMocks(fileTypeService);

        AssertHasErrors(errors, [
            new ErrorViewModel
            {
                Code = ValidationMessages.DataAndMetaFilesCannotHaveSameName.Code,
                Message = ValidationMessages.DataAndMetaFilesCannotHaveSameName.Message,
            },
        ]);
    }

    [Theory]
    [InlineData("test ")]
    [InlineData("test/")]
    [InlineData("test&")]
    [InlineData("test\0")]
    public async Task ValidateDataSet_DataFileNamesCannotContainSpacesOrSpecialCharacters(string dataFileName)
    {
        await using var context = InMemoryContentDbContext();
        var (service, fileTypeService) = BuildService(context);

        var dataFile = CreateFormFileMock($"{dataFileName}.csv").Object;
        var metaFile = CreateFormFileMock($"{dataFileName}.meta.csv").Object;

        fileTypeService
            .Setup(s => s.HasValidCsvFileMeta(dataFile.OpenReadStream()))
            .ReturnsAsync(true);
        fileTypeService
            .Setup(s => s.HasValidCsvFileMeta(metaFile.OpenReadStream()))
            .ReturnsAsync(true);

        var errors = await service.ValidateDataSet(
            Guid.NewGuid(),
            "Data title",
            dataFile.FileName,
            dataFile.OpenReadStream(),
            metaFile.FileName,
            metaFile.OpenReadStream());
        VerifyAllMocks(fileTypeService);

        AssertHasErrors(errors, [
            ValidationMessages.GenerateErrorFilenameCannotContainSpacesOrSpecialCharacters($"{dataFileName}.csv"),
            ValidationMessages.GenerateErrorFilenameCannotContainSpacesOrSpecialCharacters($"{dataFileName}.meta.csv"),
        ]);
    }

    [Fact]
    public async Task ValidateDataSet_DataFileNamesNotUnique()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion()
            .Generate();

        var releaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test.csv"))
            .Generate();

        var releaseFileMeta = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(Metadata)
                .WithFilename("test.meta.csv"))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(releaseFile, releaseFileMeta);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            var (service, fileTypeService) = BuildService(context);

            var dataFile = CreateFormFileMock("test.csv").Object;
            var metaFile = CreateFormFileMock("test.meta.csv").Object;

            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var errors = await service.ValidateDataSet(
                releaseVersion.Id,
                "Data set title",
                dataFile.FileName,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.OpenReadStream());
            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                ValidationMessages.GenerateErrorFilenameNotUnique(
                    releaseFile.File.Filename, releaseFile.File.Type),
                // NOTE: We allow duplicate meta file names - meta files aren't included in publicly downloadable
                // zips, so meta files won't be included in the same directory by name and thereby cannot clash.
            ]);
        }
    }

    [Fact]
    public async Task ValidateDataSet_DataAndMetaFilesShouldBeValidCsvFiles()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var context = InMemoryContentDbContext(contentDbContextId);
        var (service, fileTypeService) = BuildService(context);

        var dataFile = CreateFormFileMock("test.csv").Object;
        var metaFile = CreateFormFileMock("test.meta.csv").Object;

        fileTypeService
            .Setup(s => s.HasValidCsvFileMeta(dataFile.OpenReadStream()))
            .ReturnsAsync(false);
        fileTypeService
            .Setup(s => s.HasValidCsvFileMeta(metaFile.OpenReadStream()))
            .ReturnsAsync(false);

        var errors = await service.ValidateDataSet(
            Guid.NewGuid(),
            "Data set title",
            dataFile.FileName,
            dataFile.OpenReadStream(),
            metaFile.FileName,
            metaFile.OpenReadStream());
        VerifyAllMocks(fileTypeService);

        AssertHasErrors(errors, [
            ValidationMessages.GenerateErrorMustBeCsvFile(dataFile.FileName),
            ValidationMessages.GenerateErrorMustBeCsvFile(metaFile.FileName),
        ]);
    }

    [Fact]
    public async Task ValidateDataSet_Replacement_FilesNamesNotUnique()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion()
            .Generate();

        var otherSubjectId = Guid.NewGuid();

        var otherReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("usedfilename.csv")
                .WithSubjectId(otherSubjectId))
            .Generate();

        var otherMetaReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(Metadata)
                .WithFilename("usedfilename.meta.csv")
                .WithSubjectId(otherSubjectId))
            .Generate();

        var toBeReplacedSubjectId = Guid.NewGuid();

        var toBeReplacedReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName("Data set title")
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test.csv")
                .WithSubjectId(toBeReplacedSubjectId))
            .Generate();

        var toBeReplacedMetaReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(Metadata)
                .WithFilename("test.meta.csv")
                .WithSubjectId(toBeReplacedSubjectId))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(
                otherReleaseFile, otherMetaReleaseFile,
                toBeReplacedReleaseFile, toBeReplacedMetaReleaseFile);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contentDbContextId))
        {

            var (service, fileTypeService) = BuildService(context);

            // New files have different filenames to files being replaced, but same filenames as an existing subject
            var dataFile = CreateFormFileMock("usedfilename.csv").Object;
            var metaFile = CreateFormFileMock("usedfilename.meta.csv").Object;

            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var errors = await service.ValidateDataSet(
                releaseVersion.Id,
                toBeReplacedReleaseFile.Name!,
                "usedfilename.csv",
                dataFile.OpenReadStream(),
                "usedfilename.meta.csv",
                metaFile.OpenReadStream(),
                toBeReplacedReleaseFile.File);

            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                ValidationMessages.GenerateErrorFilenameNotUnique("usedfilename.csv", FileType.Data),
                // NOTE: We allow duplicate meta file names - meta files aren't included in publicly downloadable
                // zips, so meta files won't be included in the same directory by name and thereby cannot clash.
            ]);
        }
    }

    [Fact]
    public async Task ValidateDataSet_Replacement_HasApiDataSet()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid(), };

        var toBeReplacedSubjectId = Guid.NewGuid();

        var toBeReplacedReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithName("Data set title")
            .WithFile(_fixture.DefaultFile()
                .WithType(FileType.Data)
                .WithFilename("test.csv")
                .WithSubjectId(toBeReplacedSubjectId))
            .WithPublicApiDataSetId(Guid.NewGuid())
            .Generate();

        var toBeReplacedMetaReleaseFile = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFile(_fixture.DefaultFile()
                .WithType(Metadata)
                .WithFilename("test.meta.csv")
                .WithSubjectId(toBeReplacedSubjectId))
            .Generate();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            context.ReleaseFiles.AddRange(toBeReplacedReleaseFile, toBeReplacedMetaReleaseFile);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            var (service, fileTypeService) = BuildService(context);

            var dataFile = CreateFormFileMock(toBeReplacedReleaseFile.File.Filename).Object;
            var metaFile = CreateFormFileMock(toBeReplacedMetaReleaseFile.File.Filename).Object;

            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(dataFile.OpenReadStream()))
                .ReturnsAsync(true);
            fileTypeService
                .Setup(s => s.HasValidCsvFileMeta(metaFile.OpenReadStream()))
                .ReturnsAsync(true);

            var errors = await service.ValidateDataSet(
                releaseVersion.Id,
                toBeReplacedReleaseFile.Name,
                dataFile.FileName,
                dataFile.OpenReadStream(),
                metaFile.FileName,
                metaFile.OpenReadStream(),
                toBeReplacedReleaseFile.File);

            VerifyAllMocks(fileTypeService);

            AssertHasErrors(errors, [
                ValidationMessages.GenerateErrorCannotReplaceDataSetWithApiDataSet("Data set title"),
            ]);
        }
    }

    [Fact]
    public async Task ValidateFileForUpload_FileCannotBeEmpty()
    {
        var file = CreateFormFileMock("test.csv", 0).Object;

        var (service, _) = BuildService();
        var result = await service.ValidateFileForUpload(file, Ancillary);
        result.AssertBadRequest(FileCannotBeEmpty);
    }

    [Fact]
    public async Task ValidateFileForUpload_ExceptionThrownForDataFileType()
    {
        var file = CreateFormFileMock("test.csv").Object;

        var (service, _) = BuildService();
        await Assert.ThrowsAsync<ArgumentException>(() => service.ValidateFileForUpload(file, FileType.Data));
    }

    [Fact]
    public async Task ValidateFileForUpload_FileTypeIsValid()
    {
        var file = CreateFormFileMock("test.csv").Object;

        var (service, fileTypeService) = BuildService();

        fileTypeService
            .Setup(s => s.HasMatchingMimeType(file, AllowedAncillaryFileTypes))
            .ReturnsAsync(() => true);

        var result = await service.ValidateFileForUpload(file, Ancillary);
        VerifyAllMocks(fileTypeService);

        result.AssertRight();
    }

    [Fact]
    public async Task ValidateFileForUpload_FileTypeIsInvalid()
    {
        var file = CreateFormFileMock("test.csv").Object;

        var (service, fileTypeService) = BuildService();

        fileTypeService
            .Setup(s => s.HasMatchingMimeType(file, AllowedAncillaryFileTypes))
            .ReturnsAsync(() => false);

        var result = await service.ValidateFileForUpload(file, Ancillary);
        VerifyAllMocks(fileTypeService);

        result.AssertBadRequest(FileTypeInvalid);
    }

    private static (
        DataSetValidatorService,
        Mock<IFileTypeService> fileTypeService
        )
        BuildService(
            ContentDbContext? contentDbContext = null,
            IFileTypeService? fileTypeService = null)
    {
        var fileTypeServiceMock = new Mock<IFileTypeService>(Strict);

        var service = new DataSetValidatorService(
            fileTypeService ?? fileTypeServiceMock.Object,
            contentDbContext!);

        return (service, fileTypeServiceMock);
    }
}

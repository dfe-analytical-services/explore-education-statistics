#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class FileValidatorServiceTests
{
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

        fileTypeService.Setup(s => s.HasMatchingMimeType(file, AllowedAncillaryFileTypes)).ReturnsAsync(() => true);

        var result = await service.ValidateFileForUpload(file, Ancillary);
        VerifyAllMocks(fileTypeService);

        result.AssertRight();
    }

    [Fact]
    public async Task ValidateFileForUpload_FileTypeIsInvalid()
    {
        var file = CreateFormFileMock("test.csv").Object;

        var (service, fileTypeService) = BuildService();

        fileTypeService.Setup(s => s.HasMatchingMimeType(file, AllowedAncillaryFileTypes)).ReturnsAsync(() => false);

        var result = await service.ValidateFileForUpload(file, Ancillary);
        VerifyAllMocks(fileTypeService);

        result.AssertBadRequest(FileTypeInvalid);
    }

    private static (FileValidatorService, Mock<IFileTypeService> fileTypeService) BuildService(
        Mock<IFileTypeService>? fileTypeService = null
    )
    {
        fileTypeService ??= new Mock<IFileTypeService>(Strict);
        var service = new FileValidatorService(fileTypeService.Object);

        return (service, fileTypeService);
    }
}

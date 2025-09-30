#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class FileValidatorService(IFileTypeService fileTypeService) : IFileValidatorService
{
    private const int MaxFileSize = int.MaxValue; // 2GB

    public async Task<Either<ActionResult, Unit>> ValidateFileForUpload(
        IFormFile file,
        FileType type
    )
    {
        if (type is FileType.Data or Metadata)
        {
            throw new ArgumentException(
                "Cannot use generic function to validate data file",
                nameof(type)
            );
        }

        if (file.Length == 0)
        {
            return ValidationActionResult(FileCannotBeEmpty);
        }

        if (file.Length > MaxFileSize)
        {
            return ValidationActionResult(FileSizeLimitExceeded);
        }

        if (!await fileTypeService.HasMatchingMimeType(file, AllowedMimeTypesByFileType[type]))
        {
            return ValidationActionResult(FileTypeInvalid);
        }

        return Unit.Instance;
    }
}

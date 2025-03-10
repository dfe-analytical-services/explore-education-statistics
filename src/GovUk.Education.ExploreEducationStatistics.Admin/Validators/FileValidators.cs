using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class FileValidators
{
    private const int MaxFilenameSize = 150;

    public static IRuleBuilderOptions<T, IFormFile> MustBeValidCsvFile<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
                .WithMessage(ValidationMessages.FileIsNull)
            .Must(file => file.FileName.Length < MaxFilenameSize)
                .WithMessage(ValidationMessages.FilenameTooLong)
            .Must(file => file.Length > 0)
                .WithMessage(ValidationMessages.FileSizeMustNotBeZero)
            .Must(file => file.FileName.ToLower().EndsWith(".csv"))
                .WithMessage(ValidationMessages.FilenameMustEndDotCsv);
    }

    public static IRuleBuilderOptions<T, IFormFile> MustBeValidZipFile<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
                .WithMessage(ValidationMessages.FileIsNull)
            .Must(file => file.FileName.Length < MaxFilenameSize)
                .WithMessage(ValidationMessages.FilenameTooLong)
            .Must(file => file.Length > 0)
                .WithMessage(ValidationMessages.FileSizeMustNotBeZero)
            .Must(file => file.FileName.ToLower().EndsWith(".zip"))
                .WithMessage(ValidationMessages.ZipFilenameMustEndDotZip);
    }
}

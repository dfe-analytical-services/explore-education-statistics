using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class FileValidators
{
    public static IRuleBuilderOptions<T, IFormFile> MustBeValidFile<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
                .WithMessage(ValidationMessages.FileIsNull)
            .MustHaveAValidFileName()
            .Must(file => file.Length > 0)
                .WithMessage(ValidationMessages.FileSizeMustNotBeZero, "{PropertyName}");
    }

    public static IRuleBuilderOptions<T, IFormFile> MustBeValidCsvFile<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .MustBeValidFile()
            .Must(file => file.FileName.ToLower().EndsWith(".csv"))
                .WithMessage(ValidationMessages.FileNameMustEndDotCsv, "{PropertyName}");
    }

    public static IRuleBuilderOptions<T, IFormFile> MustBeValidZipFile<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .MustBeValidFile()
            .Must(file => file.FileName.ToLower().EndsWith(".zip"))
                .WithMessage(ValidationMessages.ZipFileNameMustEndDotZip, "{PropertyName}");
    }

    public static IRuleBuilderOptions<T, IFormFile> MustHaveAValidFileName<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            // TODO: Add "{FileName}" placeholder, {PropertyName}/{PropertyValue} output unsuitable
            .Must(file => FileNameValidators.MeetsLengthRequirements(file.FileName))
                .WithMessage(ValidationMessages.FilenameTooLong, "{PropertyValue}", FileNameValidators.MaxFileNameSize.ToString())
            .Must(file => FileNameValidators.DoesNotContainSpaces(file.FileName))
                .WithMessage(ValidationMessages.FileNameCannotContainSpaces, "{PropertyValue}")
            .Must(file => FileNameValidators.DoesNotContainSpecialChars(file.FileName))
                .WithMessage(ValidationMessages.FileNameCannotContainSpecialCharacters, "{PropertyValue}");
    }
}

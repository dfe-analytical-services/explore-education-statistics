using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class FileValidators
{
    public static IRuleBuilderOptions<T, IFormFile> MustBeValidFile<T>(this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .WithMessage(ValidationMessages.FileIsNull)
            .MustHaveAValidFileName()
            .Must(
                (_, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", file.FileName);
                    return file.Length > 0;
                }
            )
            .WithMessage(ValidationMessages.FileSizeMustNotBeZero, "{FileName}");
    }

    public static IRuleBuilderOptions<T, IFormFile> MustBeValidCsvFile<T>(this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .MustBeValidFile()
            .Must(
                (_, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", file.FileName);
                    return file.FileName.ToLower().EndsWith(Constants.DataSet.DataFileExtension);
                }
            )
            .WithMessage(ValidationMessages.FileNameMustEndDotCsv, "{FileName}", Constants.DataSet.DataFileExtension);
    }

    public static IRuleBuilderOptions<T, IFormFile> MustBeValidZipFile<T>(this IRuleBuilder<T, IFormFile> ruleBuilder)
    {
        return ruleBuilder
            .MustBeValidFile()
            .Must(
                (_, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", file.FileName);
                    return file.FileName.ToLower().EndsWith(".zip");
                }
            )
            .WithMessage(ValidationMessages.ZipFileNameMustEndDotZip, "{FileName}");
    }

    public static IRuleBuilderOptions<T, IFormFile> MustHaveAValidFileName<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder
    )
    {
        return ruleBuilder
            .Must(
                (_, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", file.FileName);
                    return FileNameValidators.MeetsLengthRequirements(file.FileName);
                }
            )
            .WithMessage(
                ValidationMessages.FileNameLengthInvalid,
                "{FileName}",
                FileNameValidators.MaxFileNameSize.ToString()
            )
            .Must(
                (_, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", file.FileName);
                    return !FileNameValidators.ContainsSpaces(file.FileName);
                }
            )
            .WithMessage(ValidationMessages.FileNameCannotContainSpaces, "{FileName}")
            .Must(
                (_, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", file.FileName);
                    return !FileNameValidators.ContainsSpecialChars(file.FileName);
                }
            )
            .WithMessage(ValidationMessages.FileNameCannotContainSpecialCharacters, "{FileName}");
    }
}

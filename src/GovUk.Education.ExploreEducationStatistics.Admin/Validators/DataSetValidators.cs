using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class DataSetValidators
{
    public static IRuleBuilderOptions<T, FileDto> MustHaveAValidFileName<T>(
        this IRuleBuilder<T, FileDto> ruleBuilder)
    {
        return ruleBuilder
            .Must((_, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return FileNameValidators.MeetsLengthRequirements(dto.FileName);
            })
                .WithMessage(ValidationMessages.FileNameLengthInvalid, "{FileName}", FileNameValidators.MaxFileNameSize.ToString())
            .Must((_, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return !FileNameValidators.ContainsSpaces(dto.FileName);
            })
                .WithMessage(ValidationMessages.FileNameCannotContainSpaces, "{FileName}")
            .Must((_, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return !FileNameValidators.ContainsSpecialChars(dto.FileName);
            })
                .WithMessage(ValidationMessages.FileNameCannotContainSpecialCharacters, "{FileName}");
    }

    public static IRuleBuilderOptions<T, FileDto> MustNotBeAnEmptyFile<T>(
        this IRuleBuilder<T, FileDto> ruleBuilder)
    {
        return ruleBuilder
            .Must((_, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return dto.FileStream.Length > 0;
            })
                .WithMessage(ValidationMessages.FileSizeMustNotBeZero, "{FileName}");
    }

    public static IRuleBuilderOptions<T, FileDto> MustBeValidFile<T>(
        this IRuleBuilder<T, FileDto> ruleBuilder)
    {
        return ruleBuilder
            .MustNotBeAnEmptyFile()
            .MustHaveAValidFileName()
            .When(dto => dto is not null);
    }

    public static IRuleBuilderOptions<T, FileDto> MustBeValidDataFile<T>(
        this IRuleBuilder<T, FileDto> ruleBuilder)
    {
        return ruleBuilder
            .MustBeValidFile()
            .Must((_, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return dto.FileName.ToLower().EndsWith(Constants.DataSet.DataFileExtension);
            })
                .WithMessage(ValidationMessages.FileNameMustEndDotCsv, "{FileName}", Constants.DataSet.DataFileExtension)
            .When(dto => dto is not null);
    }

    public static IRuleBuilderOptions<T, FileDto> MustBeValidMetaFile<T>(
        this IRuleBuilder<T, FileDto> ruleBuilder)
    {
        return ruleBuilder
            .MustBeValidFile()
            .Must((_, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return dto.FileName.ToLower().EndsWith(Constants.DataSet.MetaFileExtension);
            })
                .WithMessage(ValidationMessages.MetaFileNameMustEndDotMetaDotCsv, "{FileName}", Constants.DataSet.MetaFileExtension)
            .When(dto => dto is not null);
    }
}

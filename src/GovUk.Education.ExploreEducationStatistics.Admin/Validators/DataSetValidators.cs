using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class DataSetValidators
{
    public static IRuleBuilderOptions<T, DataSetFileDto> MustHaveAValidFileName<T>(
        this IRuleBuilder<T, DataSetFileDto> ruleBuilder)
    {
        return ruleBuilder
            .Must((root, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return FileNameValidators.MeetsLengthRequirements(dto.FileName);
            })
                .WithMessage(ValidationMessages.FileNameLengthInvalid, "{FileName}", FileNameValidators.MaxFileNameSize.ToString())
            .Must((root, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return FileNameValidators.DoesNotContainSpaces(dto.FileName);
            })
                .WithMessage(ValidationMessages.FileNameCannotContainSpaces, "{FileName}")
            .Must((root, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return FileNameValidators.DoesNotContainSpecialChars(dto.FileName);
            })
                .WithMessage(ValidationMessages.FileNameCannotContainSpecialCharacters, "{FileName}");
    }

    public static IRuleBuilderOptions<T, DataSetFileDto> MustNotBeAnEmptyFile<T>(
        this IRuleBuilder<T, DataSetFileDto> ruleBuilder)
    {
        return ruleBuilder
            .Must((root, dto, context) =>
            {
                context.MessageFormatter.AppendArgument("FileName", dto.FileName);
                return dto.FileStream.Length > 0;
            })
                .WithMessage(ValidationMessages.FileSizeMustNotBeZero, "{FileName}");
    }

    public static IRuleBuilderOptions<T, DataSetFileDto> MustBeValidFile<T>(
        this IRuleBuilder<T, DataSetFileDto> ruleBuilder)
    {
        return ruleBuilder
            .MustNotBeAnEmptyFile()
            .MustHaveAValidFileName()
            .When(dto => dto is not null);
    }

    public static IRuleBuilderOptions<T, DataSetFileDto> MustBeValidDataFile<T>(
        this IRuleBuilder<T, DataSetFileDto> ruleBuilder)
    {
        return ruleBuilder
            .MustBeValidFile()
            .Must(dto => dto.FileName.ToLower().EndsWith(Constants.DataSet.DataFileExtension))
                .WithMessage(ValidationMessages.FileNameMustEndDotCsv, "{PropertyValue}", Constants.DataSet.DataFileExtension)
            .When(dto => dto is not null);
    }

    public static IRuleBuilderOptions<T, DataSetFileDto> MustBeValidMetaFile<T>(
        this IRuleBuilder<T, DataSetFileDto> ruleBuilder)
    {
        return ruleBuilder
            .MustBeValidFile()
            .Must(dto => dto.FileName.ToLower().EndsWith(Constants.DataSet.MetaFileExtension))
                .WithMessage(ValidationMessages.MetaFileNameMustEndDotMetaDotCsv, "{PropertyValue}", Constants.DataSet.MetaFileExtension)
            .When(dto => dto is not null);
    }
}

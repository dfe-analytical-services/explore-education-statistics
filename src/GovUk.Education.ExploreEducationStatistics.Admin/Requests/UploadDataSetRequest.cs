#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

[ValidateNever]
public record UploadDataSetRequest
{
    public Guid ReleaseVersionId { get; init; }

    public string Title { get; init; } = string.Empty;

    public IFormFile DataFile { get; init; } = null!;

    public IFormFile MetaFile { get; init; } = null!;

    public class Validator : AbstractValidator<UploadDataSetRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseVersionId)
                .NotEmpty();

            RuleFor(request => request.Title)
                .NotEmpty()
                    .WithMessage(ValidationMessages.DataSetTitleCannotBeEmpty)
                .MaximumLength(120)
                    .WithMessage(ValidationMessages.DataSetTitleTooLong, "{PropertyValue}", "{MaxLength}");

            RuleFor(request => request.DataFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidCsvFile()
                .Must((request, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", request.DataFile.FileName);

                    var fileName = file.FileName.ToLower();

                    return
                        fileName.EndsWith(Constants.DataSet.DataFileExtension) &&
                        !fileName.EndsWith(Constants.DataSet.MetaFileExtension);
                })
                    .WithMessage(ValidationMessages.FileNameMustEndDotCsv, "{FileName}", Constants.DataSet.DataFileExtension);

            RuleFor(request => request.MetaFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidCsvFile()
                .Must((request, file, context) =>
                {
                    context.MessageFormatter.AppendArgument("FileName", request.MetaFile.FileName);
                    return file.FileName.ToLower().EndsWith(Constants.DataSet.MetaFileExtension);
                })
                    .WithMessage(ValidationMessages.MetaFileNameMustEndDotMetaDotCsv, "{FileName}", Constants.DataSet.MetaFileExtension);
        }
    }
}

[ValidateNever]
public record UploadDataSetAsZipRequest
{
    public Guid ReleaseVersionId { get; init; }

    public string Title { get; init; } = string.Empty;

    public IFormFile ZipFile { get; init; } = null!;

    public class Validator : AbstractValidator<UploadDataSetAsZipRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseVersionId)
                .NotEmpty();

            RuleFor(request => request.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                    .WithMessage(ValidationMessages.DataSetTitleCannotBeEmpty)
                .MaximumLength(120)
                    .WithMessage(ValidationMessages.DataSetTitleTooLong, "{PropertyValue}", "{MaxLength}")
                .Must(title => !FileNameValidators.ContainsSpecialChars(title))
                    .WithMessage(ValidationMessages.DataSetTitleShouldNotContainSpecialCharacters, "{PropertyValue}");

            RuleFor(request => request.ZipFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidZipFile();
        }
    }
}

[ValidateNever]
public record UploadDataSetAsBulkZipRequest
{
    public Guid ReleaseVersionId { get; init; }

    public IFormFile ZipFile { get; init; } = null!;

    public class Validator : AbstractValidator<UploadDataSetAsBulkZipRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseVersionId)
                .NotEmpty();

            RuleFor(request => request.ZipFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidZipFile();
        }
    }
}

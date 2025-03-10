#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record UploadDataSetRequest
{
    public Guid ReleaseVersionId { get; init; }

    public Guid? ReplacingFileId { get; init; }

    public string DataSetTitle { get; init; } = string.Empty;

    public IFormFile DataFile { get; init; } = null!;

    public IFormFile MetaFile { get; init; } = null!;

    public class Validator : AbstractValidator<UploadDataSetRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseVersionId)
                .NotEmpty();

            RuleFor(request => request.DataSetTitle)
                .NotEmpty()
                    .WithMessage(ValidationMessages.DataSetTitleCannotBeEmpty)
                .MaximumLength(120)
                    .WithMessage(ValidationMessages.DataSetTitleTooLong);

            RuleFor(request => request.DataFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidCsvFile();

            RuleFor(request => request.MetaFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidCsvFile()
                .Must(file => file.FileName.ToLower().EndsWith(".meta.csv"))
                    .WithMessage(ValidationMessages.MetaFilenameMustEndDotMetaDotCsv);
        }
    }
}

[ValidateNever]
public record UploadDataSetAsZipRequest
{
    public Guid ReleaseVersionId { get; init; }

    public Guid? ReplacingFileId { get; init; }

    public string DataSetTitle { get; init; } = string.Empty;

    public IFormFile ZipFile { get; init; } = null!;

    public class Validator : AbstractValidator<UploadDataSetAsZipRequest>
    {
        public Validator()
        {
            RuleFor(request => request.ReleaseVersionId)
                .NotEmpty();

            RuleFor(request => request.DataSetTitle)
                .NotEmpty()
                    .WithMessage(ValidationMessages.DataSetTitleCannotBeEmpty)
                .MaximumLength(120)
                    .WithMessage(ValidationMessages.DataSetTitleTooLong);

            RuleFor(request => request.ZipFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidZipFile();
        }
    }
}

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

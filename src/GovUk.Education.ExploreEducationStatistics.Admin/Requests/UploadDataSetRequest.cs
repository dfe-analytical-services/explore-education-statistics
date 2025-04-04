#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

[ValidateNever]
public record UploadDataSetRequest
{
    public Guid ReleaseVersionId { get; init; }

    public Guid? ReplacingFileId { get; init; }

    public string Title { get; init; } = string.Empty;

    public IFormFile DataFile { get; init; } = null!;

    public IFormFile MetaFile { get; init; } = null!;

    public class Validator : AbstractValidator<UploadDataSetRequest>
    {
        //private readonly IFileTypeService _fileTypeService;

        public Validator(/*IFileTypeService fileTypeService*/)
        {
            //_fileTypeService = fileTypeService;

            RuleFor(request => request.ReleaseVersionId)
                .NotEmpty();

            RuleFor(request => request.Title)
                .NotEmpty()
                    .WithMessage(ValidationMessages.DataSetTitleCannotBeEmpty)
                .MaximumLength(120)
                    .WithMessage(ValidationMessages.DataSetTitleTooLong, "{PropertyValue}", "{MaxLength}");

            RuleFor(request => request.DataFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidCsvFile();
            //.MustAsync(async (file, cancellationToken) => await _fileTypeService.HasValidCsvFileMeta(file))
            //    .WithMessage(d => string.Format(ValidationMessages.MustBeCsvFile.Message, d.DataFile.FileName));

            RuleFor(request => request.MetaFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidCsvFile()
                .Must(file => file.FileName.ToLower().EndsWith(".meta.csv"))
                    .WithMessage(ValidationMessages.MetaFilenameMustEndDotMetaDotCsv, "{PropertyValue}");
            //.MustAsync(async (file, cancellationToken) => await _fileTypeService.HasValidCsvFileMeta(file))
            //    .WithMessage(d => string.Format(ValidationMessages.MustBeCsvFile.Message, d.MetaFile.FileName));
        }
    }
}

[ValidateNever]
public record UploadDataSetAsZipRequest
{
    public Guid ReleaseVersionId { get; init; }

    public Guid? ReplacingFileId { get; init; }

    public string Title { get; init; } = string.Empty;

    public IFormFile ZipFile { get; init; } = null!;

    public class Validator : AbstractValidator<UploadDataSetAsZipRequest>
    {
        //private readonly IFileTypeService _fileTypeService;

        public Validator(/*IFileTypeService fileTypeService*/)
        {
            //_fileTypeService = fileTypeService;

            RuleFor(request => request.ReleaseVersionId)
                .NotEmpty();

            RuleFor(request => request.Title)
                .NotEmpty()
                    .WithMessage(ValidationMessages.DataSetTitleCannotBeEmpty)
                .MaximumLength(120)
                    .WithMessage(ValidationMessages.DataSetTitleTooLong, "{PropertyValue}", "{MaxLength}")
                .Must(FileNameValidators.DoesNotContainSpecialChars)
                    .WithMessage(ValidationMessages.DataSetTitleShouldNotContainSpecialCharacters, "{PropertyValue}");

            RuleFor(request => request.ZipFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidZipFile();
            //.MustAsync(async (file, cancellationToken) => await _fileTypeService.HasValidZipFileMeta(file))
            //    .WithMessage(d => string.Format(ValidationMessages.MustBeZipFile.Message, d.ZipFile.FileName));
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
        //private readonly IFileTypeService _fileTypeService;

        public Validator(/*IFileTypeService fileTypeService*/)
        {
            //_fileTypeService = fileTypeService;

            RuleFor(request => request.ReleaseVersionId)
                .NotEmpty();

            RuleFor(request => request.ZipFile)
                .Cascade(CascadeMode.Stop)
                .MustBeValidZipFile();
            //.MustAsync(async (file, cancellationToken) => await _fileTypeService.HasValidZipFileMeta(file))
            //    .WithMessage(d => string.Format(ValidationMessages.MustBeZipFile.Message, d.ZipFile.FileName));
        }
    }
}

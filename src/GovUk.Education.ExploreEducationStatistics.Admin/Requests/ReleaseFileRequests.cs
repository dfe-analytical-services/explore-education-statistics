#nullable enable
using FluentValidation;
using Microsoft.AspNetCore.Http;
using static GovUk.Education.ExploreEducationStatistics.Common.Constants.ValidationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseDataFileUpdateRequest
{
    public string? Title { get; set; }

    public string? Summary { get; set; }

    public class Validator : AbstractValidator<ReleaseDataFileUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Title)
                .MaximumLength(SubjectTitleMaxLength)
                .WithMessage(SubjectTitleMaxLengthMessage);
        }
    }
}

public record ReleaseAncillaryFileUploadRequest
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public IFormFile File { get; set; } = null!;

    public class Validator : AbstractValidator<ReleaseAncillaryFileUploadRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Title)
                .NotEmpty()
                .MaximumLength(TitleMaxLength)
                .WithMessage(TitleMaxLengthMessage);

            RuleFor(request => request.Summary)
                .NotEmpty()
                .MaximumLength(SummaryMaxLength)
                .WithMessage(SummaryMaxLengthMessage);

            RuleFor(request => request.File)
                .NotEmpty();
        }
    }
}

public record ReleaseAncillaryFileUpdateRequest
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public IFormFile? File { get; set; }

    public class Validator : AbstractValidator<ReleaseAncillaryFileUpdateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Title)
                .NotEmpty()
                .MaximumLength(TitleMaxLength)
                .WithMessage(TitleMaxLengthMessage);

            RuleFor(request => request.Summary)
                .NotEmpty()
                .MaximumLength(SummaryMaxLength)
                .WithMessage(SummaryMaxLengthMessage);
        }
    }
}

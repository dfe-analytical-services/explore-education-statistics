#nullable enable
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

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
                .MaximumLength(120)
                .WithMessage("Subject title must be 120 characters or less");
        }
    }
}

public record ReleaseAncillaryFileUploadRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required] public string Summary { get; set; } = string.Empty;

    [Required]
    public IFormFile File { get; set; } = null!;

    public class Validator : AbstractValidator<ReleaseAncillaryFileUploadRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Title)
                .MaximumLength(120)
                .WithMessage("Title must be 120 characters or less");

            RuleFor(request => request.Summary)
                .MaximumLength(250)
                .WithMessage("Summary must be 250 characters or less");
        }
    }
}

public record ReleaseAncillaryFileUpdateRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Summary { get; set; } = string.Empty;

    public IFormFile? File { get; set; }
}

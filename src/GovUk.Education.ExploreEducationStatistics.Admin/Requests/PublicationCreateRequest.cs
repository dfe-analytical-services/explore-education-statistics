#nullable enable
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record PublicationCreateRequest
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    [Required] public Guid ThemeId { get; set; }

    [Required] public ContactSaveRequest Contact { get; set; } = null!;

    private string _slug = string.Empty;

    public string Slug
    {
        get => string.IsNullOrEmpty(_slug) ? NamingUtils.SlugFromTitle(Title) : _slug;
        set => _slug = value;
    }

    public Guid? SupersededById { get; set; }

    public class Validator : AbstractValidator<PublicationCreateRequest>
    {
        public Validator()
        {
            RuleFor(request => request.Title)
                .NotEmpty()
                .MaximumLength(65);

            RuleFor(request => request.Summary)
                .NotEmpty()
                .MaximumLength(160);
        }
    }
}

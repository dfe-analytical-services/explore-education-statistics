#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.ComponentModel.DataAnnotations;
using static System.String;
using FluentValidation;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record PublicationSaveRequest
{
    public string Title { get; set; } = Empty;

    public string Summary { get; set; } = Empty;

    [Required] public Guid ThemeId { get; set; }

    private string _slug = Empty;

    public string Slug
    {
        get => IsNullOrEmpty(_slug) ? NamingUtils.SlugFromTitle(Title) : _slug;
        set => _slug = value;
    }

    public Guid? SupersededById { get; set; }

    public class Validator : AbstractValidator<PublicationSaveRequest>
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

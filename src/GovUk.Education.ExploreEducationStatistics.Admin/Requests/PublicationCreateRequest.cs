#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record PublicationCreateRequest
{
    [Required] public string Title { get; set; } = string.Empty;

    [Required, MaxLength(160)] public string Summary { get; set; } = string.Empty;

    [Required] public Guid ThemeId { get; set; }

    [Required] public ContactSaveRequest Contact { get; set; } = null!;

    private string _slug = string.Empty;

    public string Slug
    {
        get => string.IsNullOrEmpty(_slug) ? NamingUtils.SlugFromTitle(Title) : _slug;
        set => _slug = value;
    }

    public Guid? SupersededById { get; set; }
}

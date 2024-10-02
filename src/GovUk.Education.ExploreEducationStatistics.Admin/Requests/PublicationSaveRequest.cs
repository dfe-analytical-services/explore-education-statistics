#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.ComponentModel.DataAnnotations;
using static System.String;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record PublicationSaveRequest
{
    [Required] public string Title { get; set; } = Empty;

    [Required, MaxLength(160)] public string Summary { get; set; } = Empty;

    [Required] public Guid ThemeId { get; set; }

    [Required] public Guid TopicId { get; set; }

    private string _slug = Empty;

    public string Slug
    {
        get => IsNullOrEmpty(_slug) ? NamingUtils.SlugFromTitle(Title) : _slug;
        set => _slug = value;
    }

    public Guid? SupersededById { get; set; }
}

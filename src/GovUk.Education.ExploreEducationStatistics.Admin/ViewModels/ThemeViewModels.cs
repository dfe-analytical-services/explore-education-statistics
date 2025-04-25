using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class ThemeViewModel
{
    public Guid Id { get; set; }
    public string Slug { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
}

public class ThemeSaveViewModel
{
    private string _slug;

    public string Slug
    {
        get => string.IsNullOrEmpty(_slug) ? NamingUtils.SlugFromTitle(Title) : _slug;
        set => _slug = value;
    }

    public string Summary { get; set; }

    [Required] public string Title { get; set; }
}

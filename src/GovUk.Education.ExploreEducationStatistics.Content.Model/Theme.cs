using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Theme
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    public string Slug { get; set; }

    [Required] public string Title { get; set; }

    public string Summary { get; set; }

    public List<Publication> Publications { get; set; }

    public bool IsTestOrSeedTheme() => Title.StartsWith("UI test theme") || Title.StartsWith("Seed theme");
}

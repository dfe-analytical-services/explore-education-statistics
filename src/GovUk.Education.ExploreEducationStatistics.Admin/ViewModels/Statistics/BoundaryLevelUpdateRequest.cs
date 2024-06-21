using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;

public class BoundaryLevelUpdateRequest
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Label { get; set; }
}

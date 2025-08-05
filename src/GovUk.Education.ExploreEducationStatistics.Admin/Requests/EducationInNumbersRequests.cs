using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record CreateEducationInNumbersPageRequest
{
    [MaxLength(255)] public string Title { get; set; }

    [MaxLength(255)] public string Slug { get; set; }

    [MaxLength(2047)] public string Description { get; set; }
}

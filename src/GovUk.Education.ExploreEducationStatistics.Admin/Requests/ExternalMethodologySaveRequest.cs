#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class ExternalMethodologySaveRequest
{
    [Required] public string Title { get; set; } = string.Empty;

    [Required] [Url] public string Url { get; set; } = string.Empty;
}

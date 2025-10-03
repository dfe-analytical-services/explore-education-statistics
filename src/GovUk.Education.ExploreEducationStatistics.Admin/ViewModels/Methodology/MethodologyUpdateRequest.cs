#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;

public class MethodologyUpdateRequest : MethodologyApprovalUpdateRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
}

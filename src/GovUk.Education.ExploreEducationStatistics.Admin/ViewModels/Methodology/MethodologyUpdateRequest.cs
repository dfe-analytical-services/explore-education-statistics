#nullable enable
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;

public class MethodologyUpdateRequest : MethodologyApprovalUpdateRequest
{
    // TODO SOW4 EES-2212 - update to AlternativeTitle
    [Required] public string Title { get; set; } = string.Empty;
}

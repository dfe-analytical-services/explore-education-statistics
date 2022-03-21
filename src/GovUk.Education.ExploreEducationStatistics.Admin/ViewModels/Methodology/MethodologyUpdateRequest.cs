#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologyUpdateRequest : MethodologyApprovalUpdateRequest
    {
        // TODO SOW4 EES-2212 - update to AlternativeTitle
        [Required] public string Title { get; set; } = string.Empty;

        public string Slug => SlugFromTitle(Title);

        public bool IsDetailUpdateForMethodology(MethodologyVersion methodologyVersion)
        {
            return methodologyVersion.Title != Title;
        }
    }
}

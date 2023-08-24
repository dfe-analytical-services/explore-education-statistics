#nullable enable

using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class CommentSaveRequest
    {
        [Required] public string Content { get; set; } = string.Empty;
        public bool? SetResolved { get; set; }
    }
}

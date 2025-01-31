#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologyNoteUpdateRequest
    {
        [Required] public string Content { get; set; } = string.Empty;

        [Required] public DateTime? DisplayDate { get; set; }
    }
}

#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologyNoteAddRequest
    {
        [Required] public string Content { get; set; } = string.Empty;

        public DateTime DisplayDate { get; set; }
    }
}

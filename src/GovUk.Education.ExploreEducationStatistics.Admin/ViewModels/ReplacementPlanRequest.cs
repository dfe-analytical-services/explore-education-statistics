using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ReplacementPlanRequest
    {
        [Required]
        public Guid? OriginalReleaseFileReferenceId { get; set; }
        
        [Required]
        public Guid? ReplacementReleaseFileReferenceId { get; set; }
    }
}
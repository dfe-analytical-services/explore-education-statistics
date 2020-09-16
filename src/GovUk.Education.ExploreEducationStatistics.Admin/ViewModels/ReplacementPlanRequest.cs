using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ReplacementPlanRequest
    {
        [Required]
        public Guid? OriginalSubjectId { get; set; }
        
        [Required]
        public Guid? ReplacementSubjectId { get; set; }
    }
}
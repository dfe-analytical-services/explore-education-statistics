using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class EditReleaseViewModel
    {
        public Guid Id { get; set; } // Null on create
        public Guid PublicationId { get; set; }

        public Guid ReleaseTypeId { get; set; }
        
        public TimeIdentifier TimeIdentifier { get; set; }
        
        public DateTime? PublishScheduled { get; set; }
        
        [Required]
        public string  NextReleaseExpected { get; set; }

        [PartialDateValidator]
        public string ReleaseName { get; set; }
    }
}
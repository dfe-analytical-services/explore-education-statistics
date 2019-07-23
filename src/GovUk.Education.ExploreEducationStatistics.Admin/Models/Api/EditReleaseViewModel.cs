using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class EditReleaseViewModel
    {
        public Guid? Id { get; set; } // Null on create
        public Guid PublicationId { get; set; }

        public Guid ReleaseTypeId { get; set; }
        
        public TimeIdentifier TimeIdentifier { get; set; }
        
        public DateTime? PublishScheduled { get; set; }
        
        [PartialDateValidator]
        public PartialDate NextReleaseExpected { get; set; }
        
        public string ReleaseName { get; set; }
    }
}
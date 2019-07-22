using System;
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

        public string ReleaseName { get; set; }
    }
}
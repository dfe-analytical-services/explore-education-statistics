using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class AttachContentBlockRequest
    {
        public Guid ContentBlockId { get; set; }
        
        public int? Order { get; set; }
    }
}
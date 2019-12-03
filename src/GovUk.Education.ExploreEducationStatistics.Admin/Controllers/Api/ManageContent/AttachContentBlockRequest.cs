using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    public class AttachContentBlockRequest
    {
        public Guid ContentBlockId { get; set; }
        
        public int? Order { get; set; }
    }
}
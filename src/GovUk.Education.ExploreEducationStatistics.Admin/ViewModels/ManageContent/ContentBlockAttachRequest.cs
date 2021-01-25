using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class ContentBlockAttachRequest
    {
        public Guid ContentBlockId { get; set; }

        public int? Order { get; set; }
    }
}
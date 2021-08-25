#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class DataBlockAttachRequest
    {
        public Guid DataBlockId { get; set; }

        public int? Order { get; set; }
    }
}

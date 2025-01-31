using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class ContentBlockAddRequest
    {
        public ContentBlockType Type { get; set; }

        public int? Order { get; set; }
    }
}
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class AddContentBlockRequest
    {
        public ContentBlockType Type { get; set; }
        
        public int? Order { get; set; }
    }
}
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    public class AddContentBlockRequest
    {
        public ContentBlockType Type { get; set; }
        
        public int? Order { get; set; }
    }
}
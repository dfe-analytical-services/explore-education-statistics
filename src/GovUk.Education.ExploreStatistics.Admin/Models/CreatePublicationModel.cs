using System.Collections.Generic;

namespace GovUk.Education.ExploreStatistics.Admin.Models
{
    public class CreatePublicationModel
    {
        public string Id { get; set; }
        
        public List<PublicationViewModel> Publications { get; set; }
    }
}
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class CreatePublicationModel
    {
        public string Id { get; set; }
        
        public List<PublicationViewModel> Publications { get; set; }

        public List<ImportViewModel> PublicationsToImport { get; set; }
    }
}
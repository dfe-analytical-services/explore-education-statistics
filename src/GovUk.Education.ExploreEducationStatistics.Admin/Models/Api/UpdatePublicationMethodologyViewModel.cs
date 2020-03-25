using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class UpdatePublicationMethodologyViewModel
    {
        public Guid? MethodologyId { get; set; }
        
        public ExternalMethodology ExternalMethodology { get; set; }
    }
}
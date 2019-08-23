using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class PublicationSubjectsMetaViewModel
    {
        public Guid PublicationId { get; set; }        
        public IEnumerable<IdLabel> Subjects { get; set; }
    }
}
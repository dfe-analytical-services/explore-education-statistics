using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class PublicationSubjectsMetaViewModel
    {
        public Guid PublicationId { get; set; }        
        public IEnumerable<IdLabelViewModel> Subjects { get; set; }
    }
}
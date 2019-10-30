using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class ReleaseSubjectsMetaViewModel
    {
        public Guid ReleaseId { get; set; }
        public IEnumerable<IdLabel> Subjects { get; set; }
    }
}
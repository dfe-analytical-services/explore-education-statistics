using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class SubjectsMetaViewModel
    {
        public Guid ReleaseId { get; set; }
        public List<IdLabel> Subjects { get; set; }
    }
}
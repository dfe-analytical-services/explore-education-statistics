using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnotesSubjectMetaViewModel
    {
        public Dictionary<Guid, FootnotesFilterMetaViewModel> Filters { get; set; }

        public Dictionary<Guid, FootnotesIndicatorsMetaViewModel> Indicators { get; set; }

        public Guid SubjectId { get; set; }

        public string SubjectName { get; set; }
    }
}
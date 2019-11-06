using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnotesSubjectMetaViewModel
    {
        public Dictionary<long, FilterMetaViewModel2> Filters { get; set; }

        public Dictionary<long, IndicatorsMetaViewModel2> Indicators { get; set; }

        public long SubjectId { get; set; }

        public string SubjectName { get; set; }
    }
}
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics
{
    public class FootnotesSubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; set; }
        
        public Dictionary<string, IndicatorsMetaViewModel> Indicators { get; set; }
        
        public long SubjectId { get; set; }
        
        public string SubjectName { get; set; }
    }
}
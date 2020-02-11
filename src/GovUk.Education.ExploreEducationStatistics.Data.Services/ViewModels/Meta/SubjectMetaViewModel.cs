using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class SubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; set; }

        public Dictionary<string, IndicatorMetaViewModel> Indicators { get; set; }

        public Dictionary<string, ObservationalUnitGeoJsonMeta> Locations { get; set; }
        
        public IEnumerable<BoundaryLevelIdLabel> BoundaryLevels { get; set; }
        
        public Dictionary<string, TimePeriodMetaViewModel> TimePeriod { get; set; }
        
        public string PublicationName { get; set; }

        public string SubjectName { get; set; }

        public IEnumerable<FootnoteViewModel> Footnotes { get; set; }
        
        public bool IsGeoJsonAvailable { get; set; }

    }
}
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta
{
    public class ResultSubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; set; }

        public IEnumerable<FootnoteViewModel> Footnotes { get; set; }

        public IEnumerable<IndicatorMetaViewModel> Indicators { get; set; }

        // TODO SOW8 EES-2881 Change this to support both proposed and old location formats 
        public IEnumerable<ObservationalUnitMetaViewModel> Locations { get; set; }

        public IEnumerable<BoundaryLevelIdLabel> BoundaryLevels { get; set; }

        public string PublicationName { get; set; }

        public string SubjectName { get; set; }

        public IEnumerable<TimePeriodMetaViewModel> TimePeriodRange { get; set; }

        public bool GeoJsonAvailable { get; set; }
    }
}

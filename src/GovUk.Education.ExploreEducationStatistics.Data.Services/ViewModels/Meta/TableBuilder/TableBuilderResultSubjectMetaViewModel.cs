using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder
{
    public class TableBuilderResultSubjectMetaViewModel
    {
        public Dictionary<string, FilterMetaViewModel> Filters { get; set; }

        public IEnumerable<FootnoteViewModel> Footnotes { get; set; }

        public IEnumerable<IndicatorMetaViewModel> Indicators { get; set; }

        public IEnumerable<ObservationalUnitMetaViewModel> Locations { get; set; }

        public string PublicationName { get; set; }

        public string SubjectName { get; set; }

        public IEnumerable<TimePeriodMetaViewModel> TimePeriodRange { get; set; }
    }
}
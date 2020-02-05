using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class DataBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Heading { get; set; }

        public string CustomFootnotes { get; set; }

        public string Name { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext DataBlockRequest { get; set; }

        public List<IContentBlockChart> Charts { get; set; }

        public DataBlockSummaryViewModel Summary { get; set; }

        public List<DataBlockTableViewModel> Tables { get; set; }

        public string Type => "DataBlock";
    }
}
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class DataBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; set; }

        public int Order { get; set; }

        public string Heading { get; set; }

        public string Name { get; set; }

        public string Source { get; set; }

        public ObservationQueryContext DataBlockRequest { get; set; }

        [Obsolete("Maintain compatibility with existing cached DataBlocks")]
        public List<IContentBlockChart> Charts
        {
            set
            {
                if (value != null && value.Count > 0)
                {
                    Chart = value[0];
                }
            }
        }

        public IContentBlockChart Chart { get; set; }

        public DataBlockSummaryViewModel Summary { get; set; }

        public List<TableBuilderConfiguration> Tables { get; set; }

        public string Type => "DataBlock";
    }
}
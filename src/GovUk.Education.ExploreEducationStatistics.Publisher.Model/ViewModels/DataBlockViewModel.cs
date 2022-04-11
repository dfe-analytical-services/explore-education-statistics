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

        public ObservationQueryContext Query { get; set; }

        // TODO EES-3319 - remove backwards-compatibility for Map Configuration without its
        // own Boundary Level selection
        public List<IChart> Charts
        {
            get => Charts;
            set
            {
                value.ForEach(chart =>
                {
                    if (chart is MapChart { BoundaryLevel: null } mapChart)
                    {
                        mapChart.BoundaryLevel = Query.BoundaryLevel;       
                    }
                }); 
            }
        }

        public DataBlockSummaryViewModel Summary { get; set; }

        public TableBuilderConfiguration Table { get; set; }

        public string Type => "DataBlock";
    }
}
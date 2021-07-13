using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class ProcessorStatistics
    {
        public int FilteredObservationCount { get; set; }
        public int RowsPerBatch { get; set; }
        public int NumBatches { get; set; }
        public HashSet<GeographicLevel> GeographicLevels { get; set; }

        public ProcessorStatistics(int filteredObservationCount,
            int rowsPerBatch,
            int numBatches,
            HashSet<GeographicLevel> geographicLevels)
        {
            FilteredObservationCount = filteredObservationCount;
            RowsPerBatch = rowsPerBatch;
            NumBatches = numBatches;
            GeographicLevels = geographicLevels;
        }
    }
}

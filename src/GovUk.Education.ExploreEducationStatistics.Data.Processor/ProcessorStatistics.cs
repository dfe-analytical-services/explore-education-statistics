#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class ProcessorStatistics
    {
        public int ImportableRowCount { get; }
        public int RowsPerBatch { get; }
        public int NumBatches { get; }
        public HashSet<GeographicLevel> GeographicLevels { get; }

        public ProcessorStatistics(int importableRowCount,
            int rowsPerBatch,
            int numBatches,
            HashSet<GeographicLevel> geographicLevels)
        {
            ImportableRowCount = importableRowCount;
            RowsPerBatch = rowsPerBatch;
            NumBatches = numBatches;
            GeographicLevels = geographicLevels;
        }
    }
}

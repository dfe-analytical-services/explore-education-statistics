#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class ProcessorStatistics
    {
        public int TotalRows { get; }
        public int RowsPerBatch { get; }
        public int NumBatches { get; }
        public HashSet<GeographicLevel> GeographicLevels { get; }

        public ProcessorStatistics(int totalRows,
            int rowsPerBatch,
            int numBatches,
            HashSet<GeographicLevel> geographicLevels)
        {
            TotalRows = totalRows;
            RowsPerBatch = rowsPerBatch;
            NumBatches = numBatches;
            GeographicLevels = geographicLevels;
        }
    }
}

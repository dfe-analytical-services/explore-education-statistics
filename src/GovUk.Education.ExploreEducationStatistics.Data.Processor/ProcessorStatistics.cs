#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor;

public record ProcessorStatistics(int TotalRowCount,
    int ImportableRowCount,
    int RowsPerBatch,
    int NumBatches,
    HashSet<GeographicLevel> GeographicLevels);

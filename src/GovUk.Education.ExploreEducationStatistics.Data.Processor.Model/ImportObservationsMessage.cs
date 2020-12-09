using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Model
{
    public class ImportObservationsMessage
    {
        public Guid SubjectId { get; set; }
        public string ObservationsFilePath { get; set; }
        public string DataFileName { get; set; }
        public Guid ReleaseId { get; set; }
        public int NumBatches { get; set; }
        public int BatchNo { get; set; }
        public int RowsPerBatch { get; set; }
        public int TotalRows { get; set; }

    }
}
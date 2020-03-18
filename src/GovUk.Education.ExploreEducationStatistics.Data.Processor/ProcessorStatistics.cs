namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class ProcessorStatistics
    {
        public int FilteredObservationCount { get; set; }
        public int RowsPerBatch { get; set; }
        public int NumBatches { get; set; }

        public ProcessorStatistics(int filteredObservationCount, int rowsPerBatch, int numBatches)
        {
            FilteredObservationCount = filteredObservationCount;
            RowsPerBatch = rowsPerBatch;
            NumBatches = numBatches;
        }

        public ProcessorStatistics()
        {
        }
    }
}
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    public class Batch : TableEntity
    {
        public byte[]BatchesProcessed  { get; set; }
        public int BatchSize { get; set; }
        public int Status { get; set; }
        public string Errors { get; set; }

        public Batch(string releaseId, string subjectId, int batchSize)
        {
            PartitionKey = releaseId;
            RowKey = subjectId;
            this.BatchSize = batchSize;
            this.BatchesProcessed = new byte[32];
        }

        public Batch()
        {
        }
    }
}
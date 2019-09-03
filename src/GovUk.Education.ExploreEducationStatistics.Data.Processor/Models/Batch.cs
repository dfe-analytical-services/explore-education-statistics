using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    public class Batch : TableEntity
    {
        public byte[] BatchesProcessed { get; set; }
        public int NumBatches { get; set; }
        public int Status { get; set; }
        public string Errors { get; set; }

        public Batch(string releaseId, string dataFileName, int numBatches)
        {
            PartitionKey = releaseId;
            RowKey = dataFileName;
            this.NumBatches = numBatches;
            this.BatchesProcessed = new byte[64];
            this.Errors = "";
        }

        public Batch()
        {
        }
    }
}
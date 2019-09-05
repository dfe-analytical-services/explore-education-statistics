using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class DatafileImport : TableEntity
    {
        public byte[] BatchesProcessed { get; set; }
        public int NumBatches { get; set; }
        public int Status { get; set; }
        public string Errors { get; set; }

        public DatafileImport(string releaseId, string dataFileName, int numBatches)
        {
            PartitionKey = releaseId;
            RowKey = dataFileName;
            NumBatches = numBatches;
            BatchesProcessed = new byte[64];
            Errors = "";
            Status = 1;
        }

        public DatafileImport()
        {
        }
    }
}
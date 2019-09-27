using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class DatafileImport : TableEntity
    {
        public byte[] BatchesProcessed { get; set; }
        public int NumBatches { get; set; }
        [EntityEnumPropertyConverter]
        public IStatus Status { get; set; }
        public string Errors { get; set; }
        
        public int NumberOfRows { get; set; }

        public DatafileImport(string releaseId, string dataFileName, int numberOfRows, int numBatches)
        {
            PartitionKey = releaseId;
            RowKey = dataFileName;
            NumBatches = numBatches;
            BatchesProcessed = new byte[64];
            Errors = "";
            Status = IStatus.RUNNING_PHASE_1;
            NumberOfRows = numberOfRows;
        }

        public DatafileImport()
        {
        }
        
        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            EntityEnumPropertyConverter.Serialize(this, results);
            return results;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            EntityEnumPropertyConverter.Deserialize(this, properties);
        }
    }
}
namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    using System;
    using Microsoft.Azure.Cosmos.Table;
    
    public class PermalinkEntity : TableEntity
    {
        public PermalinkEntity()
        {
            // TODO: im not sure about the partition key, requires some more thought
            PartitionKey = "the-publication-id";
            RowKey = Guid.NewGuid().ToString();
        }

        public string Title { get; set; }
        
        public string Data { get; set; }
        
        public string Query { get; set; }
    }
}
using System;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Permalink : TableEntity
    {
        public Permalink()
        {
            // TODO: im not sure about the partition key, requires some more thought
            PartitionKey = "the-publication-id";
            RowKey = Guid.NewGuid().ToString();
        }

        public string Title { get; set; }

        // The metadata associated with the query, stored as a json string
        public string MetaData { get; set; }

        // The statistical data for the query, stored as a json string
        public string Result { get; set; }

        // the time period range of the querey, stored as a json string
        public string TimePeriodRange { get; set; }

        // The footnotes object associated with the query, stored as a json string
        public string Footnotes { get; set; }
    }
}
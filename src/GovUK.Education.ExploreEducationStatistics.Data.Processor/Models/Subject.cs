namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    using Microsoft.WindowsAzure.Storage.Blob;

    public class Subject
    {
        public CloudBlockBlob CsvDataBlob { get; set; }

        public CloudBlockBlob CsvMetaDataBlob { get; set; }

        public string Name { get; set; }
    }
}
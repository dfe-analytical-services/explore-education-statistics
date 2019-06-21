using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    public class SubjectData
    {
        public CloudBlockBlob DataBlob { get; }
        public CloudBlockBlob MetaBlob { get; }
        public string Name { get; }

        public SubjectData(CloudBlockBlob dataBlob, CloudBlockBlob metaBlob, string name)
        {
            DataBlob = dataBlob;
            MetaBlob = metaBlob;
            Name = name;
        }
    }
}
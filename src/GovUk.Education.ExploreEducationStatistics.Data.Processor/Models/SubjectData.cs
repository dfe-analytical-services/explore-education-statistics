using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    public class SubjectData
    {
        public SubjectData(CloudBlockBlob dataBlob, CloudBlockBlob metaBlob, string name)
        {
            DataBlob = dataBlob;
            MetaBlob = metaBlob;
            Name = name;
        }

        public CloudBlockBlob DataBlob { get; }
        public CloudBlockBlob MetaBlob { get; }
        public string Name { get; }
    }
}
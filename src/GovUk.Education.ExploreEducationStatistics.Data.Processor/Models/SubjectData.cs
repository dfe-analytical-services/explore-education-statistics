using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    public class SubjectData
    {
        public BlobInfo DataBlob { get; }

        public BlobInfo MetaBlob { get; }

        public string Name => DataBlob.Name;

        public SubjectData(
            BlobInfo dataBlob,
            BlobInfo metaBlob)
        {
            DataBlob = dataBlob;
            MetaBlob = metaBlob;
        }
    }
}
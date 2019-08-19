using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils
{
    public static class BlobUtils
    {
        public static string GetMetaFileName(CloudBlob blob)
        {
            return blob.Metadata["metafile"];
        }
        
        public static string GetName(CloudBlob blob)
        {
            return blob.Metadata["name"];
        }
    }
}
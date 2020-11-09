using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions
{
    public static class ReleaseFileReferenceExtensions
    {
        public static string Path(this ReleaseFileReference file)
        {
            return FileStoragePathUtils.AdminReleasePath(
                file.ReleaseId,
                file.ReleaseFileType,
                file.BlobStorageName);
        }
    }
}
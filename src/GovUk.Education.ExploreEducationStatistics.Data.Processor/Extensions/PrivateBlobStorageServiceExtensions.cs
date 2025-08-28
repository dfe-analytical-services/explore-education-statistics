using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;

public static class PrivateBlobStorageServiceExtensions
{
    public static Func<Task<Stream>> GetDataFileStreamProvider(
        this IPrivateBlobStorageService service,
        DataImport import)
    {
        return async () => (await service
            .GetDownloadStream(PrivateReleaseFiles, import.File.Path()))
            .Right;
    }
    
    public static Func<Task<Stream>> GetMetadataFileStreamProvider(
        this IPrivateBlobStorageService service,
        DataImport import)
    {
        return async () => (await service
            .GetDownloadStream(PrivateReleaseFiles, import.MetaFile.Path()))
            .Right;
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IReleaseDataFileRepository
{
    Task<File> Create(
        Guid releaseVersionId,
        Guid subjectId,
        string filename,
        long contentLength,
        FileType type,
        Guid createdById,
        string? name = null,
        File? replacingDataFile = null,
        int order = 0);

    Task<IList<File>> ListDataFiles(Guid releaseVersionId);

    Task<bool> HasAnyDataFiles(Guid releaseVersionId);

    Task<IList<File>> ListReplacementDataFiles(Guid releaseVersionId);

    Task<ReleaseFile> GetBySubject(
        Guid releaseVersionId,
        Guid subjectId);
}

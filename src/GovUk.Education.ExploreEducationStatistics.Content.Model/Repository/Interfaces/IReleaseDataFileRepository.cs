#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IReleaseDataFileRepository
{
    public Task<File> Create(
        Guid releaseVersionId,
        Guid subjectId,
        string filename,
        long contentLength,
        FileType type,
        Guid createdById,
        bool? apiCompatible = null,
        string? name = null,
        File? replacingDataFile = null,
        int order = 0
    );

    public Task<IList<File>> ListDataFiles(Guid releaseVersionId);

    public Task<bool> HasAnyDataFiles(Guid releaseVersionId);

    public Task<IList<File>> ListReplacementDataFiles(Guid releaseVersionId);

    public Task<ReleaseFile> GetBySubject(Guid releaseVersionId, Guid subjectId);
}

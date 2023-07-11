using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;

public interface IFileRepository
{
    public Task Delete(Guid id);

    public Task<File> Get(Guid id);

    Task<File> Create(
        Guid releaseId,
        string filename,
        long contentLength,
        string contentType,
        FileType type,
        Guid createdById,
        Guid? newFileId = null);
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileRepository
    {
        public Task<ReleaseFileReference> Create(
            Guid releaseId,
            string filename,
            ReleaseFileTypes type,
            ReleaseFileReference replacingFile = null,
            ReleaseFileReference source = null);

        public Task<ReleaseFileReference> CreateZip(
            Guid releaseId,
            string filename);

        public Task Delete(Guid id);

        public Task<ReleaseFileReference> Get(Guid id);

        public Task<IList<ReleaseFileReference>> ListDataFiles(Guid releaseId);

        public Task<bool> HasAnyDataFiles(Guid releaseId);

        public Task<ReleaseFileReference> UpdateFilename(
            Guid releaseId,
            Guid fileId,
            string filename);
    }
}
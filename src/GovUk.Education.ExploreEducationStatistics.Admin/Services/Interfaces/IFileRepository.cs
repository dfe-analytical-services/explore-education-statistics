using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileRepository
    {
        public Task<File> Create(
            Guid releaseId,
            string filename,
            FileType type,
            File replacingFile = null,
            File source = null);

        public Task<File> CreateZip(
            Guid releaseId,
            string filename);

        public Task Delete(Guid id);

        public Task<File> Get(Guid id);

        public Task<IList<File>> ListDataFiles(Guid releaseId);

        public Task<bool> HasAnyDataFiles(Guid releaseId);

        public Task<File> UpdateFilename(
            Guid releaseId,
            Guid fileId,
            string filename);

        public Task<IList<File>> ListReplacementDataFiles(Guid releaseId);
    }
}
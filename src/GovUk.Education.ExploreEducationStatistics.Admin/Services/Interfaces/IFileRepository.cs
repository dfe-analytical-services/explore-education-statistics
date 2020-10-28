using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileRepository
    {
        public Task<ReleaseFileReference> CreateOrUpdate(
            string filename,
            Guid releaseId,
            ReleaseFileTypes type,
            Guid? id = null,
            ReleaseFileReference replacingFile = null,
            ReleaseFileReference source = null);

        public Task<ReleaseFileReference> Get(Guid id);
    }
}
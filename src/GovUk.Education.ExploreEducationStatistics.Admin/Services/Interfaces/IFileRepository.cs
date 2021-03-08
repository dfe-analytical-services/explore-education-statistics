using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileRepository
    {
        public Task Delete(Guid id);

        public Task<File> Get(Guid id);

        public Task<string> GetSubjectName(Guid releaseId, Guid subjectId);
    }
}

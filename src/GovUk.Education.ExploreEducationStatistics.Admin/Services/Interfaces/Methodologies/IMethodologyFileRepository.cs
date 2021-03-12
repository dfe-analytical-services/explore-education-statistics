using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyFileRepository
    {
        public Task<File> Create(
            Guid methodologyId,
            string filename,
            FileType type,
            Guid createdById);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyRepository : IMethodologyRepository
    {
        private readonly ContentDbContext _contentDbContext;

        public MethodologyRepository(
            ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<List<Methodology>> GetLatestMethodologiesByRelease(Guid releaseId)
        {
            // TODO SOW4 EES-2379 Get latest methodologies for a Release for approval checklist
            return await Task.FromResult(new List<Methodology>());
        }
    }
}

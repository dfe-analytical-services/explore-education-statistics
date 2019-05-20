using GovUk.Education.ExploreEducationStatistics.Content.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class MethedologyService : IMethedologyService
    {
        private readonly ApplicationDbContext _context;

        public MethedologyService(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}

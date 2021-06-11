using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class MethodologyService : IMethodologyService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMapper _mapper;

        public MethodologyService(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMapper mapper)
        {
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, MethodologyViewModel>> Get(string slug)
        {
            // TODO SOW4 EES-2375 lookup the MethodologyParent by slug when slug is moved to the parent
            // For now, this does a lookup on the parent via any Methodology with the slug
            return await _persistenceHelper
                .CheckEntityExists<Methodology>(
                    query => query
                        .Include(mv => mv.MethodologyParent)
                        .ThenInclude(m => m.Versions)
                        .Where(mv => mv.Slug == slug))
                .OnSuccess<ActionResult, Methodology, MethodologyViewModel>(arbitraryVersion =>
                {
                    var latestPublishedVersion = arbitraryVersion.MethodologyParent.LatestPublishedVersion();
                    if (latestPublishedVersion == null)
                    {
                        return new NotFoundResult();
                    }

                    return _mapper.Map<MethodologyViewModel>(latestPublishedVersion);
                });
        }
    }
}

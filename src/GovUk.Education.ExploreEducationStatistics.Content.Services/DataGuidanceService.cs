#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class DataGuidanceService : IDataGuidanceService
    {
        private readonly IDataGuidanceSubjectService _dataGuidanceSubjectService;
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IReleaseCacheService _releaseCacheService;

        public DataGuidanceService(IDataGuidanceSubjectService dataGuidanceSubjectService,
            IPublicationCacheService publicationCacheService,
            IReleaseCacheService releaseCacheService)
        {
            _dataGuidanceSubjectService = dataGuidanceSubjectService;
            _publicationCacheService = publicationCacheService;
            _releaseCacheService = releaseCacheService;
        }

        public async Task<Either<ActionResult, DataGuidanceViewModel>> Get(string publicationSlug,
            string? releaseSlug = null)
        {
            return await _publicationCacheService.GetPublication(publicationSlug)
                .OnSuccessCombineWith(_ => _releaseCacheService.GetRelease(publicationSlug, releaseSlug))
                .OnSuccess(publicationAndRelease =>
                {
                    var (publication, release) = publicationAndRelease;
                    return _dataGuidanceSubjectService.GetSubjects(release.Id)
                        .OnSuccess(subjects => new DataGuidanceViewModel(
                            release,
                            publication,
                            subjects
                        ));
                });
        }
    }
}

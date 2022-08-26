#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class DataGuidanceService : IDataGuidanceService
    {
        private readonly IDataGuidanceSubjectService _dataGuidanceSubjectService;
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IReleaseService _releaseService;

        public DataGuidanceService(IDataGuidanceSubjectService dataGuidanceSubjectService,
            IPublicationCacheService publicationCacheService,
            IReleaseService releaseService)
        {
            _dataGuidanceSubjectService = dataGuidanceSubjectService;
            _publicationCacheService = publicationCacheService;
            _releaseService = releaseService;
        }

        public async Task<Either<ActionResult, DataGuidanceViewModel>> Get(string publicationSlug,
            string? releaseSlug = null)
        {
            return await _publicationCacheService.GetPublication(publicationSlug)
                .OnSuccessCombineWith(_ => _releaseService.GetCachedRelease(publicationSlug, releaseSlug))
                .OnSuccess(publicationAndRelease =>
                {
                    var (publication, release) = publicationAndRelease;
                    return _dataGuidanceSubjectService.GetSubjects(release!.Id)
                        .OnSuccess(subjects => new DataGuidanceViewModel(
                            release,
                            publication,
                            subjects
                        ));
                });
        }
    }
}

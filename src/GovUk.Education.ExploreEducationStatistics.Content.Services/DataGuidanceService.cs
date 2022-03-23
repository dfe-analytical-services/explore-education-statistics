#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class DataGuidanceService : IDataGuidanceService
    {
        private readonly IDataGuidanceSubjectService _dataGuidanceSubjectService;
        private readonly Interfaces.IPublicationService _publicationService;
        private readonly Interfaces.IReleaseService _releaseService;

        public DataGuidanceService(
            IDataGuidanceSubjectService dataGuidanceSubjectService,
            Interfaces.IPublicationService publicationService,
            Interfaces.IReleaseService releaseService)
        {
            _dataGuidanceSubjectService = dataGuidanceSubjectService;
            _publicationService = publicationService;
            _releaseService = releaseService;
        }

        public async Task<Either<ActionResult, DataGuidanceViewModel>> Get(string publicationSlug, string? releaseSlug = null)
        {
            return await _publicationService.Get(publicationSlug)
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

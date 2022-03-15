#nullable enable
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
            var publicationTask = _publicationService.Get(publicationSlug);
            var releaseTask = _releaseService.GetCachedRelease(publicationSlug, releaseSlug);

            await Task.WhenAll(publicationTask, releaseTask);

            if (releaseTask.Result.IsRight && publicationTask.Result.IsRight)
            {
                return await _dataGuidanceSubjectService.GetSubjects(releaseTask.Result.Right!.Id)
                    .OnSuccess(
                        subjects =>
                            new DataGuidanceViewModel(
                                releaseTask.Result.Right,
                                publicationTask.Result.Right,
                                subjects
                            )
                    );
            }

            return new Either<ActionResult, DataGuidanceViewModel>(new NotFoundResult());
        }
    }
}

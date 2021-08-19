using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class MetaGuidanceService : IMetaGuidanceService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IMetaGuidanceSubjectService _metaGuidanceSubjectService;

        public MetaGuidanceService(
            IFileStorageService fileStorageService,
            IMetaGuidanceSubjectService metaGuidanceSubjectService)
        {
            _fileStorageService = fileStorageService;
            _metaGuidanceSubjectService = metaGuidanceSubjectService;
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Get(string publicationPath, string releasePath)
        {
            var publicationTask = _fileStorageService.GetDeserialized<CachedPublicationViewModel>(publicationPath);
            var releaseTask = _fileStorageService.GetDeserialized<CachedReleaseViewModel>(releasePath);

            await Task.WhenAll(publicationTask, releaseTask);

            if (releaseTask.Result.IsRight && publicationTask.Result.IsRight)
            {
                return await _metaGuidanceSubjectService.GetSubjects(releaseTask.Result.Right.Id)
                    .OnSuccess(
                        subjects =>
                            new MetaGuidanceViewModel(
                                releaseTask.Result.Right,
                                publicationTask.Result.Right,
                                subjects
                            )
                    );
            }

            return new Either<ActionResult, MetaGuidanceViewModel>(new NotFoundResult());
        }
    }
}

using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class DataGuidanceService : IDataGuidanceService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IDataGuidanceSubjectService _dataGuidanceSubjectService;

        public DataGuidanceService(
            IFileStorageService fileStorageService,
            IDataGuidanceSubjectService dataGuidanceSubjectService)
        {
            _fileStorageService = fileStorageService;
            _dataGuidanceSubjectService = dataGuidanceSubjectService;
        }

        public async Task<Either<ActionResult, DataGuidanceViewModel>> Get(string publicationPath, string releasePath)
        {
            var publicationTask = _fileStorageService.GetDeserialized<CachedPublicationViewModel>(publicationPath);
            var releaseTask = _fileStorageService.GetDeserialized<CachedReleaseViewModel>(releasePath);

            await Task.WhenAll(publicationTask, releaseTask);

            if (releaseTask.Result.IsRight && publicationTask.Result.IsRight)
            {
                return await _dataGuidanceSubjectService.GetSubjects(releaseTask.Result.Right.Id)
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

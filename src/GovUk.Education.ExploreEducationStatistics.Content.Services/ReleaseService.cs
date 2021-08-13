#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IMethodologyService _methodologyService;

        public ReleaseService(IFileStorageService fileStorageService, IMethodologyService methodologyService)
        {
            _fileStorageService = fileStorageService;
            _methodologyService = methodologyService;
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> Get(string publicationPath, string releasePath)
        {
            return await CreateFromCachedPublicationAndRelease(publicationPath, releasePath,
                    (publication, release) => new ReleaseViewModel(release, publication))
                .OnSuccessCombineWith(model => _methodologyService.GetSummariesByPublication(model.Publication.Id))
                .OnSuccess(viewModelAndMethodologies =>
                {
                    var (viewModel, methodologies) = viewModelAndMethodologies;
                    viewModel.Publication.Methodologies = methodologies;
                    return viewModel;
                });
        }

        public async Task<Either<ActionResult, ReleaseSummaryViewModel>> GetSummary(string publicationPath,
            string releasePath)
        {
            return await CreateFromCachedPublicationAndRelease(publicationPath, releasePath,
                (publication, release) => new ReleaseSummaryViewModel(release, publication));
        }

        private async Task<Either<ActionResult, T>> CreateFromCachedPublicationAndRelease<T>(
            string publicationPath,
            string releasePath,
            Func<CachedPublicationViewModel, CachedReleaseViewModel, T> func)
        {
            var publicationTask = _fileStorageService.GetDeserialized<CachedPublicationViewModel>(publicationPath);
            var releaseTask = _fileStorageService.GetDeserialized<CachedReleaseViewModel>(releasePath);

            await Task.WhenAll(publicationTask, releaseTask);

            if (releaseTask.Result.IsRight && publicationTask.Result.IsRight)
            {
                return func.Invoke(publicationTask.Result.Right, releaseTask.Result.Right);
            }

            return new NotFoundResult();
        }
    }
}
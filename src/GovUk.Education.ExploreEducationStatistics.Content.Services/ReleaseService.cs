#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMethodologyService _methodologyService;
        private readonly IUserService _userService;

        public ReleaseService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IFileStorageService fileStorageService,
            IMethodologyService methodologyService,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _fileStorageService = fileStorageService;
            _methodologyService = methodologyService;
            _userService = userService;
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

        public async Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> List(string publicationSlug)
        {
            return await _persistenceHelper.CheckEntityExists<Publication>(
                    q => q
                        .Include(p => p.Releases)
                        .Where(p => p.Slug == publicationSlug)
                )
                .OnSuccess(_userService.CheckCanViewPublication)
                .OnSuccess(
                    publication => publication.Releases
                        .Where(release => release.IsLatestPublishedVersionOfRelease())
                        .OrderByDescending(r => r.Year)
                        .ThenByDescending(r => r.TimePeriodCoverage)
                        .Select(release => new ReleaseSummaryViewModel(release))
                        .ToList()
                );
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
                if (publicationTask.Result.Right is not null
                    && releaseTask.Result.Right is not null)
                {
                    return func.Invoke(publicationTask.Result.Right, releaseTask.Result.Right);
                }
            }

            return new NotFoundResult();
        }
    }
}

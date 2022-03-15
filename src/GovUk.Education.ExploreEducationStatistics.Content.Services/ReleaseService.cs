#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMethodologyService _methodologyService;
        private readonly IUserService _userService;
        private readonly IPublicationService _publicationService;
        private readonly IMapper _mapper;

        public ReleaseService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IFileStorageService fileStorageService,
            IMethodologyService methodologyService,
            IUserService userService,
            IPublicationService publicationService,
            IMapper mapper)

        {
            _persistenceHelper = persistenceHelper;
            _fileStorageService = fileStorageService;
            _methodologyService = methodologyService;
            _userService = userService;
            _publicationService = publicationService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> Get(string publicationSlug, string? releaseSlug = null)
        {
            return await _publicationService.Get(publicationSlug)
                .OnSuccessCombineWith(publication => _methodologyService.GetSummariesByPublication(publication.Id))
                .OnSuccess(async tuple =>
                {
                    var (publication, methodologies) = tuple;
                    var cachedRelease = await GetCachedRelease(publicationSlug, releaseSlug);

                    if (cachedRelease.IsRight
                        && cachedRelease.Right is not null
                        && publication is not null)
                    {
                        var result = new Either<ActionResult, ReleaseViewModel>(new ReleaseViewModel(
                            _mapper.Map<CachedReleaseViewModel>(cachedRelease.Right),
                            _mapper.Map<PublicationViewModel>(publication)
                        ));

                        result.Right.Publication.Methodologies = methodologies;
                        return result;
                    }

                    return new NotFoundResult();
                });
        }

        public async Task<Either<ActionResult, ReleaseSummaryViewModel>> GetSummary(string publicationSlug,
            string? releaseSlug)
        {
            return await _persistenceHelper
                .CheckEntityExists<Publication>(query => query
                    .Where(p => p.Slug == publicationSlug))
                .OnSuccess(async _ =>
                {
                    var publicationTask = _publicationService.Get(publicationSlug);
                    var releaseTask = GetCachedRelease(publicationSlug, releaseSlug);

                    await Task.WhenAll(publicationTask, releaseTask);

                    if (releaseTask.Result.IsRight
                        && releaseTask.Result.Right is not null
                        && publicationTask.Result.IsRight
                        && publicationTask.Result.Right is not null)
                    {
                        return new Either<ActionResult, ReleaseSummaryViewModel>(new ReleaseSummaryViewModel(
                            _mapper.Map<CachedReleaseViewModel>(releaseTask.Result.Right),
                            _mapper.Map<PublicationViewModel>(publicationTask.Result.Right)
                        ));
                    }

                    return new NotFoundResult();
                });
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
                    // @MarkFix EES-3149 Superseded releases shouldn't have "latest data" label
                    publication => publication.Releases
                        .Where(release => release.IsLatestPublishedVersionOfRelease())
                        .OrderByDescending(r => r.Year)
                        .ThenByDescending(r => r.TimePeriodCoverage)
                        .Select(release => new ReleaseSummaryViewModel(release))
                        .ToList()
                );
        }

        public Task<Either<ActionResult, CachedReleaseViewModel?>> GetCachedRelease(
            string publicationSlug, string? releaseSlug = null)
        {
            var releasePath = releaseSlug != null
                ? PublicContentReleasePath(publicationSlug, releaseSlug)
                : PublicContentLatestReleasePath(publicationSlug);
            return _fileStorageService.GetDeserialized<CachedReleaseViewModel>(releasePath);
        }
    }
}

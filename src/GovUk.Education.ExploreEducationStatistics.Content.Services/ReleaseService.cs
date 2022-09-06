﻿#nullable enable
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
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
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
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IPublicationCacheService _publicationCacheService;
        private readonly IUserService _userService;

        public ReleaseService(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IFileStorageService fileStorageService,
            IMethodologyCacheService methodologyCacheService,
            IPublicationCacheService publicationCacheService,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _fileStorageService = fileStorageService;
            _methodologyCacheService = methodologyCacheService;
            _publicationCacheService = publicationCacheService;
            _userService = userService;
        }

        // TODO EES-3643 - move into a ReleaseCacheService?
        public async Task<Either<ActionResult, ReleaseViewModel>> GetCachedViewModel(string publicationSlug, string? releaseSlug = null)
        {
            return await _publicationCacheService.GetPublication(publicationSlug)
                .OnSuccessCombineWith(publication => _methodologyCacheService.GetSummariesByPublication(publication.Id))
                .OnSuccess(async tuple =>
                {
                    var (publication, methodologies) = tuple;
                    return await GetCachedRelease(publicationSlug, releaseSlug)
                        .OnSuccess(cachedRelease =>
                        {
                            var result = new ReleaseViewModel(
                                cachedRelease!,
                                publication
                            )
                            {
                                Publication =
                                {
                                    Methodologies = methodologies
                                }
                            };

                            return result;
                        });
                });
        }

        public async Task<Either<ActionResult, ReleaseSummaryViewModel>> GetSummary(string publicationSlug,
            string? releaseSlug)
        {
            return await _publicationCacheService.GetPublication(publicationSlug)
                .OnSuccessCombineWith(_ => GetCachedRelease(publicationSlug, releaseSlug))
                .OnSuccess(publicationAndRelease =>
                {
                    var (publication, release) = publicationAndRelease;
                    return new ReleaseSummaryViewModel(
                        release!,
                        publication
                    );
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
                .OnSuccess(publication => publication.Releases
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

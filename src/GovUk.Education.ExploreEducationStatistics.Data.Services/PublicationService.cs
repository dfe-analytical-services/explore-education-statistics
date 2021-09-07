#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly IReleaseRepository _releaseRepository;
        private readonly IReleaseService _releaseService;

        public PublicationService(
            IReleaseRepository releaseRepository,
            IReleaseService releaseService)
        {
            _releaseRepository = releaseRepository;
            _releaseService = releaseService;
        }

        public async Task<Either<ActionResult, List<SubjectViewModel>>> ListLatestReleaseSubjects(
            Guid publicationId)
        {
            return await GetLatestRelease(publicationId)
                .OnSuccess(release => _releaseService.ListSubjects(release.Id));
        }

        public async Task<Either<ActionResult, List<FeaturedTableViewModel>>> ListLatestReleaseFeaturedTables(
            Guid publicationId)
        {
            return await GetLatestRelease(publicationId)
                .OnSuccess(release =>_releaseService.ListFeaturedTables(release.Id));
        }

        private async Task<Either<ActionResult, Release>> GetLatestRelease(Guid publicationId)
        {
            var release = _releaseRepository.GetLatestPublishedRelease(publicationId);

            if (release is null)
            {
                return new NotFoundResult();
            }

            return await Task.FromResult(release);
        }
    }
}
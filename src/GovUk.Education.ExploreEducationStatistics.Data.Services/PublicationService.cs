#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
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

        public async Task<Either<ActionResult, PublicationViewModel>> GetPublication(
            Guid publicationId)
        {
            var release = _releaseRepository.GetLatestPublishedRelease(publicationId);

            if (release == null)
            {
                return new NotFoundResult();
            }

            return await _releaseService.GetRelease(release.Id)
                .OnSuccess(
                    viewModel => new PublicationViewModel
                    {
                        Id = publicationId,
                        LatestReleaseId = release.Id,
                        Highlights = viewModel.Highlights,
                        Subjects = viewModel.Subjects,
                    }
                );
        }
    }
}
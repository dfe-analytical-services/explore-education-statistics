#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly IReleaseRepository _releaseRepository;

        public PublicationService(IReleaseRepository releaseRepository)
        {
            _releaseRepository = releaseRepository;
        }

        public Either<ActionResult, Release> GetLatestRelease(Guid publicationId)
        {
            var release = _releaseRepository.GetLatestPublishedRelease(publicationId);

            if (release is null)
            {
                return new NotFoundResult();
            }

            return release;
        }
    }
}
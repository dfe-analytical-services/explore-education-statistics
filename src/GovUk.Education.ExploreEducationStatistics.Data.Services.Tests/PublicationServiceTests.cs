#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class PublicationServiceTests
    {
        [Fact]
        public void GetLatestRelease()
        {
            var publicationId = Guid.NewGuid();
            var release = new Release();
            
            var (service, releaseRepository) = BuildServiceAndMocks();

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(publicationId))
                .Returns(release);
            
            var result = service.GetLatestRelease(publicationId);
            result.AssertRight(release);
        }
        
        [Fact]
        public void GetLatestRelease_NotFound()
        {
            var publicationId = Guid.NewGuid();
            
            var (service, releaseRepository) = BuildServiceAndMocks();

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(publicationId))
                .Returns((Release?)null);
            
            var result = service.GetLatestRelease(publicationId);
            result.AssertNotFound();
        }

        private static (PublicationService service, Mock<IReleaseRepository> releaseRepository) BuildServiceAndMocks()
        {
            var releaseRepository = new Mock<IReleaseRepository>(Strict);
            var controller = new PublicationService(releaseRepository.Object);
            return (controller, releaseRepository);
        }
    }
}

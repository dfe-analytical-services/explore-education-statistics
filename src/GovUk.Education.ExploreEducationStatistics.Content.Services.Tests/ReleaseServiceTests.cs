using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class ReleaseServiceTests
    {
        [Fact]
        public async Task Get()
        {
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            const string publicationPath = "publications/publication-a/publication.json";
            const string releasePath = "publications/publication-a/releases/2016.json";

            var methodology = new MethodologySummaryViewModel 
            {
                    Id = Guid.NewGuid(),
                    Slug = "methodology-slug",
                    Title = "Methodology"
            };

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
        
            SetupPublicationExpectations(fileStorageService, publicationId, releaseId, publicationPath);
            SetupReleaseExpectations(fileStorageService, releaseId, releasePath);

            methodologyService.Setup(mock => mock.GetSummariesByPublication(publicationId))
                .ReturnsAsync(AsList(methodology));

            var service = SetupReleaseService(
                fileStorageService: fileStorageService.Object,
                methodologyService: methodologyService.Object);
        
            var result = await service.Get(publicationPath, releasePath);
            var releaseViewModel = result.Right;

            var publication = releaseViewModel.Publication;
            Assert.Equal(publicationId, publication.Id);

            Assert.Single(publication.Methodologies);
            Assert.Equal(methodology, publication.Methodologies[0]);

            MockUtils.VerifyAllMocks(fileStorageService, methodologyService);
        }

        [Fact]
        public async Task Get_PublicationNotFound()
        {
            var releaseId = Guid.NewGuid();

            const string publicationPath = "publications/publication-a/publication.json";
            const string releasePath = "publications/publication-a/releases/2016.json";
            
            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
            
            fileStorageService
                .Setup(s => s.GetDeserialized<CachedPublicationViewModel>(publicationPath))
                    .ReturnsAsync(new NotFoundResult());

            SetupReleaseExpectations(fileStorageService, releaseId, releasePath);

            var service = SetupReleaseService(
                fileStorageService: fileStorageService.Object,
                methodologyService: methodologyService.Object);

            var result = await service.Get(publicationPath, releasePath);

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(fileStorageService, methodologyService);
        }

        [Fact]
        public async Task Get_ReleaseNotFound()
        {
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            const string publicationPath = "publications/publication-a/publication.json";
            const string releasePath = "publications/publication-a/releases/2016.json";
            
            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            SetupPublicationExpectations(fileStorageService, releaseId, releaseId, publicationPath);

            fileStorageService
                .Setup(s => s.GetDeserialized<CachedReleaseViewModel>(releasePath))
                .ReturnsAsync(new NotFoundResult());

            var service = SetupReleaseService(
                fileStorageService: fileStorageService.Object,
                methodologyService: methodologyService.Object);

            var result = await service.Get(publicationPath, releasePath);

            result.AssertNotFound();

            MockUtils.VerifyAllMocks(fileStorageService, methodologyService);
        }
        
        private static void SetupPublicationExpectations(Mock<IFileStorageService> fileStorageService,
            Guid publicationId,
            Guid releaseId,
            string publicationPath)
        {
            fileStorageService.Setup(
                    s => s.GetDeserialized<CachedPublicationViewModel>(
                        publicationPath
                    )
                )
                .ReturnsAsync(new CachedPublicationViewModel
                {
                    Id = publicationId,
                    Releases = new List<ReleaseTitleViewModel>
                    {
                        new ReleaseTitleViewModel
                        {
                            Id = releaseId
                        }
                    }
                });
        }

        private static void SetupReleaseExpectations(Mock<IFileStorageService> fileStorageService,
            Guid releaseId,
            string releasePath)
        {
            fileStorageService.Setup(
                    s => s.GetDeserialized<CachedReleaseViewModel>(
                        releasePath
                    )
                )
                .ReturnsAsync(new CachedReleaseViewModel
                {
                    Id = releaseId
                });
        }
        
        private static ReleaseService SetupReleaseService(
            IFileStorageService fileStorageService = null,
            IMethodologyService methodologyService = null)
        {
            return new ReleaseService(
                fileStorageService ?? new Mock<IFileStorageService>().Object,
                methodologyService ?? new Mock<IMethodologyService>().Object
            );
        }
    }
}

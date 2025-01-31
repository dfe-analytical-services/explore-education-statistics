using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class MethodologyServiceTests
    {
        private readonly DataFixture _dataFixture = new();

        [Fact]
        public async Task GetFiles()
        {
            var methodologyVersion = new MethodologyVersion();

            var imageFile1 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    Filename = "image1.png",
                    Type = Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    Filename = "image2.png",
                    Type = Image
                }
            };

            var otherFile = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2, otherFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext);

                var result = await service.GetFiles(methodologyVersion.Id, Image);

                Assert.Equal(2, result.Count);
                Assert.Equal(imageFile1.File.Id, result[0].Id);
                Assert.Equal(imageFile2.File.Id, result[1].Id);
            }
        }

        [Fact]
        public async Task GetLatestVersionByRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var methodologies = AsList(
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    PreviousVersionId = null,
                    PublishingStrategy = Immediately,
                    Status = Approved,
                    Version = 0
                },
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    PreviousVersionId = null,
                    PublishingStrategy = Immediately,
                    Status = Approved,
                    Version = 0
                });

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository
                .Setup(mock => mock.GetLatestVersionByPublication(releaseVersion.Release.PublicationId))
                .ReturnsAsync(methodologies);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext,
                    methodologyVersionRepository.Object);

                var result = await service.GetLatestVersionByRelease(releaseVersion);

                Assert.Equal(methodologies, result);
            }

            MockUtils.VerifyAllMocks(methodologyVersionRepository);
        }

        [Fact]
        public async Task IsBeingPublishedAlongsideRelease_NotApproved()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Draft,
            };

            await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
            var methodologyService = SetupMethodologyService(contentDbContext);
            var result = await methodologyService.IsBeingPublishedAlongsideRelease(
                methodologyVersion,
                new ReleaseVersion());
            Assert.False(result);
        }

        [Fact]
        public async Task IsBeingPublishedAlongsideRelease_Immediately()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = Immediately,
            };

            await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);

            publicationRepository.Setup(mock => mock.IsPublished(releaseVersion.Release.PublicationId))
                .ReturnsAsync(false);

            var methodologyService = SetupMethodologyService(contentDbContext,
                publicationRepository: publicationRepository.Object);
            var result = await methodologyService.IsBeingPublishedAlongsideRelease(
                methodologyVersion,
                releaseVersion);
            Assert.True(result);
        }

        [Fact]
        public async Task IsBeingPublishedAlongsideRelease_Immediately_PublicationAlreadyPublished()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = Immediately,
            };

            await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);

            publicationRepository.Setup(mock => mock.IsPublished(releaseVersion.Release.PublicationId))
                .ReturnsAsync(true);

            var methodologyService = SetupMethodologyService(contentDbContext,
                publicationRepository: publicationRepository.Object);
            var result = await methodologyService.IsBeingPublishedAlongsideRelease(
                methodologyVersion,
                releaseVersion);
            Assert.False(result);
        }

        [Fact]
        public async Task IsBeingPublishedAlongsideRelease_WithRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseVersionId = releaseVersion.Id,
            };

            await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);

            publicationRepository.Setup(mock => mock.IsPublished(releaseVersion.Release.PublicationId))
                .ReturnsAsync(false);

            var methodologyService = SetupMethodologyService(contentDbContext,
                publicationRepository: publicationRepository.Object);
            var result = await methodologyService.IsBeingPublishedAlongsideRelease(
                methodologyVersion,
                releaseVersion);
            Assert.True(result);
        }

        [Fact]
        public async Task IsBeingPublishedAlongsideRelease_WithRelease_IncorrectRelease()
        {
            ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease()
                    .WithPublication(_dataFixture.DefaultPublication()));

            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseVersionId = Guid.NewGuid(),
            };

            await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);

            publicationRepository.Setup(mock => mock.IsPublished(releaseVersion.Release.PublicationId))
                .ReturnsAsync(false);

            var methodologyService = SetupMethodologyService(contentDbContext,
                publicationRepository: publicationRepository.Object);
            var result = await methodologyService.IsBeingPublishedAlongsideRelease(
                methodologyVersion,
                releaseVersion);
            Assert.False(result);
        }

        private static MethodologyService SetupMethodologyService(
            ContentDbContext contentDbContext,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IPublicationRepository? publicationRepository = null)
        {
            return new(
                contentDbContext,
                methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(MockBehavior.Strict),
                publicationRepository ?? Mock.Of<IPublicationRepository>(MockBehavior.Strict));
        }
    }
}

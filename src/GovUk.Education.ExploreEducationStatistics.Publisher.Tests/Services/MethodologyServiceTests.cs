#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class MethodologyServiceTests
    {
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
            var release = new Release
            {
                Publication = new Publication
                {
                    Title = "Publication",
                    Slug = "publication-slug"
                },
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ1
            };

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
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestVersionByPublication(release.PublicationId))
                .ReturnsAsync(methodologies);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext,
                    methodologyVersionRepository.Object);

                var result = await service.GetLatestByRelease(release.Id);

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
                new Release());
            Assert.False(result);
        }

        [Fact]
        public async Task IsBeingPublishedAlongsideRelease_Immediately()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = Immediately,
            };

            var release = new Release
            {
                PublicationId = Guid.NewGuid(),
            };

            await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);

            publicationRepository.Setup(mock => mock.IsPublished(release.PublicationId))
                .ReturnsAsync(false);

            var methodologyService = SetupMethodologyService(contentDbContext,
                publicationRepository: publicationRepository.Object);
            var result = await methodologyService.IsBeingPublishedAlongsideRelease(
                methodologyVersion,
                release);
            Assert.True(result);
        }

        [Fact]
        public async Task IsBeingPublishedAlongsideRelease_Immediately_PublicationAlreadyPublished()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = Immediately,
            };

            var release = new Release
            {
                PublicationId = Guid.NewGuid(),
            };

            await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);

            publicationRepository.Setup(mock => mock.IsPublished(release.PublicationId))
                .ReturnsAsync(true);

            var methodologyService = SetupMethodologyService(contentDbContext,
                publicationRepository: publicationRepository.Object);
            var result = await methodologyService.IsBeingPublishedAlongsideRelease(
                methodologyVersion,
                release);
            Assert.False(result);
        }

        [Fact]
        public async Task IsBeingPublishedAlongsideRelease_WithRelease()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid(),
            };

            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = release.Id,
            };

            await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);

            publicationRepository.Setup(mock => mock.IsPublished(release.PublicationId))
                .ReturnsAsync(false);

            var methodologyService = SetupMethodologyService(contentDbContext,
                publicationRepository: publicationRepository.Object);
            var result = await methodologyService.IsBeingPublishedAlongsideRelease(
                methodologyVersion,
                release);
            Assert.True(result);
        }

        [Fact]
        public async Task IsBeingPublishedAlongsideRelease_WithRelease_IncorrectRelease()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid(),
            };

            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid(),
            };

            await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);

            publicationRepository.Setup(mock => mock.IsPublished(release.PublicationId))
                .ReturnsAsync(false);

            var methodologyService = SetupMethodologyService(contentDbContext,
                publicationRepository: publicationRepository.Object);
            var result = await methodologyService.IsBeingPublishedAlongsideRelease(
                methodologyVersion,
                release);
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

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
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
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
        public async Task GetLatestByRelease()
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
        public async Task SetPublishedDatesByPublication_PublishedMethodologyHasPublishedDateSet()
        {
            var publicationId = Guid.NewGuid();
            var published = DateTime.UtcNow;

            var methodologyVersion = new MethodologyVersion
            {
                Published = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersionByPublication(publicationId))
                .ReturnsAsync(AsList(methodologyVersion));

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext,
                    methodologyVersionRepository.Object);

                await service.SetPublishedDatesByPublication(publicationId, published);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var actual = await contentDbContext.MethodologyVersions.FindAsync(methodologyVersion.Id);
                Assert.NotNull(actual);
                Assert.Equal(published, actual.Published);
            }

            MockUtils.VerifyAllMocks(methodologyVersionRepository);
        }

        [Fact]
        public async Task SetPublishedDatesByPublication_MethodologyWithAPublishedDateRemainsUntouched()
        {
            var publicationId = Guid.NewGuid();

            var methodologyVersion = new MethodologyVersion
            {
                Published = DateTime.UtcNow.AddDays(-1),
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersionByPublication(publicationId))
                .ReturnsAsync(AsList(methodologyVersion));

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext,
                    methodologyVersionRepository.Object);

                await service.SetPublishedDatesByPublication(publicationId, DateTime.UtcNow);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var actual = await contentDbContext.MethodologyVersions.FindAsync(methodologyVersion.Id);
                Assert.NotNull(actual);
                Assert.Equal(methodologyVersion.Published, actual.Published);
            }

            MockUtils.VerifyAllMocks(methodologyVersionRepository);
        }

        private static MethodologyService SetupMethodologyService(ContentDbContext contentDbContext,
            IMethodologyVersionRepository methodologyVersionRepository = null)
        {
            return new(
                contentDbContext,
                methodologyVersionRepository ?? new Mock<IMethodologyVersionRepository>().Object);
        }
    }
}

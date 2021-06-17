using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository
{
    public class MethodologyRepositoryTests
    {
        [Fact]
        public async Task GetLatestPublishedByPublication()
        {
            var publication = new Publication();

            var methodology1 = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        Version = 0
                    },
                    new Methodology
                    {
                        Id = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                        PreviousVersionId = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        Version = 1
                    },
                    new Methodology
                    {
                        Id = Guid.Parse("9108ed11-ab53-4578-9e9a-f5cfc3443e66"),
                        PreviousVersionId = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                        PublishingStrategy = Immediately,
                        Status = Draft,
                        Version = 2
                    }
                )
            };

            var methodology2 = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.Parse("4baee814-4c36-4850-9dc7-cfdae6026815"),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        Version = 0
                    },
                    new Methodology
                    {
                        Id = Guid.Parse("2d6632b9-2480-4c34-b298-123064b6f04e"),
                        PreviousVersionId = Guid.Parse("4baee814-4c36-4850-9dc7-cfdae6026815"),
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        Version = 1
                    },
                    new Methodology
                    {
                        Id = Guid.Parse("f3aaa275-5f43-4a6d-b633-a9fa2333332a"),
                        PreviousVersionId = Guid.Parse("2d6632b9-2480-4c34-b298-123064b6f04e"),
                        PublishingStrategy = Immediately,
                        Status = Draft,
                        Version = 2
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.MethodologyParents.AddRangeAsync(methodology1, methodology2);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodology1,
                        Owner = true
                    },
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodology2,
                        Owner = false
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodology1, methodology2);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(AsList(methodology1, methodology2));

                var result = await service.GetLatestPublishedByPublication(publication.Id);

                // Check the result contains the latest versions of the methodologies
                Assert.Equal(2, result.Count);
                Assert.True(result.Exists(mv => mv.Id == methodology1.Versions[1].Id));
                Assert.True(result.Exists(mv => mv.Id == methodology2.Versions[1].Id));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication_MethodologyHasNoPublishedVersions()
        {
            var publication = new Publication();

            var methodology1 = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Draft,
                        Version = 0
                    }
                )
            };

            var methodology2 = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.Parse("4baee814-4c36-4850-9dc7-cfdae6026815"),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Draft,
                        Version = 0
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.MethodologyParents.AddRangeAsync(methodology1, methodology2);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodology1,
                        Owner = true
                    },
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodology2,
                        Owner = false
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodology1, methodology2);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(AsList(methodology1, methodology2));

                var result = await service.GetLatestPublishedByPublication(publication.Id);
                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication_MethodologyHasNoVersions()
        {
            var publication = new Publication();

            var methodology = new MethodologyParent
            {
                Versions = new List<Methodology>()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.MethodologyParents.AddAsync(methodology);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodology,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Attach(methodology);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(AsList(methodology));

                var result = await service.GetLatestPublishedByPublication(publication.Id);
                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication_PublicationNotFoundThrowsException()
        {
            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetLatestPublishedByPublication(Guid.NewGuid()));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication_PublicationHasNoMethodologies()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(new List<MethodologyParent>());

                var result = await service.GetLatestPublishedByPublication(publication.Id);

                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        private static MethodologyRepository BuildMethodologyRepository(ContentDbContext contentDbContext,
            IMethodologyParentRepository methodologyParentRepository = null)
        {
            return new MethodologyRepository(
                contentDbContext,
                methodologyParentRepository ?? new Mock<IMethodologyParentRepository>().Object);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
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
        public async Task CreateMethodologyForPublication()
        {
            var publication = new Publication
            {
                Title = "The Publication Title",
                Slug = "the-publication-slug"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            Guid methodologyId;

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);
                var methodology = await service.CreateMethodologyForPublication(publication.Id);
                await contentDbContext.SaveChangesAsync();
                methodologyId = methodology.Id;
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var methodology = await contentDbContext
                    .Methodologies
                    .Include(m => m.MethodologyParent)
                    .ThenInclude(p => p.Publications)
                    .ThenInclude(p => p.Publication)
                    .SingleAsync(m => m.Id == methodologyId);

                var savedPublication = await contentDbContext.Publications.SingleAsync(p => p.Id == publication.Id);

                Assert.Equal(Immediately, methodology.PublishingStrategy);
                Assert.NotNull(methodology.MethodologyParent);
                Assert.Single(methodology.MethodologyParent.Publications);
                Assert.Equal(savedPublication, methodology.MethodologyParent.Publications[0].Publication);
                Assert.Equal(savedPublication.Title, methodology.Title);
                Assert.Equal(savedPublication.Slug, methodology.Slug);
            }
        }

        [Fact]
        public async Task GetLatestByPublication()
        {
            var publication = new Publication();

            var methodologyParent1 = new MethodologyParent
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
                        Status = Draft,
                        Version = 1
                    }
                )
            };

            var methodologyParent2 = new MethodologyParent
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
                        Status = Draft,
                        Version = 1
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.MethodologyParents.AddRangeAsync(methodologyParent1, methodologyParent2);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodologyParent1,
                        Owner = true
                    },
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodologyParent2,
                        Owner = false
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodologyParent1, methodologyParent2);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(AsList(methodologyParent1, methodologyParent2));

                var result = await service.GetLatestByPublication(publication.Id);

                // Check the result contains the latest versions of the methodologies
                Assert.Equal(2, result.Count);
                Assert.True(result.Exists(mv => mv.Id == methodologyParent1.Versions[1].Id));
                Assert.True(result.Exists(mv => mv.Id == methodologyParent2.Versions[1].Id));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestByPublication_MethodologyHasNoVersions()
        {
            var publication = new Publication();

            var methodologyParent = new MethodologyParent
            {
                Versions = new List<Methodology>()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.MethodologyParents.AddAsync(methodologyParent);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodologyParent,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Attach(methodologyParent);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(AsList(methodologyParent));

                var result = await service.GetLatestByPublication(publication.Id);
                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestByPublication_PublicationNotFoundThrowsException()
        {
            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.GetLatestByPublication(Guid.NewGuid()));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestByPublication_PublicationHasNoMethodologies()
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

                var result = await service.GetLatestByPublication(publication.Id);

                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByMethodologyParent()
        {
            var previousVersion = new Methodology
            {
                Id = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new Methodology
            {
                Id = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                PreviousVersionId = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 1
            };

            var latestDraftVersion = new Methodology
            {
                Id = Guid.Parse("9108ed11-ab53-4578-9e9a-f5cfc3443e66"),
                PreviousVersionId = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 2
            };

            var methodologyParent = new MethodologyParent
            {
                Versions = AsList(previousVersion, latestPublishedVersion, latestDraftVersion)
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddAsync(methodologyParent);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = new Publication
                        {
                            Published = DateTime.UtcNow
                        },
                        MethodologyParent = methodologyParent,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodologyParent);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                var result = await service.GetLatestPublishedByMethodologyParent(methodologyParent.Id);

                Assert.Equal(latestPublishedVersion, result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByMethodologyParent_MethodologyParentHasNoPublishedVersions()
        {
            var methodologyParent = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        PublishingStrategy = Immediately,
                        Status = Draft
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddAsync(methodologyParent);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = new Publication
                        {
                            Published = DateTime.UtcNow
                        },
                        MethodologyParent = methodologyParent,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodologyParent);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                var result = await service.GetLatestPublishedByMethodologyParent(methodologyParent.Id);

                Assert.Null(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByMethodologyParent_MethodologyParentHasNoVersions()
        {
            var methodologyParent = new MethodologyParent
            {
                Versions = new List<Methodology>()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddAsync(methodologyParent);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = new Publication
                        {
                            Published = DateTime.UtcNow
                        },
                        MethodologyParent = methodologyParent,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodologyParent);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                var result = await service.GetLatestPublishedByMethodologyParent(methodologyParent.Id);

                Assert.Null(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByMethodologyParent_PublicationIsNotPublished()
        {
            var nonLivePublication = new Publication
            {
                Published = null
            };

            var methodologyParent = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        PublishingStrategy = Immediately,
                        Status = Approved,
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddAsync(methodologyParent);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = nonLivePublication,
                        MethodologyParent = methodologyParent,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodologyParent);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                var result = await service.GetLatestPublishedByMethodologyParent(methodologyParent.Id);

                Assert.Null(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByMethodologyParent_MethodologyParentNotFoundThrowsException()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.GetLatestPublishedByMethodologyParent(Guid.NewGuid()));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication()
        {
            var publication = new Publication
            {
                Published = DateTime.UtcNow
            };

            var methodologyParent1 = new MethodologyParent
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

            var methodologyParent2 = new MethodologyParent
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
                await contentDbContext.MethodologyParents.AddRangeAsync(methodologyParent1, methodologyParent2);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodologyParent1,
                        Owner = true
                    },
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodologyParent2,
                        Owner = false
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodologyParent1, methodologyParent2);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(AsList(methodologyParent1, methodologyParent2));

                var result = await service.GetLatestPublishedByPublication(publication.Id);

                // Check the result contains the latest versions of the methodologies
                Assert.Equal(2, result.Count);
                Assert.True(result.Exists(mv => mv.Id == methodologyParent1.Versions[1].Id));
                Assert.True(result.Exists(mv => mv.Id == methodologyParent2.Versions[1].Id));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication_PublicationIsNotPublished()
        {
            var nonLivePublication = new Publication
            {
                Published = null
            };

            var methodologyParent = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        PublishingStrategy = Immediately,
                        Status = Approved,
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(nonLivePublication);
                await contentDbContext.MethodologyParents.AddRangeAsync(methodologyParent);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = nonLivePublication,
                        MethodologyParent = methodologyParent,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodologyParent);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(nonLivePublication.Id))
                    .ReturnsAsync(AsList(methodologyParent));

                var result = await service.GetLatestPublishedByPublication(nonLivePublication.Id);

                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication_MethodologyParentHasNoPublishedVersions()
        {
            var publication = new Publication
            {
                Published = DateTime.UtcNow
            };

            var methodologyParent1 = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        PublishingStrategy = Immediately,
                        Status = Draft
                    }
                )
            };

            var methodologyParent2 = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        PublishingStrategy = Immediately,
                        Status = Draft
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.MethodologyParents.AddRangeAsync(methodologyParent1, methodologyParent2);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodologyParent1,
                        Owner = true
                    },
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodologyParent2,
                        Owner = false
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodologyParent1, methodologyParent2);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(AsList(methodologyParent1, methodologyParent2));

                var result = await service.GetLatestPublishedByPublication(publication.Id);
                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication_MethodologyParentHasNoVersions()
        {
            var publication = new Publication
            {
                Published = DateTime.UtcNow
            };

            var methodologyParent = new MethodologyParent
            {
                Versions = new List<Methodology>()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.MethodologyParents.AddAsync(methodologyParent);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        MethodologyParent = methodologyParent,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Attach(methodologyParent);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(AsList(methodologyParent));

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

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.GetLatestPublishedByPublication(Guid.NewGuid()));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication_PublicationHasNoMethodologies()
        {
            var publication = new Publication
            {
                Published = DateTime.UtcNow
            };

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

        [Fact]
        public async Task IsPubliclyAccessible_ApprovedAndPublishedImmediately()
        {
            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = Immediately
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Published = DateTime.UtcNow
                },
                MethodologyParent = new MethodologyParent
                {
                    Versions = AsList(methodology)
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                Assert.True(await service.IsPubliclyAccessible(methodology.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ApprovedAndScheduledWithLiveRelease()
        {
            var liveRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow
            };

            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = liveRelease,
                ScheduledWithReleaseId = liveRelease.Id
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Published = DateTime.UtcNow
                },
                MethodologyParent = new MethodologyParent
                {
                    Versions = AsList(methodology)
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(liveRelease);
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                Assert.True(await service.IsPubliclyAccessible(methodology.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_PublishedImmediatelyButNotApprovedIsNotAccessible()
        {
            var methodology = new Methodology
            {
                Status = Draft,
                PublishingStrategy = Immediately
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Published = DateTime.UtcNow
                },
                MethodologyParent = new MethodologyParent
                {
                    Versions = AsList(methodology)
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(methodology.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ScheduledWithLiveReleaseButNotApprovedIsNotAccessible()
        {
            var liveRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow
            };

            var methodology = new Methodology
            {
                Status = Draft,
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = liveRelease,
                ScheduledWithReleaseId = liveRelease.Id
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Published = DateTime.UtcNow
                },
                MethodologyParent = new MethodologyParent
                {
                    Versions = AsList(methodology)
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(liveRelease);
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(methodology.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ApprovedButScheduledWithNonLiveReleaseIsNotAccessible()
        {
            var nonLiveRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = nonLiveRelease,
                ScheduledWithReleaseId = nonLiveRelease.Id
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Published = DateTime.UtcNow
                },
                MethodologyParent = new MethodologyParent
                {
                    Versions = AsList(methodology)
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(nonLiveRelease);
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(methodology.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ApprovedAndPublishedImmediatelyHasCorrectLatestVersionAccessible()
        {
            var previousVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 1
            };

            var latestDraftVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = latestPublishedVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 2
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Published = DateTime.UtcNow
                },
                MethodologyParent = new MethodologyParent
                {
                    Versions = AsList(previousVersion, latestPublishedVersion, latestDraftVersion)
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(previousVersion.Id));
                Assert.True(await service.IsPubliclyAccessible(latestPublishedVersion.Id));
                Assert.False(await service.IsPubliclyAccessible(latestDraftVersion.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ApprovedAndScheduledWithLiveReleaseHasCorrectLatestVersionAccessible()
        {
            var liveRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow
            };

            var previousVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = liveRelease,
                ScheduledWithReleaseId = liveRelease.Id,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = liveRelease,
                ScheduledWithReleaseId = liveRelease.Id,
                Status = Approved,
                Version = 1
            };

            var latestDraftVersion = new Methodology
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = latestPublishedVersion.Id,
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = liveRelease,
                ScheduledWithReleaseId = liveRelease.Id,
                Status = Draft,
                Version = 2
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Published = DateTime.UtcNow
                },
                MethodologyParent = new MethodologyParent
                {
                    Versions = AsList(previousVersion, latestPublishedVersion, latestDraftVersion)
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(previousVersion.Id));
                Assert.True(await service.IsPubliclyAccessible(latestPublishedVersion.Id));
                Assert.False(await service.IsPubliclyAccessible(latestDraftVersion.Id));
            }
        }

        [Fact]
        public async Task
            IsPubliclyAccessible_ApprovedAndPublishedImmediatelyButPublicationNotPublishedIsNotAccessible()
        {
            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = Immediately
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Published = null
                },
                MethodologyParent = new MethodologyParent
                {
                    Versions = AsList(methodology)
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(methodology.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ScheduledWithReleaseNotFoundIsNotAccessible()
        {
            var methodology = new Methodology
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid()
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Published = null
                },
                MethodologyParent = new MethodologyParent
                {
                    Versions = AsList(methodology)
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                await Assert.ThrowsAsync<InvalidOperationException>(() => service.IsPubliclyAccessible(methodology.Id));
            }
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

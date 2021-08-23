#nullable enable
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

            var userId = Guid.NewGuid();

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
                var methodology = await service.CreateMethodologyForPublication(publication.Id, userId);
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
                Assert.NotNull(methodology.Created);
                Assert.InRange(DateTime.UtcNow.Subtract(methodology.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(userId, methodology.CreatedById);
            }
        }

        [Fact]
        public async Task GetLatestByPublication()
        {
            var publication = new Publication();

            var methodologyParent1 = new MethodologyParent
            {
                Versions = ListOf(
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
                Versions = ListOf(
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
                    .ReturnsAsync(ListOf(methodologyParent1, methodologyParent2));

                var result = await service.GetLatestByPublication(publication.Id);

                // Check the result contains the latest versions of the methodologies
                Assert.Equal(2, result.Count);
                Assert.True(result.Exists(mv => mv.Id == methodologyParent1.Versions[1].Id));
                Assert.True(result.Exists(mv => mv.Id == methodologyParent2.Versions[1].Id));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestByPublication_MethodologyHasNoVersionsThrowsException()
        {
            var publication = new Publication();

            var methodologyParent = new MethodologyParent();

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
                    .ReturnsAsync(ListOf(methodologyParent));

                await Assert.ThrowsAsync<ArgumentException>(
                    () => service.GetLatestByPublication(publication.Id));
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

            var methodologyParent = new MethodologyParent
            {
                Versions = ListOf(previousVersion, latestPublishedVersion, latestDraftVersion)
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
                        MethodologyParent = methodologyParent
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                var result = await service.GetLatestPublishedByMethodologyParent(methodologyParent.Id);

                Assert.NotNull(result);
                Assert.Equal(latestPublishedVersion.Id, result.Id);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByMethodologyParent_MethodologyParentHasNoPublishedVersions()
        {
            var methodologyParent = new MethodologyParent
            {
                Versions = ListOf(
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
                        MethodologyParent = methodologyParent
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
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
                        MethodologyParent = methodologyParent
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
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
                Versions = ListOf(
                    new Methodology
                    {
                        PublishingStrategy = Immediately,
                        Status = Approved
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
                        MethodologyParent = methodologyParent
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
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
                Versions = ListOf(
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
                Versions = ListOf(
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
                    .ReturnsAsync(ListOf(methodologyParent1, methodologyParent2));

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
                Versions = ListOf(
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
                        MethodologyParent = methodologyParent
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
                    .ReturnsAsync(ListOf(methodologyParent));

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
                Versions = ListOf(
                    new Methodology
                    {
                        PublishingStrategy = Immediately,
                        Status = Draft
                    }
                )
            };

            var methodologyParent2 = new MethodologyParent
            {
                Versions = ListOf(
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
                    .ReturnsAsync(ListOf(methodologyParent1, methodologyParent2));

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
                        MethodologyParent = methodologyParent
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
                    .ReturnsAsync(ListOf(methodologyParent));

                var result = await service.GetLatestPublishedByPublication(publication.Id);
                Assert.Empty(result);
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestPublishedByPublication_PublicationNotFoundIsEmpty()
        {
            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            methodologyParentRepository.Setup(mock => mock.GetByPublication(It.IsAny<Guid>()))
                .ReturnsAsync(new List<MethodologyParent>());

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                var result = await service.GetLatestPublishedByPublication(Guid.NewGuid());
                Assert.Empty(result);
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

                MockUtils.VerifyAllMocks(methodologyParentRepository);
            }
        }

        [Fact]
        public async Task GetOwningPublicationByMethodologyParent()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    Publications = ListOf(
                        new PublicationMethodology
                        {
                            Publication = new Publication
                            {
                                Title = "Adopting publication 1"
                            },
                            Owner = false
                        },
                        new PublicationMethodology
                        {
                            Publication = new Publication
                            {
                                Title = "Owning publication"
                            },
                            Owner = true
                        },
                        new PublicationMethodology
                        {
                            Publication = new Publication
                            {
                                Title = "Adopting publication 2"
                            },
                            Owner = false
                        }
                    )
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                var result = await service.GetOwningPublicationByMethodologyParent(methodology.MethodologyParentId);

                Assert.Equal("Owning publication", result.Title);
            }
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
                    Versions = ListOf(methodology)
                }
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
                    Versions = ListOf(methodology)
                }
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
                    Versions = ListOf(methodology)
                }
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
                    Versions = ListOf(methodology)
                }
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
                    Versions = ListOf(methodology)
                }
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
                    Versions = ListOf(previousVersion, latestPublishedVersion, latestDraftVersion)
                }
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
                    Versions = ListOf(previousVersion, latestPublishedVersion, latestDraftVersion)
                }
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
                    Versions = ListOf(methodology)
                }
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
                    Versions = ListOf(methodology)
                }
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

        [Fact]
        public async Task PublicationTitleChanged()
        {
            var publicationId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = new PublicationMethodology
                {
                    PublicationId = publicationId,
                    Owner = true,
                    MethodologyParent = new MethodologyParent
                    {
                        Versions = ListOf(new Methodology
                        {
                            Status = Draft
                        }),
                        Slug = "original-slug",
                        OwningPublicationTitle = "Original Title"
                    }
                };

                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.MethodologyParent)
                    .SingleAsync(m => m.PublicationId == publicationId);

                // As the Publication's Title and Slug changed and as this Methodology is not yet publicly accessible
                // and is still inheriting the Publication's Slug at this point, this change will be reflected in the
                // Methodology's Slug also.
                Assert.Equal("New Title", publicationMethodology.MethodologyParent.OwningPublicationTitle);
                Assert.Equal("new-slug", publicationMethodology.MethodologyParent.Slug);
            }
        }

        [Fact]
        public async Task PublicationTitleChanged_DoesNotAffectUnrelatedMethodologies()
        {
            var publicationId = Guid.NewGuid();
            var unrelatedPublicationId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = new PublicationMethodology
                {
                    PublicationId = publicationId,
                    Owner = true,
                    MethodologyParent = new MethodologyParent
                    {
                        Versions = ListOf(new Methodology
                        {
                            Status = Draft
                        }),
                        Slug = "original-slug",
                        OwningPublicationTitle = "Original Title"
                    }
                };

                var unrelatedPublicationMethodology = new PublicationMethodology
                {
                    PublicationId = unrelatedPublicationId,
                    Owner = true,
                    MethodologyParent = new MethodologyParent
                    {
                        Versions = ListOf(new Methodology
                        {
                            Status = Draft
                        }),
                        Slug = "original-slug",
                        OwningPublicationTitle = "Original Title"
                    }
                };

                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    publicationMethodology,
                    unrelatedPublicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.MethodologyParent)
                    .SingleAsync(m => m.PublicationId == unrelatedPublicationId);

                // This Methodology was not related to the Publication being updated, and so was not affected by the update.
                Assert.Equal("Original Title", publicationMethodology.MethodologyParent.OwningPublicationTitle);
                Assert.Equal("original-slug", publicationMethodology.MethodologyParent.Slug);
            }
        }

        [Fact]
        public async Task PublicationTitleChanged_DoesNotAffectUnownedMethodologies()
        {
            var publicationId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = new PublicationMethodology
                {
                    PublicationId = publicationId,
                    Owner = false,
                    MethodologyParent = new MethodologyParent
                    {
                        Versions = ListOf(new Methodology
                        {
                            Status = Draft
                        }),
                        Slug = "original-slug",
                        OwningPublicationTitle = "Original Title"
                    }
                };

                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.MethodologyParent)
                    .SingleAsync(m => m.PublicationId == publicationId);

                // This Methodology was not owned by the Publication being updated, and so was not affected by the update.
                Assert.Equal("Original Title", publicationMethodology.MethodologyParent.OwningPublicationTitle);
                Assert.Equal("original-slug", publicationMethodology.MethodologyParent.Slug);
            }
        }

        [Fact]
        public async Task PublicationTitleChanged_MethodologySlugHasAlreadyBeenAmendedByAlternativeTitle()
        {
            var publicationId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = new PublicationMethodology
                {
                    PublicationId = publicationId,
                    Owner = true,
                    MethodologyParent = new MethodologyParent
                    {
                        Versions = ListOf(new Methodology
                        {
                            Status = Draft
                        }),
                        Slug = "alternative-slug",
                        OwningPublicationTitle = "Original title"
                    }
                };

                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.MethodologyParent)
                    .SingleAsync(m => m.PublicationId == publicationId);

                // The Methodology has already had an alternative Slug set by virtue of one of its Versions'
                // Alternative Titles being updated, and so changing the Publication's Title and Slug won't override
                // this more specific Slug change.
                Assert.Equal("New Title", publicationMethodology.MethodologyParent.OwningPublicationTitle);
                Assert.Equal("alternative-slug", publicationMethodology.MethodologyParent.Slug);
            }
        }

        [Fact]
        public async Task PublicationTitleChanged_MethodologyAlreadyPubliclyAvailable()
        {
            var publicationId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = new PublicationMethodology
                {
                    Publication = new Publication
                    {
                        Id = publicationId,
                        Published = DateTime.UtcNow.AddDays(-1)
                    },
                    Owner = true,
                    MethodologyParent = new MethodologyParent
                    {
                        Versions = ListOf(new Methodology
                        {
                            Status = Approved,
                            PublishingStrategy = Immediately,
                        }),
                        Slug = "original-slug",
                        OwningPublicationTitle = "Original title"
                    }
                };

                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.MethodologyParent)
                    .SingleAsync(m => m.PublicationId == publicationId);

                // The Methodology that was attached to the updated Publication was publicly available, as it was linked
                // to a Publication that was "Live" and was set to be published "Immediately", and so its Slug does
                // not update.
                Assert.Equal("New Title", publicationMethodology.MethodologyParent.OwningPublicationTitle);
                Assert.Equal("original-slug", publicationMethodology.MethodologyParent.Slug);
            }
        }

        private static MethodologyRepository BuildMethodologyRepository(ContentDbContext contentDbContext,
            IMethodologyParentRepository? methodologyParentRepository = null)
        {
            return new(
                contentDbContext,
                methodologyParentRepository ?? Mock.Of<IMethodologyParentRepository>());
        }
    }
}

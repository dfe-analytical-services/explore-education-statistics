#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository
{
    public class MethodologyVersionRepositoryTests
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
                var service = BuildMethodologyVersionRepository(contentDbContext);
                var methodology = await service.CreateMethodologyForPublication(publication.Id, userId);
                await contentDbContext.SaveChangesAsync();
                methodologyId = methodology.Id;
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var methodology = await contentDbContext
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .ThenInclude(p => p.Publications)
                    .ThenInclude(p => p.Publication)
                    .SingleAsync(m => m.Id == methodologyId);

                var savedPublication = await contentDbContext.Publications.SingleAsync(p => p.Id == publication.Id);

                Assert.Equal(Immediately, methodology.PublishingStrategy);
                Assert.NotNull(methodology.Methodology);
                Assert.Single(methodology.Methodology.Publications);
                Assert.Equal(savedPublication, methodology.Methodology.Publications[0].Publication);
                Assert.Equal(savedPublication.Title, methodology.Title);
                Assert.Equal(savedPublication.Slug, methodology.Slug);
                Assert.NotNull(methodology.Created);
                Assert.InRange(DateTime.UtcNow.Subtract(methodology.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(userId, methodology.CreatedById);
            }
        }

        [Fact]
        public async Task GetLatestVersion()
        {
            var methodology = new Methodology();

            var version1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                Methodology = methodology,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var version2 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = version1.Id,
                Methodology = methodology,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 1
            };

            var version3 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = version2.Id,
                Methodology = methodology,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 2
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddRangeAsync(methodology);
                await contentDbContext.MethodologyVersions.AddRangeAsync(version1, version2, version3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext);

                var result = await service.GetLatestVersion(methodology.Id);

                // Check the result contains the latest versions of the methodologies
                Assert.Equal(version3.Id, result.Id);
            }
        }

        [Fact]
        public async Task GetLatestByPublication()
        {
            var publication = new Publication();

            var methodology1 = new Methodology();
            var methodology2 = new Methodology();

            var methodology1Version1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                Methodology = methodology1,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var methodology1Version2 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = methodology1Version1.Id,
                Methodology = methodology1,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 1
            };

            var methodology2Version1 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                Methodology = methodology2,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var methodology2Version2 = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = methodology2Version1.Id,
                Methodology = methodology2,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 1
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Methodologies.AddRangeAsync(methodology1, methodology2);
                await contentDbContext.MethodologyVersions.AddRangeAsync(methodology1Version1,
                    methodology1Version2, methodology2Version1, methodology2Version2);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        Methodology = methodology1,
                        Owner = true
                    },
                    new PublicationMethodology
                    {
                        Publication = publication,
                        Methodology = methodology2,
                        Owner = false
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodology1, methodology2);

                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                methodologyRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(ListOf(methodology1, methodology2));

                var result = await service.GetLatestVersionByPublication(publication.Id);

                VerifyAllMocks(methodologyRepository);

                // Check the result contains the latest versions of the methodologies
                Assert.Equal(2, result.Count);
                Assert.True(result.Exists(mv => mv.Id == methodology1Version2.Id));
                Assert.True(result.Exists(mv => mv.Id == methodology2Version2.Id));
            }
        }

        [Fact]
        public async Task GetLatestByPublication_MethodologyHasNoVersionsThrowsException()
        {
            var publication = new Publication();

            var methodology = new Methodology();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        Methodology = methodology,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Attach(methodology);

                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                methodologyRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(ListOf(methodology));

                await Assert.ThrowsAsync<ArgumentException>(
                    () => service.GetLatestVersionByPublication(publication.Id));
            }

            VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetLatestByPublication_PublicationNotFoundThrowsException()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.GetLatestVersionByPublication(Guid.NewGuid()));
            }

            VerifyAllMocks(methodologyRepository);
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

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                methodologyRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(new List<Methodology>());

                var result = await service.GetLatestVersionByPublication(publication.Id);

                VerifyAllMocks(methodologyRepository);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetLatestPublishedVersion()
        {
            var previousVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 1
            };

            var latestDraftVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = latestPublishedVersion.Id,
                PublishingStrategy = Immediately,
                Status = Draft,
                Version = 2
            };

            var methodology = new Methodology
            {
                Versions = ListOf(previousVersion, latestPublishedVersion, latestDraftVersion)
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = new Publication
                        {
                            LatestPublishedRelease = new Release()
                        },
                        Methodology = methodology
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetLatestPublishedVersion(methodology.Id);

                VerifyAllMocks(methodologyRepository);

                Assert.NotNull(result);
                Assert.Equal(latestPublishedVersion.Id, result!.Id);
            }
        }

        [Fact]
        public async Task GetLatestPublishedVersion_MethodologyHasNoPublishedVersions()
        {
            var methodology = new Methodology
            {
                Versions = ListOf(
                    new MethodologyVersion
                    {
                        PublishingStrategy = Immediately,
                        Status = Draft
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = new Publication
                        {
                            LatestPublishedRelease = new Release()
                        },
                        Methodology = methodology
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetLatestPublishedVersion(methodology.Id);

                VerifyAllMocks(methodologyRepository);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetLatestPublishedVersion_MethodologyHasNoVersions()
        {
            var methodology = new Methodology
            {
                Versions = new List<MethodologyVersion>()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = new Publication
                        {
                            LatestPublishedRelease = new Release()
                        },
                        Methodology = methodology
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetLatestPublishedVersion(methodology.Id);

                VerifyAllMocks(methodologyRepository);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetLatestPublishedVersion_PublicationIsNotPublished()
        {
            var nonLivePublication = new Publication
            {
                LatestPublishedRelease = null
            };

            var methodology = new Methodology
            {
                Versions = ListOf(
                    new MethodologyVersion
                    {
                        PublishingStrategy = Immediately,
                        Status = Approved
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = nonLivePublication,
                        Methodology = methodology
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetLatestPublishedVersion(methodology.Id);

                VerifyAllMocks(methodologyRepository);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetLatestPublishedVersion_MethodologyNotFoundThrowsException()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.GetLatestPublishedVersion(Guid.NewGuid()));
            }

            VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetLatestPublishedVersionByPublication()
        {
            var publication = new Publication
            {
                LatestPublishedRelease = new Release()
            };

            var methodology1 = new Methodology
            {
                Versions = ListOf(
                    new MethodologyVersion
                    {
                        Id = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        Version = 0
                    },
                    new MethodologyVersion
                    {
                        Id = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                        PreviousVersionId = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        Version = 1
                    },
                    new MethodologyVersion
                    {
                        Id = Guid.Parse("9108ed11-ab53-4578-9e9a-f5cfc3443e66"),
                        PreviousVersionId = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                        PublishingStrategy = Immediately,
                        Status = Draft,
                        Version = 2
                    }
                )
            };

            var methodology2 = new Methodology
            {
                Versions = ListOf(
                    new MethodologyVersion
                    {
                        Id = Guid.Parse("4baee814-4c36-4850-9dc7-cfdae6026815"),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        Version = 0
                    },
                    new MethodologyVersion
                    {
                        Id = Guid.Parse("2d6632b9-2480-4c34-b298-123064b6f04e"),
                        PreviousVersionId = Guid.Parse("4baee814-4c36-4850-9dc7-cfdae6026815"),
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        Version = 1
                    },
                    new MethodologyVersion
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
                await contentDbContext.Methodologies.AddRangeAsync(methodology1, methodology2);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        Methodology = methodology1,
                        Owner = true
                    },
                    new PublicationMethodology
                    {
                        Publication = publication,
                        Methodology = methodology2,
                        Owner = false
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodology1, methodology2);

                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                methodologyRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(ListOf(methodology1, methodology2));

                var result = await service.GetLatestPublishedVersionByPublication(publication.Id);

                VerifyAllMocks(methodologyRepository);

                // Check the result contains the latest versions of the methodologies
                Assert.Equal(2, result.Count);
                Assert.True(result.Exists(mv => mv.Id == methodology1.Versions[1].Id));
                Assert.True(result.Exists(mv => mv.Id == methodology2.Versions[1].Id));
            }
        }

        [Fact]
        public async Task GetLatestPublishedVersionByPublication_PublicationIsNotPublished()
        {
            var nonLivePublication = new Publication
            {
                LatestPublishedRelease = null
            };

            var methodology = new Methodology
            {
                Versions = ListOf(
                    new MethodologyVersion
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
                await contentDbContext.Methodologies.AddRangeAsync(methodology);
                await contentDbContext.PublicationMethodologies.AddAsync(
                    new PublicationMethodology
                    {
                        Publication = nonLivePublication,
                        Methodology = methodology
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodology);

                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                methodologyRepository.Setup(mock => mock.GetByPublication(nonLivePublication.Id))
                    .ReturnsAsync(ListOf(methodology));

                var result = await service.GetLatestPublishedVersionByPublication(nonLivePublication.Id);

                VerifyAllMocks(methodologyRepository);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetLatestPublishedVersionByPublication_MethodologyHasNoPublishedVersions()
        {
            var publication = new Publication
            {
                LatestPublishedRelease = new Release()
            };

            var methodology1 = new Methodology
            {
                Versions = ListOf(
                    new MethodologyVersion
                    {
                        PublishingStrategy = Immediately,
                        Status = Draft
                    }
                )
            };

            var methodology2 = new Methodology
            {
                Versions = ListOf(
                    new MethodologyVersion
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
                await contentDbContext.Methodologies.AddRangeAsync(methodology1, methodology2);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        Methodology = methodology1,
                        Owner = true
                    },
                    new PublicationMethodology
                    {
                        Publication = publication,
                        Methodology = methodology2,
                        Owner = false
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodology1, methodology2);

                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                methodologyRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(ListOf(methodology1, methodology2));

                var result = await service.GetLatestPublishedVersionByPublication(publication.Id);

                VerifyAllMocks(methodologyRepository);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetLatestPublishedVersionByPublication_MethodologyHasNoVersions()
        {
            var publication = new Publication
            {
                LatestPublishedRelease = new Release()
            };

            var methodology = new Methodology
            {
                Versions = new List<MethodologyVersion>()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(
                    new PublicationMethodology
                    {
                        Publication = publication,
                        Methodology = methodology
                    });
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Attach(methodology);

                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                methodologyRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(ListOf(methodology));

                var result = await service.GetLatestPublishedVersionByPublication(publication.Id);

                VerifyAllMocks(methodologyRepository);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetLatestPublishedVersionByPublication_PublicationNotFoundIsEmpty()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            methodologyRepository.Setup(mock => mock.GetByPublication(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Methodology>());

            await using var contentDbContext = InMemoryContentDbContext();
            var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                methodologyRepository: methodologyRepository.Object);

            var result = await service.GetLatestPublishedVersionByPublication(Guid.NewGuid());

            VerifyAllMocks(methodologyRepository);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLatestPublishedVersionByPublication_PublicationHasNoMethodologies()
        {
            var publication = new Publication
            {
                LatestPublishedRelease = new Release()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyVersionRepository(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                methodologyRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(new List<Methodology>());

                var result = await service.GetLatestPublishedVersionByPublication(publication.Id);

                VerifyAllMocks(methodologyRepository);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ApprovedAndPublishedImmediately()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = Immediately
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(methodologyVersion)
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.True(await service.IsPubliclyAccessible(methodologyVersion.Id));
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

            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = liveRelease.Id
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(methodologyVersion)
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.True(await service.IsPubliclyAccessible(methodologyVersion.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_PublishedImmediatelyButNotApprovedIsNotAccessible()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Draft,
                PublishingStrategy = Immediately
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(methodologyVersion)
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(methodologyVersion.Id));
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

            var methodologyVersion = new MethodologyVersion
            {
                Status = Draft,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = liveRelease.Id
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(methodologyVersion)
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(methodologyVersion.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ApprovedButScheduledWithNonLiveReleaseIsNotAccessible()
        {
            var nonLiveRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = nonLiveRelease.Id
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(methodologyVersion)
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(methodologyVersion.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ApprovedAndPublishedImmediatelyIsAccessibleIfNextVersionIsDraft()
        {
            var previousVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 1
            };

            var latestDraftVersion = new MethodologyVersion
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
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(previousVersion.Id));
                Assert.True(await service.IsPubliclyAccessible(latestPublishedVersion.Id));
                Assert.False(await service.IsPubliclyAccessible(latestDraftVersion.Id));
            }
        }

        [Fact]
        public async Task
            IsPubliclyAccessible_ApprovedAndPublishedImmediatelyIsAccessibleIfNextVersionIsScheduledWithNonLiveRelease()
        {
            var nonLiveRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var previousVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 1
            };

            var latestApprovedVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = latestPublishedVersion.Id,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = nonLiveRelease.Id,
                Status = Approved,
                Version = 2
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(previousVersion, latestPublishedVersion, latestApprovedVersion)
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(previousVersion.Id));
                Assert.True(await service.IsPubliclyAccessible(latestPublishedVersion.Id));
                Assert.False(await service.IsPubliclyAccessible(latestApprovedVersion.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ApprovedAndScheduledWithLiveReleaseIsAccessibleIfNextVersionIsDraft()
        {
            var liveRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow
            };

            var previousVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = liveRelease.Id,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = liveRelease.Id,
                Status = Approved,
                Version = 1
            };

            var latestDraftVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = latestPublishedVersion.Id,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = liveRelease.Id,
                Status = Draft,
                Version = 2
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(previousVersion, latestPublishedVersion, latestDraftVersion)
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(previousVersion.Id));
                Assert.True(await service.IsPubliclyAccessible(latestPublishedVersion.Id));
                Assert.False(await service.IsPubliclyAccessible(latestDraftVersion.Id));
            }
        }

        [Fact]
        public async Task
            IsPubliclyAccessible_ApprovedAndScheduledWithLiveReleaseIsAccessibleIfNextVersionIsScheduledWithNonLiveRelease()
        {
            var liveRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow
            };

            var nonLiveRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var previousVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Version = 0
            };

            var latestPublishedVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousVersion.Id,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = liveRelease.Id,
                Status = Approved,
                Version = 1
            };

            var latestApprovedVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = latestPublishedVersion.Id,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = nonLiveRelease.Id,
                Status = Approved,
                Version = 2
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(previousVersion, latestPublishedVersion, latestApprovedVersion)
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(liveRelease, nonLiveRelease);
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(previousVersion.Id));
                Assert.True(await service.IsPubliclyAccessible(latestPublishedVersion.Id));
                Assert.False(await service.IsPubliclyAccessible(latestApprovedVersion.Id));
            }
        }

        [Fact]
        public async Task
            IsPubliclyAccessible_ApprovedAndPublishedImmediatelyButPublicationNotPublishedIsNotAccessible()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = Immediately
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = null
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(methodologyVersion)
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                Assert.False(await service.IsPubliclyAccessible(methodologyVersion.Id));
            }
        }

        [Fact]
        public async Task IsPubliclyAccessible_ScheduledWithReleaseNotFoundIsNotAccessible()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid()
            };

            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    LatestPublishedRelease = new Release()
                },
                Methodology = new Methodology
                {
                    Versions = ListOf(methodologyVersion)
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
                var service = BuildMethodologyVersionRepository(contentDbContext);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.IsPubliclyAccessible(methodologyVersion.Id));
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
                    Methodology = new Methodology
                    {
                        Versions = ListOf(new MethodologyVersion
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
                var service = BuildMethodologyVersionRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.PublicationId == publicationId);

                // As the Publication's Title and Slug changed and as this Methodology is not yet publicly accessible
                // and is still inheriting the Publication's Slug at this point, this change will be reflected in the
                // Methodology's Slug also.
                Assert.Equal("New Title", publicationMethodology.Methodology.OwningPublicationTitle);
                Assert.Equal("new-slug", publicationMethodology.Methodology.Slug);
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
                    Methodology = new Methodology
                    {
                        Versions = ListOf(new MethodologyVersion
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
                    Methodology = new Methodology
                    {
                        Versions = ListOf(new MethodologyVersion
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
                var service = BuildMethodologyVersionRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.PublicationId == unrelatedPublicationId);

                // This Methodology was not related to the Publication being updated, and so was not affected by the update.
                Assert.Equal("Original Title", publicationMethodology.Methodology.OwningPublicationTitle);
                Assert.Equal("original-slug", publicationMethodology.Methodology.Slug);
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
                    Methodology = new Methodology
                    {
                        Versions = ListOf(new MethodologyVersion
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
                var service = BuildMethodologyVersionRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.PublicationId == publicationId);

                // This Methodology was not owned by the Publication being updated, and so was not affected by the update.
                Assert.Equal("Original Title", publicationMethodology.Methodology.OwningPublicationTitle);
                Assert.Equal("original-slug", publicationMethodology.Methodology.Slug);
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
                    Methodology = new Methodology
                    {
                        Versions = ListOf(new MethodologyVersion
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
                var service = BuildMethodologyVersionRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.PublicationId == publicationId);

                // The Methodology has already had an alternative Slug set by virtue of one of its Versions'
                // Alternative Titles being updated, and so changing the Publication's Title and Slug won't override
                // this more specific Slug change.
                Assert.Equal("New Title", publicationMethodology.Methodology.OwningPublicationTitle);
                Assert.Equal("alternative-slug", publicationMethodology.Methodology.Slug);
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
                        LatestPublishedRelease = new Release()
                    },
                    Owner = true,
                    Methodology = new Methodology
                    {
                        Versions = ListOf(new MethodologyVersion
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
                var service = BuildMethodologyVersionRepository(contentDbContext);
                await service.PublicationTitleChanged(publicationId, "original-slug", "New Title", "new-slug");
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var publicationMethodology = await contentDbContext
                    .PublicationMethodologies
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.PublicationId == publicationId);

                // The Methodology that was attached to the updated Publication was publicly available, as it was linked
                // to a Publication that was "Live" and was set to be published "Immediately", and so its Slug does
                // not update.
                Assert.Equal("New Title", publicationMethodology.Methodology.OwningPublicationTitle);
                Assert.Equal("original-slug", publicationMethodology.Methodology.Slug);
            }
        }

        private static MethodologyVersionRepository BuildMethodologyVersionRepository(
            ContentDbContext contentDbContext,
            IMethodologyRepository? methodologyRepository = null)
        {
            return new(
                contentDbContext,
                methodologyRepository ?? Mock.Of<IMethodologyRepository>(Strict));
        }
    }
}

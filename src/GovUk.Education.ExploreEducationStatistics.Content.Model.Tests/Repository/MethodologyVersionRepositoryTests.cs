using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository;

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

        Guid methodologyVersionId;

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(contentDbContext);
            var methodologyVersion = await service.CreateMethodologyForPublication(publication.Id, userId);
            await contentDbContext.SaveChangesAsync();
            methodologyVersionId = methodologyVersion.Id;
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var methodologyVersion = await contentDbContext
                .MethodologyVersions
                .Include(m => m.Methodology)
                .ThenInclude(p => p.Publications)
                .ThenInclude(p => p.Publication)
                .SingleAsync(m => m.Id == methodologyVersionId);

            var savedPublication = await contentDbContext.Publications.SingleAsync(p => p.Id == publication.Id);

            Assert.Equal(Immediately, methodologyVersion.PublishingStrategy);
            Assert.NotNull(methodologyVersion.Methodology);
            Assert.Single(methodologyVersion.Methodology.Publications);
            Assert.Equal(savedPublication, methodologyVersion.Methodology.Publications[0].Publication);
            Assert.Equal(savedPublication.Title, methodologyVersion.Title);
            Assert.Equal(savedPublication.Slug, methodologyVersion.Slug);
            methodologyVersion.Created.AssertUtcNow();
            Assert.Equal(userId, methodologyVersion.CreatedById);
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
    public async Task GetLatestPublishedVersionBySlug()
    {
        var methodologyVersionId = Guid.NewGuid();
        var methodologyVersion = new MethodologyVersion
        {
            Id = methodologyVersionId,
            Methodology = new Methodology
            {
                OwningPublicationSlug = "publication-slug",
                LatestPublishedVersionId = methodologyVersionId,
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(
                contentDbContext: contentDbContext);

            var result = await service.GetLatestPublishedVersionBySlug(methodologyVersion.Slug);

            Assert.NotNull(result);
            Assert.Equal(methodologyVersionId, result.Id);
            Assert.Equal(methodologyVersion.Methodology.OwningPublicationSlug, result.Slug);
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
            LatestPublishedVersion = latestPublishedVersion,
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
                        LatestPublishedReleaseVersion = new ReleaseVersion()
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
            Assert.Equal(latestPublishedVersion.Id, result.Id);
        }
    }

    [Fact]
    public async Task GetLatestPublishedVersion_MethodologyHasNoPublishedVersions()
    {
        var methodology = new Methodology
        {
            LatestPublishedVersionId = null,
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
                        LatestPublishedReleaseVersion = new ReleaseVersion()
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
            LatestPublishedVersionId = null,
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
                        LatestPublishedReleaseVersion = new ReleaseVersion()
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
    public async Task GetLatestPublishedVersion_MethodologyHasNoLatestPublishedVersion()
    {
        var methodology = new Methodology
        {
            LatestPublishedVersionId = null,
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
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.PublicationMethodologies.AddAsync(
                new PublicationMethodology
                {
                    PublicationId = Guid.NewGuid(),
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
            LatestPublishedReleaseVersion = new ReleaseVersion()
        };

        var methodology1 = new Methodology
        {
            LatestPublishedVersionId = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
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
            LatestPublishedVersionId = Guid.Parse("2d6632b9-2480-4c34-b298-123064b6f04e"),
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
    public async Task GetLatestPublishedVersionByPublication_MethodologyHasNoPublishedVersions()
    {
        var publication = new Publication
        {
            LatestPublishedReleaseVersion = new ReleaseVersion()
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
            LatestPublishedVersionId = null,
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
            LatestPublishedReleaseVersion = new ReleaseVersion()
        };

        var methodology = new Methodology
        {
            LatestPublishedVersionId = null,
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
            LatestPublishedReleaseVersion = new ReleaseVersion()
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
    public async Task GetLatestPublishedVersionBySlug_AlternativeSlug()
    {
        var latestPublishedVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            LatestPublishedVersionId = latestPublishedVersionId,
            OwningPublicationSlug = "not-like-this",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    AlternativeSlug = "not-like-this",
                    Version = 1,
                    PreviousVersionId = latestPublishedVersionId,
                },
                new()
                {
                    Id = latestPublishedVersionId,
                    AlternativeSlug = "slug",
                    Version = 0,
                },
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(
                contentDbContext: contentDbContext);

            var result = await service
                .GetLatestPublishedVersionBySlug("slug");

            VerifyAllMocks(methodologyRepository);

            Assert.NotNull(result);
            Assert.Equal(latestPublishedVersionId, result.Id);
            Assert.Equal("slug", result.Slug);
        }
    }

    [Fact]
    public async Task GetLatestPublishedVersionBySlug_OwningPublicationSlug()
    {
        var latestPublishedVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            LatestPublishedVersionId = latestPublishedVersionId,
            OwningPublicationSlug = "slug",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    AlternativeSlug = "not-like-this",
                    Version = 1,
                    PreviousVersionId = latestPublishedVersionId,
                },
                new()
                {
                    Id = latestPublishedVersionId,
                    AlternativeSlug = null,
                    Version = 0,
                },
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(
                contentDbContext: contentDbContext);

            var result = await service
                .GetLatestPublishedVersionBySlug("slug");

            VerifyAllMocks(methodologyRepository);

            Assert.NotNull(result);
            Assert.Equal(latestPublishedVersionId, result.Id);
            Assert.Null(result.AlternativeSlug);
            // doesn't return result.Methodology, so cannot check result.Slug/result.Methodology.OwningPublicationSlug
        }
    }

    [Fact]
    public async Task GetLatestPublishedVersionBySlug_UnpublishedMethodology()
    {
        var methodology = new Methodology
        {
            LatestPublishedVersionId = null,
            OwningPublicationSlug = "owning-publication-slug",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    AlternativeSlug = "alternative-slug",
                    Version = 0,
                },
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Methodologies.AddAsync(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(
                contentDbContext: contentDbContext);

            var result = await service
                .GetLatestPublishedVersionBySlug("slug");

            VerifyAllMocks(methodologyRepository);

            Assert.Null(result);
        }
    }

    [Fact]
    public async Task IsToBePublished_ApprovedAndPublishedImmediately()
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
                LatestPublishedReleaseVersion = new ReleaseVersion()
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

            await contentDbContext.Entry(methodologyVersion)
                .ReloadAsync();

            Assert.True(await service.IsToBePublished(methodologyVersion));
        }
    }

    [Fact]
    public async Task IsToBePublished_ApprovedAndScheduledWithLiveRelease()
    {
        var liveReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow
        };

        var methodologyVersion = new MethodologyVersion
        {
            Status = Approved,
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = liveReleaseVersion.Id
        };

        var publicationMethodology = new PublicationMethodology
        {
            Publication = new Publication
            {
                LatestPublishedReleaseVersion = new ReleaseVersion()
            },
            Methodology = new Methodology
            {
                Versions = ListOf(methodologyVersion)
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(liveReleaseVersion);
            contentDbContext.PublicationMethodologies.Add(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(contentDbContext);

            await contentDbContext.Entry(methodologyVersion)
                .ReloadAsync();

            Assert.True(await service.IsToBePublished(methodologyVersion));
        }
    }

    [Fact]
    public async Task IsToBePublished_PublishedImmediatelyButNotApprovedIsNotAccessible()
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
                LatestPublishedReleaseVersion = new ReleaseVersion()
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

            Assert.False(await service.IsToBePublished(methodologyVersion));
        }
    }

    [Fact]
    public async Task IsToBePublished_ScheduledWithLiveReleaseButNotApprovedIsNotAccessible()
    {
        var liveReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow
        };

        var methodologyVersion = new MethodologyVersion
        {
            Status = Draft,
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = liveReleaseVersion.Id
        };

        var publicationMethodology = new PublicationMethodology
        {
            Publication = new Publication
            {
                LatestPublishedReleaseVersion = new ReleaseVersion()
            },
            Methodology = new Methodology
            {
                Versions = ListOf(methodologyVersion)
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(liveReleaseVersion);
            contentDbContext.PublicationMethodologies.Add(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(contentDbContext);

            Assert.False(await service.IsToBePublished(methodologyVersion));
        }
    }

    [Fact]
    public async Task IsToBePublished_ApprovedButScheduledWithNonLiveReleaseIsNotAccessible()
    {
        var nonLiveReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid()
        };

        var methodologyVersion = new MethodologyVersion
        {
            Status = Approved,
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = nonLiveReleaseVersion.Id
        };

        var publicationMethodology = new PublicationMethodology
        {
            Publication = new Publication
            {
                LatestPublishedReleaseVersion = new ReleaseVersion()
            },
            Methodology = new Methodology
            {
                Versions = ListOf(methodologyVersion)
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(nonLiveReleaseVersion);
            contentDbContext.PublicationMethodologies.Add(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(contentDbContext);

            await contentDbContext.Entry(methodologyVersion)
                .ReloadAsync();

            Assert.False(await service.IsToBePublished(methodologyVersion));
        }
    }

    [Fact]
    public async Task IsToBePublished_ApprovedAndPublishedImmediatelyIsAccessibleIfNextVersionIsDraft()
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
                LatestPublishedReleaseVersion = new ReleaseVersion()
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

            await contentDbContext.Entry(previousVersion)
                .ReloadAsync();
            await contentDbContext.Entry(latestPublishedVersion)
                .ReloadAsync();
            await contentDbContext.Entry(latestDraftVersion)
                .ReloadAsync();

            Assert.False(await service.IsToBePublished(previousVersion));
            Assert.True(await service.IsToBePublished(latestPublishedVersion));
            Assert.False(await service.IsToBePublished(latestDraftVersion));
        }
    }

    [Fact]
    public async Task
        IsToBePublished_ApprovedAndPublishedImmediatelyIsAccessibleIfNextVersionIsScheduledWithNonLiveRelease()
    {
        var nonLiveReleaseVersion = new ReleaseVersion
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
            ScheduledWithReleaseVersionId = nonLiveReleaseVersion.Id,
            Status = Approved,
            Version = 2
        };

        var publicationMethodology = new PublicationMethodology
        {
            Publication = new Publication
            {
                LatestPublishedReleaseVersion = new ReleaseVersion()
            },
            Methodology = new Methodology
            {
                Versions = ListOf(previousVersion, latestPublishedVersion, latestApprovedVersion)
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(nonLiveReleaseVersion);
            contentDbContext.PublicationMethodologies.Add(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(contentDbContext);

            await contentDbContext.Entry(previousVersion)
                .ReloadAsync();
            await contentDbContext.Entry(latestPublishedVersion)
                .ReloadAsync();
            await contentDbContext.Entry(latestApprovedVersion)
                .ReloadAsync();

            Assert.False(await service.IsToBePublished(previousVersion));
            Assert.True(await service.IsToBePublished(latestPublishedVersion));
            Assert.False(await service.IsToBePublished(latestApprovedVersion));
        }
    }

    [Fact]
    public async Task IsToBePublished_ApprovedAndScheduledWithLiveReleaseIsAccessibleIfNextVersionIsDraft()
    {
        var liveReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow
        };

        var previousVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PreviousVersionId = null,
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = liveReleaseVersion.Id,
            Status = Approved,
            Version = 0
        };

        var latestPublishedVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PreviousVersionId = previousVersion.Id,
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = liveReleaseVersion.Id,
            Status = Approved,
            Version = 1
        };

        var latestDraftVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PreviousVersionId = latestPublishedVersion.Id,
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = liveReleaseVersion.Id,
            Status = Draft,
            Version = 2
        };

        var publicationMethodology = new PublicationMethodology
        {
            Publication = new Publication
            {
                LatestPublishedReleaseVersion = new ReleaseVersion()
            },
            Methodology = new Methodology
            {
                Versions = ListOf(previousVersion, latestPublishedVersion, latestDraftVersion)
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(liveReleaseVersion);
            contentDbContext.PublicationMethodologies.Add(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(contentDbContext);

            await contentDbContext.Entry(previousVersion)
                .ReloadAsync();
            await contentDbContext.Entry(latestPublishedVersion)
                .ReloadAsync();
            await contentDbContext.Entry(latestDraftVersion)
                .ReloadAsync();

            Assert.False(await service.IsToBePublished(previousVersion));
            Assert.True(await service.IsToBePublished(latestPublishedVersion));
            Assert.False(await service.IsToBePublished(latestDraftVersion));
        }
    }

    [Fact]
    public async Task
        IsToBePublished_ApprovedAndScheduledWithLiveReleaseIsAccessibleIfNextVersionIsScheduledWithNonLiveRelease()
    {
        var liveReleaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            Published = DateTime.UtcNow
        };

        var nonLiveReleaseVersion = new ReleaseVersion
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
            ScheduledWithReleaseVersionId = liveReleaseVersion.Id,
            Status = Approved,
            Version = 1
        };

        var latestApprovedVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PreviousVersionId = latestPublishedVersion.Id,
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = nonLiveReleaseVersion.Id,
            Status = Approved,
            Version = 2
        };

        var publicationMethodology = new PublicationMethodology
        {
            Publication = new Publication
            {
                LatestPublishedReleaseVersion = new ReleaseVersion()
            },
            Methodology = new Methodology
            {
                Versions = ListOf(previousVersion, latestPublishedVersion, latestApprovedVersion)
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(liveReleaseVersion, nonLiveReleaseVersion);
            contentDbContext.PublicationMethodologies.Add(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildMethodologyVersionRepository(contentDbContext);

            await contentDbContext.Entry(previousVersion)
                .ReloadAsync();
            await contentDbContext.Entry(latestPublishedVersion)
                .ReloadAsync();
            await contentDbContext.Entry(latestApprovedVersion)
                .ReloadAsync();

            Assert.False(await service.IsToBePublished(previousVersion));
            Assert.True(await service.IsToBePublished(latestPublishedVersion));
            Assert.False(await service.IsToBePublished(latestApprovedVersion));
        }
    }

    [Fact]
    public async Task
        IsToBePublished_ApprovedAndPublishedImmediatelyButPublicationNotPublishedIsNotAccessible()
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
                LatestPublishedReleaseVersion = null
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

            await contentDbContext.Entry(methodologyVersion)
                .ReloadAsync();

            Assert.False(await service.IsToBePublished(methodologyVersion));
        }
    }

    [Fact]
    public async Task IsToBePublished_ScheduledWithReleaseNotFoundIsNotAccessible()
    {
        var methodologyVersion = new MethodologyVersion
        {
            Status = Approved,
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = Guid.NewGuid()
        };

        var publicationMethodology = new PublicationMethodology
        {
            Publication = new Publication
            {
                LatestPublishedReleaseVersion = new ReleaseVersion()
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
                service.IsToBePublished(methodologyVersion));
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

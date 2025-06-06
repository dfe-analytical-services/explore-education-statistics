#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies;

public class MethodologyServiceTests
{
    private readonly DataFixture _dataFixture = new();

    private static readonly User User = new()
    {
        Id = Guid.NewGuid()
    };

    private static readonly Contact MockContact = new()
    {
        TeamName = "Mock Team Name",
        TeamEmail = "mockteam@mockteam.com",
        ContactName = "Mock Contact Name",
    };

    private static readonly Publication MockPublication = new()
    {
        Title = "Test publication",
        Slug = "test-publication",
        LatestPublishedReleaseVersion = new ReleaseVersion
        {
            Release = new Release
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                PublicationId = Guid.NewGuid(),
                Year = 2021,
                Slug = "latest-release-slug"
            }
        },
        Contact = MockContact
    };

    [Fact]
    public async Task AdoptMethodology()
    {
        var publication = new Publication();

        // Setup methodology owned by a different publication
        var methodology = new Methodology
        {
            LatestPublishedVersionId = Guid.NewGuid(),
            Publications = new List<PublicationMethodology>
            {
                new()
                {
                    Publication = new Publication(),
                    Owner = true
                }
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.Contacts.AddAsync(MockContact);
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);

        methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
            .ReturnsAsync(
                new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                    new List<AllMethodologiesThemeViewModel>()));

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context,
                methodologyCacheService: methodologyCacheService.Object);

            var result = await service.AdoptMethodology(publication.Id, methodology.Id);

            VerifyAllMocks(methodologyCacheService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationMethodologies = await context.PublicationMethodologies
                .AsQueryable()
                .ToListAsync();

            // Check the existing and new relationships between publications and methodologies
            Assert.Equal(2, publicationMethodologies.Count);
            Assert.True(publicationMethodologies.Exists(pm => pm.MethodologyId == methodology.Id
                                                              && pm.PublicationId != publication.Id
                                                              && pm.Owner));
            Assert.True(publicationMethodologies.Exists(pm => pm.MethodologyId == methodology.Id
                                                              && pm.PublicationId == publication.Id
                                                              && !pm.Owner));
        }
    }

    [Fact]
    public async Task AdoptMethodology_CannotAdoptUnpublishedMethodology()
    {
        var publication = new Publication();

        // Setup methodology owned by a different publication
        var methodology = new Methodology
        {
            LatestPublishedVersionId = null, // methodology is unpublished
            Publications = new List<PublicationMethodology>
            {
                new()
                {
                    Publication = new Publication(),
                    Owner = true
                }
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.Contacts.AddAsync(MockContact);
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AdoptMethodology(publication.Id, methodology.Id));
            Assert.Equal("Cannot adopt an unpublished methodology", exception.Message);
        }
    }

    [Fact]
    public async Task AdoptMethodology_AlreadyAdoptedByPublicationFails()
    {
        var publication = new Publication();

        // Setup methodology adopted by this publication
        var methodology = new Methodology
        {
            LatestPublishedVersionId = Guid.NewGuid(),
            Publications = new List<PublicationMethodology>
            {
                new()
                {
                    Publication = new Publication(),
                    Owner = true
                },
                new()
                {
                    Publication = publication,
                    Owner = false
                }
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.Contacts.AddAsync(MockContact);
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context);

            var result = await service.AdoptMethodology(publication.Id, methodology.Id);

            result.AssertBadRequest(CannotAdoptMethodologyAlreadyLinkedToPublication);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationMethodologies = await context.PublicationMethodologies
                .AsQueryable()
                .ToListAsync();

            // Check the relationships between publications and methodologies are not altered
            Assert.Equal(2, publicationMethodologies.Count);
            Assert.True(publicationMethodologies.Exists(pm => pm.MethodologyId == methodology.Id
                                                              && pm.PublicationId != publication.Id
                                                              && pm.Owner));
            Assert.True(publicationMethodologies.Exists(pm => pm.MethodologyId == methodology.Id
                                                              && pm.PublicationId == publication.Id
                                                              && !pm.Owner));
        }
    }

    [Fact]
    public async Task AdoptMethodology_AdoptingOwnedMethodologyFails()
    {
        var publication = new Publication();

        // Setup methodology owned by this publication
        var methodology = new Methodology
        {
            LatestPublishedVersionId = Guid.NewGuid(),
            Publications = new List<PublicationMethodology>
            {
                new()
                {
                    Publication = publication,
                    Owner = true
                }
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.Contacts.AddAsync(MockContact);
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context);

            var result = await service.AdoptMethodology(publication.Id, methodology.Id);

            result.AssertBadRequest(CannotAdoptMethodologyAlreadyLinkedToPublication);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationMethodologies = await context.PublicationMethodologies
                .AsQueryable()
                .ToListAsync();

            // Check the relationships between publications and methodologies are not altered
            Assert.Single(publicationMethodologies);
            Assert.True(publicationMethodologies.Exists(pm => pm.MethodologyId == methodology.Id
                                                              && pm.PublicationId == publication.Id
                                                              && pm.Owner));
        }
    }

    [Fact]
    public async Task AdoptMethodology_PublicationNotFound()
    {
        var methodology = new Methodology
        {
            Publications = new List<PublicationMethodology>
            {
                new()
                {
                    Publication = new Publication(),
                    Owner = true
                }
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context);

            var result = await service.AdoptMethodology(Guid.NewGuid(), methodology.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task AdoptMethodology_MethodologyNotFound()
    {
        var publication = new Publication();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.Contacts.AddAsync(MockContact);
            await context.SaveChangesAsync();
        }

        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context);

            var result = await service.AdoptMethodology(publication.Id, Guid.NewGuid());

            VerifyAllMocks(methodologyRepository);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task CreateMethodology()
    {
        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(MockPublication);
            await context.Contacts.AddAsync(MockContact);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            var service = SetupMethodologyService(
                contentDbContext: context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var createdMethodology = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                Methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    OwningPublicationSlug = MockPublication.Slug,
                    OwningPublicationTitle = MockPublication.Title,
                    Publications = new List<PublicationMethodology>
                    {
                        new()
                        {
                            Owner = true,
                            Publication = MockPublication
                        }
                    }
                },
                Status = Draft
            };

            methodologyVersionRepository
                .Setup(s => s.CreateMethodologyForPublication(MockPublication.Id, User.Id))
                .ReturnsAsync(createdMethodology);

            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug(MockPublication.Slug))
                .ReturnsAsync((MethodologyVersion?)null);

            context.Attach(createdMethodology);

            var viewModel = (await service.CreateMethodology(MockPublication.Id)).AssertRight();
            VerifyAllMocks(methodologyVersionRepository);

            Assert.Equal(createdMethodology.Id, viewModel.Id);
            Assert.Equal("test-publication", viewModel.Slug);
            Assert.False(viewModel.Amendment);
            Assert.Null(viewModel.InternalReleaseNote);
            Assert.Equal(createdMethodology.Methodology.Id, viewModel.MethodologyId);
            Assert.Null(viewModel.Published);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, viewModel.PublishingStrategy);
            Assert.Equal(Draft, viewModel.Status);
            Assert.Equal("Test publication", viewModel.Title);

            Assert.Equal(MockPublication.Id, viewModel.OwningPublication.Id);
            Assert.Equal("Test publication", viewModel.OwningPublication.Title);
            Assert.Equal(MockPublication.LatestPublishedReleaseVersion!.Release.Slug, viewModel.OwningPublication.LatestReleaseSlug);
            Assert.Empty(viewModel.OtherPublications);
        }
    }

    [Fact]
    public async Task CreateMethodology_MethodologySlugNotUnique()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication",
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug(publication.Slug))
                .ReturnsAsync(new MethodologyVersion());

            var service = SetupMethodologyService(
                contentDbContext: context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var createdMethodology = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                Methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    OwningPublicationTitle = publication.Title,
                    OwningPublicationSlug = publication.Slug,
                    Publications = new List<PublicationMethodology>
                    {
                        new()
                        {
                            Owner = true,
                            Publication = publication
                        }
                    }
                },
                Status = Draft
            };

            context.Attach(createdMethodology);

            var actionResult = (await service.CreateMethodology(publication.Id)).AssertLeft();

            VerifyAllMocks(methodologyVersionRepository);

            actionResult.AssertValidationProblem(MethodologySlugNotUnique);
        }
    }

    [Fact]
    public async Task DropMethodology()
    {
        var publication = new Publication();

        // Setup methodology adopted by this publication
        var methodology = new Methodology
        {
            Publications = new List<PublicationMethodology>
            {
                new()
                {
                    Publication = new Publication(),
                    Owner = true
                },
                new()
                {
                    Publication = publication,
                    Owner = false
                }
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.Contacts.AddAsync(MockContact);
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);

        methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
            .ReturnsAsync(
                new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                    new List<AllMethodologiesThemeViewModel>()));

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context,
                methodologyCacheService: methodologyCacheService.Object);

            var result = await service.DropMethodology(publication.Id, methodology.Id);

            VerifyAllMocks(methodologyCacheService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationMethodologies = await context.PublicationMethodologies
                .AsQueryable()
                .ToListAsync();

            // Check the adopting relationship is removed
            Assert.Single(publicationMethodologies);
            Assert.False(publicationMethodologies.Exists(pm => pm.PublicationId == publication.Id));
        }
    }

    [Fact]
    public async Task DropMethodology_DropMethodologyNotAdoptedFails()
    {
        var publication = new Publication();

        // Setup methodology which is not adopted by this publication
        var methodology = new Methodology
        {
            Publications = new List<PublicationMethodology>
            {
                new()
                {
                    Publication = new Publication(),
                    Owner = true
                },
                new()
                {
                    Publication = new Publication(),
                    Owner = false
                }
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.Contacts.AddAsync(MockContact);
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context);

            var result = await service.DropMethodology(publication.Id, methodology.Id);

            result.AssertNotFound();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationMethodologies = await context.PublicationMethodologies
                .AsQueryable()
                .ToListAsync();

            // Check the relationships between publications and methodologies are not altered
            Assert.Equal(2, publicationMethodologies.Count);
            Assert.True(publicationMethodologies.Exists(pm => pm.MethodologyId == methodology.Id
                                                              && pm.PublicationId != publication.Id
                                                              && pm.Owner));
            Assert.True(publicationMethodologies.Exists(pm => pm.MethodologyId == methodology.Id
                                                              && pm.PublicationId != publication.Id
                                                              && !pm.Owner));
        }
    }

    [Fact]
    public async Task DropMethodology_PublicationNotFound()
    {
        var methodology = new Methodology
        {
            Publications = new List<PublicationMethodology>
            {
                new()
                {
                    Publication = new Publication(),
                    Owner = true
                }
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context);

            var result = await service.DropMethodology(Guid.NewGuid(), methodology.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task DropMethodology_MethodologyNotFound()
    {
        var publication = new Publication();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.Contacts.AddAsync(MockContact);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context);

            var result = await service.DropMethodology(publication.Id, Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetAdoptableMethodologies()
    {
        var methodology = new Methodology
        {
            LatestPublishedVersionId = Guid.NewGuid(),
            OwningPublicationTitle = "Owning publication",
            OwningPublicationSlug = "owning-publication",
        };

        var publication = new Publication
        {
            Title = "Owning publication",
            Slug = "owning-publication",
            LatestPublishedReleaseVersion = new ReleaseVersion
            {
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug"
                }
            },
            Methodologies = new List<PublicationMethodology>
            {
                new()
                {
                    Methodology = methodology,
                    Owner = true
                }
            },
            Contact = MockContact
        };

        var methodologyVersion = new MethodologyVersion
        {
            Methodology = methodology,
            Published = null,
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            AlternativeTitle = "Alternative title",
            AlternativeSlug = "alternative-slug",
        };

        var methodologyStatus = new MethodologyStatus
        {
            MethodologyVersion = methodologyVersion,
            InternalReleaseNote = "Test approval",
            ApprovalStatus = Approved,
        };

        var adoptingPublication = new Publication
        {
            Contact = MockContact
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddRangeAsync(publication, adoptingPublication);
            await context.Contacts.AddAsync(MockContact);
            await context.Methodologies.AddAsync(methodology);
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.MethodologyStatus.AddAsync(methodologyStatus);
            await context.SaveChangesAsync();
        }

        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

        methodologyRepository.Setup(mock =>
                mock.GetPublishedMethodologiesUnrelatedToPublication(adoptingPublication.Id))
            .ReturnsAsync(ListOf(methodology));

        methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersion(methodology.Id))
            .ReturnsAsync(methodologyVersion);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context,
                methodologyRepository: methodologyRepository.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var result = (await service.GetAdoptableMethodologies(adoptingPublication.Id)).AssertRight();

            VerifyAllMocks(methodologyRepository, methodologyVersionRepository);

            Assert.Single(result);

            var viewModel = result[0];

            Assert.Equal(methodologyVersion.Id, viewModel.Id);
            Assert.False(viewModel.Amendment);
            Assert.Equal("Test approval", viewModel.InternalReleaseNote);
            Assert.Equal(methodologyVersion.MethodologyId, viewModel.MethodologyId);
            Assert.Null(viewModel.Published);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, viewModel.PublishingStrategy);
            Assert.Equal(Draft, viewModel.Status);
            Assert.Equal("Alternative title", viewModel.Title);
            Assert.Equal("alternative-slug", viewModel.Slug);

            Assert.Equal(publication.Id, viewModel.OwningPublication.Id);
            Assert.Equal("Owning publication", viewModel.OwningPublication.Title);
            Assert.Equal("latest-release-slug", viewModel.OwningPublication.LatestReleaseSlug);
            Assert.Empty(viewModel.OtherPublications);
        }
    }

    [Fact]
    public async Task GetAdoptableMethodologies_NoUnpublishedMethodologies()
    {
        var methodology = new Methodology
        {
            LatestPublishedVersionId = null, // methodology is unpublished
            OwningPublicationSlug = "test-publication",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Published = null,
                    PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                    Status = Draft,
                    AlternativeTitle = "Alternative title"
                },
            },
        };

        var publication = new Publication
        {
            Title = "Owning publication",
            Methodologies = new List<PublicationMethodology>
            {
                new()
                {
                    Methodology = methodology,
                    Owner = true
                }
            }
        };

        var adoptingPublication = new Publication();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddRangeAsync(publication, adoptingPublication);
            await context.Contacts.AddAsync(MockContact);
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
        methodologyRepository.Setup(mock =>
                mock.GetPublishedMethodologiesUnrelatedToPublication(adoptingPublication.Id))
            .ReturnsAsync(new List<Methodology>());

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context,
                methodologyRepository: methodologyRepository.Object);

            var result = await service.GetAdoptableMethodologies(adoptingPublication.Id);
            var adoptableMethodologyList = result.AssertRight();

            VerifyAllMocks(methodologyRepository);

            Assert.Empty(adoptableMethodologyList);
        }
    }

    [Fact]
    public async Task GetAdoptableMethodologies_NoUnrelatedMethodologies()
    {
        var publication = new Publication();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.Contacts.AddAsync(MockContact);
            await context.SaveChangesAsync();
        }

        var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

        methodologyRepository.Setup(mock =>
                mock.GetPublishedMethodologiesUnrelatedToPublication(publication.Id))
            .ReturnsAsync(new List<Methodology>());

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(
                contentDbContext: context,
                methodologyRepository: methodologyRepository.Object);

            var result = (await service.GetAdoptableMethodologies(publication.Id)).AssertRight();

            VerifyAllMocks(methodologyRepository);

            Assert.Empty(result);
        }
    }

    [Fact]
    public async Task GetMethodology()
    {
        Publication owningPublication = _dataFixture.DefaultPublication()
            .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var (adoptingPublication1, adoptingPublication2) = _dataFixture.DefaultPublication()
            .WithReleases(_ =>
            [
                _dataFixture.DefaultRelease(publishedVersions: 1, year: 2020)
            ])
            .GenerateTuple2();

        Methodology methodology = _dataFixture.DefaultMethodology()
            .WithOwningPublication(owningPublication)
            .WithAdoptingPublications([adoptingPublication1, adoptingPublication2])
            .WithMethodologyVersions(_ => [
                _dataFixture.DefaultMethodologyVersion()
                    .WithAlternativeSlug("alternative-title")
                    .WithAlternativeTitle("Alternative title")
                    .WithApprovalStatus(Approved)
                    .WithPublished(new DateTime(2020, 5, 25))
                    .WithPublishingStrategy(MethodologyPublishingStrategy.WithRelease)
                    .WithScheduledWithReleaseVersion(owningPublication.Releases[0].Versions[0])
            ]);

        var methodologyStatus = new MethodologyStatus
        {
            MethodologyVersionId = methodology.Versions[0].Id,
            InternalReleaseNote = "Test approval",
            ApprovalStatus = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            context.Methodologies.Add(methodology);
            context.MethodologyStatus.Add(methodologyStatus);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext: context);

            var result = await service.GetMethodology(methodology.Versions[0].Id);

            var viewModel = result.AssertRight();

            Assert.Equal(methodology.Versions[0].Id, viewModel.Id);
            Assert.Equal("Alternative title", viewModel.Title);
            Assert.Equal("alternative-title", viewModel.Slug);
            Assert.False(viewModel.Amendment);
            Assert.Equal(methodologyStatus.InternalReleaseNote, viewModel.InternalReleaseNote);
            Assert.Equal(methodology.Id, viewModel.MethodologyId);
            Assert.Equal(methodology.Versions[0].Published, viewModel.Published);
            Assert.Equal(MethodologyPublishingStrategy.WithRelease, viewModel.PublishingStrategy);
            Assert.Equal(methodology.Versions[0].Status, viewModel.Status);

            Assert.Equal(owningPublication.Id, viewModel.OwningPublication.Id);
            Assert.Equal(owningPublication.Title, viewModel.OwningPublication.Title);
                
            Assert.Equal(2, viewModel.OtherPublications.Count);
            Assert.Equal(adoptingPublication1.Id, viewModel.OtherPublications[0].Id);
            Assert.Equal(adoptingPublication1.Title, viewModel.OtherPublications[0].Title);
            Assert.Equal(adoptingPublication2.Id, viewModel.OtherPublications[1].Id);
            Assert.Equal(adoptingPublication2.Title, viewModel.OtherPublications[1].Title);

            Assert.NotNull(viewModel.ScheduledWithRelease);
            Assert.Equal(methodology.Versions[0].ScheduledWithReleaseVersionId, viewModel.ScheduledWithRelease!.Id);
            Assert.Equal($"{owningPublication.Title} - {owningPublication.Releases[0].Title}", viewModel.ScheduledWithRelease.Title);
        }
    }

    [Fact]
    public async Task GetUnpublishedReleasesUsingMethodology()
    {
        // Set up a randomly ordered mix of published and unpublished Releases on owning and adopting publications
        Publication owningPublication = _dataFixture.DefaultPublication()
            .WithReleases(_ => [
                _dataFixture.DefaultRelease(publishedVersions: 1, year: 2018),
                _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021),
                _dataFixture.DefaultRelease(publishedVersions: 1, year: 2019),
                _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2020),
            ]);

        Publication adoptingPublication = _dataFixture.DefaultPublication()
            .WithReleases(_ => [
                _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2018),
                _dataFixture.DefaultRelease(publishedVersions: 1, year: 2021),
                _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2019),
                _dataFixture.DefaultRelease(publishedVersions: 1, year: 2020),
            ]);

        Methodology methodology = _dataFixture.DefaultMethodology()
            .WithOwningPublication(owningPublication)
            .WithAdoptingPublications([adoptingPublication])
            .WithMethodologyVersions(_ => [_dataFixture.DefaultMethodologyVersion()]);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Methodologies.Add(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext: contentDbContext);

            var result = await service.GetUnpublishedReleasesUsingMethodology(methodology.Versions[0].Id);

            var viewModel = result.AssertRight();

            // Check that only unpublished Releases are included and that they are in the correct order

            var expectedReleaseVersionAtIndex0 =
                owningPublication.Releases.Single(r => r.Year == 2021).Versions[0];
            var expectedReleaseVersionAtIndex1 =
                owningPublication.Releases.Single(r => r.Year == 2020).Versions[0];
            var expectedReleaseVersionAtIndex2 =
                adoptingPublication.Releases.Single(r => r.Year == 2019).Versions[0];
            var expectedReleaseVersionAtIndex3 =
                adoptingPublication.Releases.Single(r => r.Year == 2018).Versions[0];

            Assert.Equal(4, viewModel.Count);

            Assert.Equal(expectedReleaseVersionAtIndex0.Id, viewModel[0].Id);
            Assert.Equal(
                $"{expectedReleaseVersionAtIndex0.Release.Publication.Title} - {expectedReleaseVersionAtIndex0.Release.Title}",
                viewModel[0].Title);

            Assert.Equal(expectedReleaseVersionAtIndex1.Id, viewModel[1].Id);
            Assert.Equal(
                $"{expectedReleaseVersionAtIndex1.Release.Publication.Title} - {expectedReleaseVersionAtIndex1.Release.Title}",
                viewModel[1].Title);

            Assert.Equal(expectedReleaseVersionAtIndex2.Id, viewModel[2].Id);
            Assert.Equal(
                $"{expectedReleaseVersionAtIndex2.Release.Publication.Title} - {expectedReleaseVersionAtIndex2.Release.Title}",
                viewModel[2].Title);

            Assert.Equal(expectedReleaseVersionAtIndex3.Id, viewModel[3].Id);
            Assert.Equal(
                $"{expectedReleaseVersionAtIndex3.Release.Publication.Title} - {expectedReleaseVersionAtIndex3.Release.Title}",
                viewModel[3].Title);
        }
    }

    [Fact]
    public async Task GetUnpublishedReleasesUsingMethodology_MethodologyNotFound()
    {
        await using var contentDbContext = InMemoryApplicationDbContext();

        var service = SetupMethodologyService(contentDbContext: contentDbContext);

        var result = await service.GetUnpublishedReleasesUsingMethodology(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetUnpublishedReleasesUsingMethodology_PublicationsHaveNoReleases()
    {
        Methodology methodology = _dataFixture.DefaultMethodology()
            .WithOwningPublication(_dataFixture.DefaultPublication())
            .WithAdoptingPublications(_ => [_dataFixture.DefaultPublication()])
            .WithMethodologyVersions(_ => [_dataFixture.DefaultMethodologyVersion()]);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Methodologies.Add(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext: contentDbContext);

            var result = await service.GetUnpublishedReleasesUsingMethodology(methodology.Versions[0].Id);

            var viewModel = result.AssertRight();

            Assert.Empty(viewModel);
        }
    }

    [Fact]
    public async Task GetUnpublishedReleasesUsingMethodology_PublicationsHaveNoUnpublishedReleases()
    {
        Methodology methodology = _dataFixture.DefaultMethodology()
            .WithOwningPublication(_dataFixture.DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]))
            .WithAdoptingPublications(_ => [_dataFixture.DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])])
            .WithMethodologyVersions(_ => [_dataFixture.DefaultMethodologyVersion()]);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Methodologies.Add(methodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext: contentDbContext);

            var result = await service.GetUnpublishedReleasesUsingMethodology(methodology.Versions[0].Id);

            var viewModel = result.AssertRight();

            Assert.Empty(viewModel);
        }
    }

    [Fact]
    public async Task ListLatestMethodologyVersions()
    {
        var methodology1 = new Methodology
        {
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 0,
                    AlternativeTitle = "Methodology 1 Version 1",
                    Published = new DateTime(2021, 1, 1),
                    Status = Approved,
                }
            }
        };

        var methodology2 = new Methodology
        {
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 0,
                    AlternativeTitle = "Methodology 2 Version 1",
                    Published = new DateTime(2021, 1, 1),
                    Status = Approved,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 1,
                    AlternativeTitle = "Methodology 2 Version 2",
                    Published = null,
                    Status = Draft,
                }
            }
        };
        methodology2.Versions[1].PreviousVersion = methodology2.Versions[0];

        var methodology3 = new Methodology
        {
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 0,
                    AlternativeTitle = "Methodology 3 Version 1",
                    Published = new DateTime(2021, 1, 1),
                    Status = Approved,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 1,
                    AlternativeTitle = "Methodology 3 Version 2",
                    Published = new DateTime(2022, 1, 1),
                    Status = Approved,
                }
            }
        };
        methodology3.Versions[1].PreviousVersion = methodology3.Versions[0];

        var methodology4 = new Methodology
        {
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 0,
                    AlternativeTitle = "Methodology 4 Version 1",
                    Published = new DateTime(2021, 1, 1),
                    Status = Draft,
                    PreviousVersion = null,
                },
            }
        };

        var publication = new Publication
        {
            Methodologies = new List<PublicationMethodology>
            {
                new()
                {
                    Owner = false,
                    Methodology = methodology2,
                },
                new()
                {
                    Owner = true,
                    Methodology = methodology1,
                },
                new()
                {
                    Owner = false,
                    Methodology = methodology3,
                },
                new()
                {
                    Owner = true,
                    Methodology = methodology4,
                },
            },
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.Contacts.AddAsync(MockContact);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupMethodologyService(contentDbContext);

            var result = await service.ListLatestMethodologyVersions(
                publication.Id);
            var viewModels = result.AssertRight();

            // Check that the latest versions of the methodologies are returned in title order
            Assert.Equal(4, viewModels.Count);

            Assert.Equal(methodology1.Versions[0].Id, viewModels[0].Id);
            Assert.False(viewModels[0].Amendment);
            Assert.True(viewModels[0].Owned);
            Assert.Equal(new DateTime(2021, 1, 1), viewModels[0].Published);
            Assert.Equal(Approved, viewModels[0].Status);
            Assert.Equal("Methodology 1 Version 1", viewModels[0].Title);
            Assert.Equal(methodology1.Id, viewModels[0].MethodologyId);
            Assert.Null(viewModels[0].PreviousVersionId);

            Assert.Equal(methodology2.Versions[1].Id, viewModels[1].Id);
            Assert.True(viewModels[1].Amendment);
            Assert.False(viewModels[1].Owned);
            Assert.Null(viewModels[1].Published);
            Assert.Equal(Draft, viewModels[1].Status);
            Assert.Equal("Methodology 2 Version 2", viewModels[1].Title);
            Assert.Equal(methodology2.Id, viewModels[1].MethodologyId);
            Assert.Equal(methodology2.Versions[0].Id, viewModels[1].PreviousVersionId);

            Assert.Equal(methodology3.Versions[1].Id, viewModels[2].Id);
            Assert.False(viewModels[2].Amendment);
            Assert.False(viewModels[2].Owned);
            Assert.Equal(new DateTime(2022, 1, 1), viewModels[2].Published);
            Assert.Equal(Approved, viewModels[2].Status);
            Assert.Equal("Methodology 3 Version 2", viewModels[2].Title);
            Assert.Equal(methodology3.Id, viewModels[2].MethodologyId);
            Assert.Equal(methodology3.Versions[0].Id, viewModels[2].PreviousVersionId);

            Assert.Equal(methodology4.Versions[0].Id, viewModels[3].Id);
            Assert.False(viewModels[3].Amendment);
            Assert.True(viewModels[3].Owned);
            Assert.Equal(new DateTime(2021, 1, 1), viewModels[3].Published);
            Assert.Equal(Draft, viewModels[3].Status);
            Assert.Equal("Methodology 4 Version 1", viewModels[3].Title);
            Assert.Equal(methodology4.Id, viewModels[3].MethodologyId);
            Assert.Null(viewModels[3].PreviousVersionId);
        }
    }

    [Fact]
    public async Task ListLatestMethodologyVersions_IsPrerelease()
    {
        var methodology1 = new Methodology
        {
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    // Version in result because latest is approved
                    Id = Guid.NewGuid(),
                    Version = 0,
                    AlternativeTitle = "Methodology 1 Version 1",
                    Published = new DateTime(2021, 1, 1),
                    Status = Approved,
                }
            }
        };

        var methodology2 = new Methodology
        {
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    // This version is in results because latest is draft, so previous is used
                    Id = Guid.NewGuid(),
                    Version = 0,
                    AlternativeTitle = "Methodology 2 Version 1",
                    Published = new DateTime(2021, 1, 1),
                    Status = Approved,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 1,
                    AlternativeTitle = "Methodology 2 Version 2",
                    Published = null,
                    Status = Draft,
                }
            }
        };
        methodology2.Versions[1].PreviousVersion = methodology2.Versions[0];

        var methodology3 = new Methodology
        {
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 0,
                    AlternativeTitle = "Methodology 3 Version 1",
                    Published = new DateTime(2021, 1, 1),
                    Status = Approved,
                },
                new()
                {
                    // This is in results because it is latest approved
                    Id = Guid.NewGuid(),
                    Version = 1,
                    AlternativeTitle = "Methodology 3 Version 2",
                    Published = new DateTime(2022, 1, 1),
                    Status = Approved,
                }
            }
        };
        methodology3.Versions[1].PreviousVersion = methodology3.Versions[0];

        var methodology4 = new Methodology
        {
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    // Not in results because draft and no previous
                    Id = Guid.NewGuid(),
                    Version = 0,
                    AlternativeTitle = "Methodology 4 Version 1",
                    Published = new DateTime(2021, 1, 1),
                    Status = Draft,
                    PreviousVersion = null,
                },
            }
        };

        var publication = new Publication
        {
            Methodologies = new List<PublicationMethodology>
            {
                new()
                {
                    Owner = false,
                    Methodology = methodology2,
                },
                new()
                {
                    Owner = true,
                    Methodology = methodology1,
                },
                new()
                {
                    Owner = false,
                    Methodology = methodology3,
                },
                new()
                {
                    Owner = true,
                    Methodology = methodology4,
                },
            },
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.Contacts.AddAsync(MockContact);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupMethodologyService(contentDbContext);

            var result = await service.ListLatestMethodologyVersions(
                publication.Id, isPrerelease: true);
            var viewModels = result.AssertRight();

            // Check that the latest versions of the methodologies are returned in title order
            Assert.Equal(3, viewModels.Count);

            Assert.Equal(methodology1.Versions[0].Id, viewModels[0].Id);
            Assert.False(viewModels[0].Amendment);
            Assert.True(viewModels[0].Owned);
            Assert.Equal(new DateTime(2021, 1, 1), viewModels[0].Published);
            Assert.Equal(Approved, viewModels[0].Status);
            Assert.Equal("Methodology 1 Version 1", viewModels[0].Title);
            Assert.Equal(methodology1.Id, viewModels[0].MethodologyId);
            Assert.Null(viewModels[0].PreviousVersionId);

            Assert.Equal(methodology2.Versions[0].Id, viewModels[1].Id);
            Assert.False(viewModels[1].Amendment);
            Assert.False(viewModels[1].Owned);
            Assert.Equal(new DateTime(2021, 1, 1), viewModels[1].Published);
            Assert.Equal(Approved, viewModels[1].Status);
            Assert.Equal("Methodology 2 Version 1", viewModels[1].Title);
            Assert.Equal(methodology2.Id, viewModels[1].MethodologyId);
            Assert.Null(viewModels[1].PreviousVersionId);

            Assert.Equal(methodology3.Versions[1].Id, viewModels[2].Id);
            Assert.False(viewModels[2].Amendment);
            Assert.False(viewModels[2].Owned);
            Assert.Equal(new DateTime(2022, 1, 1), viewModels[2].Published);
            Assert.Equal(Approved, viewModels[2].Status);
            Assert.Equal("Methodology 3 Version 2", viewModels[2].Title);
            Assert.Equal(methodology3.Id, viewModels[2].MethodologyId);
            Assert.Equal(methodology3.Versions[0].Id, viewModels[2].PreviousVersionId);
        }
    }

    [Fact]
    public async Task ListLatestMethodologyVersions_VerifyPermissions()
    {
        var publication = new Publication
        {
            Methodologies = new List<PublicationMethodology>
            {
                new()
                {
                    Methodology = new Methodology
                    {
                        Versions = new List<MethodologyVersion>
                        {
                            new()
                            {
                                Status = Approved,
                            },
                        },
                    },
                },
            },
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.Contacts.AddAsync(MockContact);
            await contentDbContext.SaveChangesAsync();
        }

        var userService = new Mock<IUserService>(Strict);

        userService.Setup(s => s.MatchesPolicy(It.IsAny<Publication>(), CanViewSpecificPublication))
            .ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanDeleteSpecificMethodology))
            .ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanUpdateSpecificMethodology))
            .ReturnsAsync(false);
        userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanApproveSpecificMethodology))
            .ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanSubmitSpecificMethodologyToHigherReview))
            .ReturnsAsync(false);
        userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanMarkSpecificMethodologyAsDraft))
            .ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(It.IsAny<MethodologyVersion>(), CanMakeAmendmentOfSpecificMethodology))
            .ReturnsAsync(false);
        userService.Setup(s => s.MatchesPolicy(It.IsAny<PublicationMethodology>(), CanDropMethodologyLink))
            .ReturnsAsync(true);

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupMethodologyService(contentDbContext: contentDbContext,
                userService: userService.Object);

            var result = await service.ListLatestMethodologyVersions(publication.Id);
            var viewModels = result.AssertRight();

            VerifyAllMocks(userService);

            var viewModel = Assert.Single(viewModels);
            var permissions = viewModel.Permissions;

            Assert.True(permissions.CanDeleteMethodology);
            Assert.False(permissions.CanUpdateMethodology);
            Assert.True(permissions.CanApproveMethodology);
            Assert.False(permissions.CanSubmitMethodologyForHigherReview);
            Assert.True(permissions.CanMarkMethodologyAsDraft);
            Assert.False(permissions.CanMakeAmendmentOfMethodology);
            Assert.True(permissions.CanRemoveMethodologyLink);
        }
    }

    [Fact]
    public async Task UpdateMethodology()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication",
            LatestPublishedReleaseVersion = new ReleaseVersion
            {
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug"
                }
            },
            Contact = MockContact
        };

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                OwningPublicationTitle = "Test publication",
                OwningPublicationSlug = "test-publication",
                Publications = ListOf(new PublicationMethodology
                {
                    Owner = true,
                    Publication = publication
                })
            }
        };

        var request = new MethodologyUpdateRequest
        {
            LatestInternalReleaseNote = null,
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Title = "Updated Methodology Title"
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock =>
                    mock.GetLatestPublishedVersionBySlug("updated-methodology-title"))
                .ReturnsAsync((MethodologyVersion?)null);

            var service = SetupMethodologyService(context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

            VerifyAllMocks(methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, viewModel.Id);
            Assert.Equal("updated-methodology-title", viewModel.Slug);
            Assert.Null(viewModel.InternalReleaseNote);
            Assert.Null(viewModel.Published);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, viewModel.PublishingStrategy);
            Assert.Null(viewModel.ScheduledWithRelease);
            Assert.Equal(request.Status, viewModel.Status);
            Assert.Equal(request.Title, viewModel.Title);
            Assert.Equal(publication.Id, viewModel.OwningPublication.Id);
            Assert.Equal(publication.Title, viewModel.OwningPublication.Title);
            Assert.Equal(publication.LatestPublishedReleaseVersion.Release.Slug, viewModel.OwningPublication.LatestReleaseSlug);
            Assert.Empty(viewModel.OtherPublications);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodology = await context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Null(updatedMethodology.Published);
            Assert.Equal(Draft, updatedMethodology.Status);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, updatedMethodology.PublishingStrategy);
            Assert.Equal("Updated Methodology Title", updatedMethodology.Title);
            Assert.Equal("Updated Methodology Title", updatedMethodology.AlternativeTitle);
            Assert.Equal("updated-methodology-title", updatedMethodology.Slug);
            Assert.True(updatedMethodology.Updated.HasValue);
            updatedMethodology.Updated.AssertUtcNow();
        }
    }

    [Fact]
    public async Task UpdateMethodology_UpdatingUnpublishedThenSlugChangesAndNoRedirect()
    {
        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            Status = Draft,
            Version = 0,
            Methodology = new Methodology
            {
                OwningPublicationTitle = "Test publication",
                OwningPublicationSlug = "test-publication",
                Publications = ListOf(new PublicationMethodology
                {
                    Owner = true,
                    Publication = MockPublication
                })
            },
        };

        var request = new MethodologyUpdateRequest
        {
            LatestInternalReleaseNote = null,
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Title = "Updated Methodology Title"
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug("updated-methodology-title"))
                .ReturnsAsync((MethodologyVersion?)null);

            var service = SetupMethodologyService(context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

            VerifyAllMocks(methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, viewModel.Id);
            Assert.Equal("Updated Methodology Title", viewModel.Title);
            Assert.Equal("updated-methodology-title", viewModel.Slug);
            Assert.Null(viewModel.InternalReleaseNote);
            Assert.Null(viewModel.Published);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, viewModel.PublishingStrategy);
            Assert.Null(viewModel.ScheduledWithRelease);
            Assert.Equal(request.Status, viewModel.Status);
            Assert.Equal(request.Title, viewModel.Title);
            Assert.Equal(MockPublication.Id, viewModel.OwningPublication.Id);
            Assert.Equal(MockPublication.Title, viewModel.OwningPublication.Title);
            Assert.Equal(MockPublication.LatestPublishedReleaseVersion!.Release.Slug, viewModel.OwningPublication.LatestReleaseSlug);
            Assert.Empty(viewModel.OtherPublications);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Draft, updatedMethodologyVersion.Status);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Equal("Updated Methodology Title", updatedMethodologyVersion.Title);
            Assert.Equal("Updated Methodology Title", updatedMethodologyVersion.AlternativeTitle);
            Assert.Equal("updated-methodology-title", updatedMethodologyVersion.Slug);
            Assert.Equal("updated-methodology-title", updatedMethodologyVersion.AlternativeSlug);
            Assert.Equal("Test publication", updatedMethodologyVersion.Methodology.OwningPublicationTitle);
            Assert.Equal("test-publication", updatedMethodologyVersion.Methodology.OwningPublicationSlug);
            Assert.True(updatedMethodologyVersion.Updated.HasValue);
            updatedMethodologyVersion.Updated.AssertUtcNow();

            // previous slug was not live, so no redirect required
            var redirects = context.MethodologyRedirects.ToList();
            Assert.Empty(redirects);
        }
    }

    [Fact]
    public async Task UpdateMethodology_UpdatingTitleSlugToMatchPublicationUnsetsAlternativeTitleSlug()
    {
        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            AlternativeTitle = "Alternative Methodology Title",
            AlternativeSlug = "alternative-methodology-title",
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = "Test publication",
                OwningPublicationSlug = "test-publication",
                Publications = ListOf(new PublicationMethodology
                {
                    Owner = true,
                    Publication = MockPublication
                })
            }
        };

        var request = new MethodologyUpdateRequest
        {
            LatestInternalReleaseNote = null,
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Title = "Test publication"
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug("test-publication"))
                .ReturnsAsync((MethodologyVersion?)null);

            var service = SetupMethodologyService(context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

            VerifyAllMocks(methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, viewModel.Id);
            Assert.Equal("test-publication", viewModel.Slug);
            Assert.Null(viewModel.InternalReleaseNote);
            Assert.Null(viewModel.Published);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, viewModel.PublishingStrategy);
            Assert.Null(viewModel.ScheduledWithRelease);
            Assert.Equal(request.Status, viewModel.Status);
            Assert.Equal(request.Title, viewModel.Title);
            Assert.Equal(MockPublication.Id, viewModel.OwningPublication.Id);
            Assert.Equal(MockPublication.Title, viewModel.OwningPublication.Title);
            Assert.Equal(MockPublication.LatestPublishedReleaseVersion!.Release.Slug, viewModel.OwningPublication.LatestReleaseSlug);
            Assert.Empty(viewModel.OtherPublications);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Draft, updatedMethodologyVersion.Status);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Equal(MockPublication.Title, updatedMethodologyVersion.Title);

            // Test explicitly that AlternativeTitle has been unset.
            Assert.Null(updatedMethodologyVersion.AlternativeTitle);
            Assert.Null(updatedMethodologyVersion.AlternativeSlug);
            Assert.Equal("test-publication", updatedMethodologyVersion.Slug);
            Assert.True(updatedMethodologyVersion.Updated.HasValue);
            updatedMethodologyVersion.Updated.AssertUtcNow();
        }
    }

    [Fact]
    public async Task UpdateMethodology_UpdatingAmendmentWithAlternativeSlug()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication",
            LatestPublishedReleaseVersion = new ReleaseVersion
            {
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug"
                }
            },
            Contact = MockContact
        };

        var latestPublishedVersionId = Guid.NewGuid();
        var latestVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            LatestPublishedVersionId = latestPublishedVersionId,
            OwningPublicationTitle = "Test publication",
            OwningPublicationSlug = "test-publication",
            Publications = ListOf(new PublicationMethodology
            {
                Owner = true,
                Publication = publication
            }),
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = latestPublishedVersionId,
                    Status = Approved,
                    Version = 0,
                },
                new()
                {
                    Id = latestVersionId,
                    Status = Draft,
                    Version = 1,
                    PreviousVersionId = latestPublishedVersionId,
                },
            },
        };

        var request = new MethodologyUpdateRequest
        {
            LatestInternalReleaseNote = null,
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Title = "Alternative title",
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Methodologies.AddAsync(methodology);
            await context.Contacts.AddAsync(MockContact);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug("alternative-title"))
                .ReturnsAsync((MethodologyVersion?)null);

            var service = SetupMethodologyService(context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var response = await service.UpdateMethodology(methodology.Versions[1].Id, request);
            var viewModel = response.AssertRight();

            VerifyAllMocks(methodologyVersionRepository);

            Assert.Equal(methodology.Versions[1].Id, viewModel.Id);
            Assert.Equal("alternative-title", viewModel.Slug);
            Assert.Null(viewModel.InternalReleaseNote);
            Assert.Null(viewModel.Published);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, viewModel.PublishingStrategy);
            Assert.Null(viewModel.ScheduledWithRelease);
            Assert.Equal(request.Status, viewModel.Status);
            Assert.Equal(request.Title, viewModel.Title);
            Assert.Equal(publication.Id, viewModel.OwningPublication.Id);
            Assert.Equal(publication.Title, viewModel.OwningPublication.Title);
            Assert.Equal(publication.LatestPublishedReleaseVersion.Release.Slug, viewModel.OwningPublication.LatestReleaseSlug);
            Assert.Empty(viewModel.OtherPublications);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodology.Versions[1].Id);

            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Draft, updatedMethodologyVersion.Status);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Equal("Alternative title", updatedMethodologyVersion.Title);
            Assert.Equal("alternative-title", updatedMethodologyVersion.Slug);

            Assert.Equal("Alternative title", updatedMethodologyVersion.AlternativeTitle);
            Assert.Equal("alternative-title", updatedMethodologyVersion.AlternativeSlug);
            Assert.Equal("Test publication", updatedMethodologyVersion.Methodology.OwningPublicationTitle);
            Assert.Equal("test-publication", updatedMethodologyVersion.Methodology.OwningPublicationSlug);

            Assert.True(updatedMethodologyVersion.Updated.HasValue);
            updatedMethodologyVersion.Updated.AssertUtcNow();

            // We need a redirect for "test-publication" when this amendment is published.
            // The redirect is inactive until the amendment is published.
            var dbMethodologyRedirect = await context
                .MethodologyRedirects
                .SingleAsync(mr => mr.MethodologyVersionId == latestVersionId);
            Assert.Equal("test-publication", dbMethodologyRedirect.Slug);
        }
    }

    [Fact]
    public async Task UpdateMethodology_UpdatingAmendmentUnsetsAlternativeTitleAndSlug()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication",
            LatestPublishedReleaseVersion = new ReleaseVersion
            {
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug"
                }
            },
            Contact = MockContact
        };

        var latestPublishedVersionId = Guid.NewGuid();
        var latestVersionId = Guid.NewGuid();
        var methodology = new Methodology
        {
            LatestPublishedVersionId = latestPublishedVersionId,
            OwningPublicationTitle = "Test publication",
            OwningPublicationSlug = "test-publication",
            Publications = ListOf(new PublicationMethodology
            {
                Owner = true,
                Publication = publication
            }),
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = latestPublishedVersionId,
                    Status = Approved,
                    Version = 0,
                },
                new()
                {
                    Id = latestVersionId,
                    AlternativeTitle = "Alternative Methodology Title",
                    AlternativeSlug = "alternative-methodology-title",
                    Status = Draft,
                    Version = 1,
                    PreviousVersionId = latestPublishedVersionId,
                },
            },
        };

        var methodologyRedirect = new MethodologyRedirect
        {
            MethodologyVersionId = latestVersionId,
            Slug = "test-publication",
        };

        var request = new MethodologyUpdateRequest
        {
            LatestInternalReleaseNote = null,
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Title = publication.Title
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Methodologies.AddAsync(methodology);
            await context.MethodologyRedirects.AddAsync(methodologyRedirect);
            await context.Contacts.AddAsync(MockContact);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug(publication.Slug))
                .ReturnsAsync((MethodologyVersion?)null);

            var service = SetupMethodologyService(context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var response = await service.UpdateMethodology(methodology.Versions[1].Id, request);
            var viewModel = response.AssertRight();

            VerifyAllMocks(methodologyVersionRepository);

            Assert.Equal(methodology.Versions[1].Id, viewModel.Id);
            Assert.Equal("test-publication", viewModel.Slug);
            Assert.Null(viewModel.InternalReleaseNote);
            Assert.Null(viewModel.Published);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, viewModel.PublishingStrategy);
            Assert.Null(viewModel.ScheduledWithRelease);
            Assert.Equal(request.Status, viewModel.Status);
            Assert.Equal(request.Title, viewModel.Title);
            Assert.Equal(publication.Id, viewModel.OwningPublication.Id);
            Assert.Equal(publication.Title, viewModel.OwningPublication.Title);
            Assert.Equal(publication.LatestPublishedReleaseVersion.Release.Slug, viewModel.OwningPublication.LatestReleaseSlug);
            Assert.Empty(viewModel.OtherPublications);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodology.Versions[1].Id);

            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Draft, updatedMethodologyVersion.Status);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Equal(MockPublication.Title, updatedMethodologyVersion.Title);

            Assert.Null(updatedMethodologyVersion.AlternativeTitle);
            Assert.Null(updatedMethodologyVersion.AlternativeSlug);

            Assert.Equal("test-publication", updatedMethodologyVersion.Slug);
            Assert.True(updatedMethodologyVersion.Updated.HasValue);
            updatedMethodologyVersion.Updated.AssertUtcNow();

            // Previous amendment slug was never live, so no redirect required
            // but "test-publication" is the currently live, so still need a redirect for that slug
            // (that will become active when the amendment is published)
            var dbMethodologyRedirect = await context
                .MethodologyRedirects
                .SingleAsync(mr => mr.MethodologyVersionId == latestVersionId);
            Assert.Equal("test-publication", dbMethodologyRedirect.Slug);
        }
    }

    [Fact]
    public async Task UpdateMethodology_SettingAlternativeTitleCausesSlugClash()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication"
        };

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = "Test publication",
                OwningPublicationSlug = "test-publication",
                Publications = ListOf(new PublicationMethodology
                {
                    Owner = true,
                    Publication = publication
                })
            }
        };

        // This pre-existing Methodology has a slug that the update will clash with.
        var methodologyWithTargetSlug = new MethodologyVersion
        {
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = "Test publication 2",
                OwningPublicationSlug = "test-publication-2",
            }
        };

        var request = new MethodologyUpdateRequest
        {
            LatestInternalReleaseNote = null,
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Title = "Updated Methodology Title"
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddRangeAsync(methodologyVersion, methodologyWithTargetSlug);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug("updated-methodology-title"))
                .ReturnsAsync(methodologyWithTargetSlug);

            var service = SetupMethodologyService(context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var result = await service.UpdateMethodology(methodologyVersion.Id, request);
            result.AssertBadRequest(MethodologySlugNotUnique);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var notUpdatedMethodology = await context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Null(notUpdatedMethodology.Published);
            Assert.Equal(Draft, notUpdatedMethodology.Status);
            Assert.Equal(MethodologyPublishingStrategy.Immediately, notUpdatedMethodology.PublishingStrategy);
            Assert.Equal("Test publication", notUpdatedMethodology.Title);
            Assert.Equal("Test publication", notUpdatedMethodology.Methodology.OwningPublicationTitle);
            Assert.Null(notUpdatedMethodology.AlternativeTitle);
            Assert.Equal("test-publication", notUpdatedMethodology.Slug);
            Assert.Equal("test-publication", notUpdatedMethodology.Methodology.OwningPublicationSlug);
            Assert.Null(notUpdatedMethodology.AlternativeSlug);
            Assert.False(notUpdatedMethodology.Updated.HasValue);
        }
    }

    [Fact]
    public async Task UpdateMethodology_RedirectForNewSlugAlreadyExists()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication"
        };

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                LatestPublishedVersionId = null,
                OwningPublicationTitle = "Test publication",
                OwningPublicationSlug = "test-publication",
                Publications = ListOf(new PublicationMethodology
                {
                    Owner = true,
                    Publication = publication
                })
            }
        };

        var versionWithRedirect = new MethodologyVersion
        {
            Methodology = new Methodology(),
        };

        var methodologyRedirect = new MethodologyRedirect
        {
            MethodologyVersion = versionWithRedirect,
            Slug = "updated-methodology-title",
        };

        var request = new MethodologyUpdateRequest
        {
            Title = "Updated Methodology Title"
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddRangeAsync(methodologyVersion, versionWithRedirect);
            await context.MethodologyRedirects.AddAsync(methodologyRedirect);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug("updated-methodology-title"))
                .ReturnsAsync((MethodologyVersion?)null);

            var service = SetupMethodologyService(context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var result = await service.UpdateMethodology(methodologyVersion.Id, request);
            result.AssertBadRequest(MethodologySlugUsedByRedirect);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var notUpdatedMethodology = await context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Equal("Test publication", notUpdatedMethodology.Title);
            Assert.Equal("Test publication", notUpdatedMethodology.Methodology.OwningPublicationTitle);
            Assert.Null(notUpdatedMethodology.AlternativeTitle);
            Assert.Equal("test-publication", notUpdatedMethodology.Slug);
            Assert.Equal("test-publication", notUpdatedMethodology.Methodology.OwningPublicationSlug);
            Assert.Null(notUpdatedMethodology.AlternativeSlug);

            Assert.False(notUpdatedMethodology.Updated.HasValue);
        }
    }

    [Fact]
    public async Task UpdateMethodology_RedirectForNewSlugAlreadyExistsButForSameMethodology()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication",
            LatestPublishedReleaseVersion = new ReleaseVersion
            {
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug"
                }
            },
            Contact = MockContact,
        };

        var versionWithRedirectId = Guid.NewGuid();
        var methodology = new Methodology
        {
            LatestPublishedVersionId = versionWithRedirectId,
            OwningPublicationTitle = "Test publication",
            OwningPublicationSlug = "test-publication",
            Publications = ListOf(new PublicationMethodology
            {
                Owner = true,
                Publication = publication
            })
        };

        var versionWithRedirect = new MethodologyVersion
        {
            Id = versionWithRedirectId,
            Methodology = methodology,
            Status = Approved,
        };

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            Methodology = methodology,
            Status = Draft,
            PreviousVersionId = versionWithRedirectId,
        };

        var methodologyRedirect = new MethodologyRedirect
        {
            MethodologyVersion = versionWithRedirect,
            Slug = "updated-methodology-title",
        };

        var request = new MethodologyUpdateRequest
        {
            Title = "Updated Methodology Title",
            Status = Draft,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddRangeAsync(methodologyVersion, versionWithRedirect);
            await context.MethodologyRedirects.AddAsync(methodologyRedirect);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug("updated-methodology-title"))
                .ReturnsAsync((MethodologyVersion?)null);

            var service = SetupMethodologyService(context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var result = await service.UpdateMethodology(methodologyVersion.Id, request);
            var methodologyVersionViewModel = result.AssertRight();

            Assert.Equal(methodologyVersion.Id, methodologyVersionViewModel.Id);
            Assert.Equal("Updated Methodology Title", methodologyVersionViewModel.Title);
            Assert.Equal("updated-methodology-title", methodologyVersionViewModel.Slug);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedVersion = await context
                .MethodologyVersions
                .Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Equal("Updated Methodology Title", updatedVersion.Title);
            Assert.Equal("Updated Methodology Title", updatedVersion.AlternativeTitle);
            Assert.Equal("Test publication", updatedVersion.Methodology.OwningPublicationTitle);

            Assert.Equal("updated-methodology-title", updatedVersion.Slug);
            Assert.Equal("updated-methodology-title", updatedVersion.AlternativeSlug);
            Assert.Equal("test-publication", updatedVersion.Methodology.OwningPublicationSlug);

            Assert.True(updatedVersion.Updated.HasValue);
        }
    }

    [Fact]
    public async Task UpdateMethodology_StatusUpdate()
    {
        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                OwningPublicationTitle = MockPublication.Title,
                OwningPublicationSlug = MockPublication.Slug,
                Publications = ListOf(new PublicationMethodology
                {
                    Owner = true,
                    Publication = MockPublication
                })
            }
        };

        var request = new MethodologyUpdateRequest
        {
            Title = "Updated Methodology Title",
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionBySlug(
                    "updated-methodology-title"))
                .ReturnsAsync((MethodologyVersion?)null);

            var service = SetupMethodologyService(
                context,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            await service.UpdateMethodology(methodologyVersion.Id, request);

            VerifyAllMocks(methodologyVersionRepository);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodology = await context
                .MethodologyVersions
                .AsQueryable()
                .Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            // Check that title and slug updates have occurred as well as approval updates.
            Assert.Equal("Updated Methodology Title", updatedMethodology.Title);
            Assert.Equal("Updated Methodology Title", updatedMethodology.AlternativeTitle);
            Assert.Equal("updated-methodology-title", updatedMethodology.Slug);
            Assert.Equal("updated-methodology-title", updatedMethodology.AlternativeSlug);
            Assert.True(updatedMethodology.Updated.HasValue);
            updatedMethodology.Updated.AssertUtcNow();
        }
    }

    [Fact]
    public async Task DeleteMethodology()
    {
        var methodologyVersion1Id = Guid.NewGuid();
        var methodologyVersion2Id = Guid.NewGuid();
        var methodologyVersion3Id = Guid.NewGuid();
        var methodologyVersion4Id = Guid.NewGuid();

        var methodology = new Methodology
        {
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Id = methodologyVersion2Id,
                    PreviousVersionId = methodologyVersion1Id
                },
                new()
                {
                    Id = methodologyVersion1Id
                },
                new()
                {
                    Id = methodologyVersion4Id,
                    PreviousVersionId = methodologyVersion3Id
                },
                new()
                {
                    Id = methodologyVersion3Id,
                    PreviousVersionId = methodologyVersion2Id,
                }
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Sanity check that the Methodology and MethodologyVersions were created.
            Assert.Equal(1, context.Methodologies.Count());
            Assert.Equal(4, context.MethodologyVersions.Count());
        }

        var methodologyImageService = new Mock<IMethodologyImageService>(Strict);

        // Since the MethodologyVersions should be deleted in sequence, expect a call to delete images for each of the
        // versions in the same sequence

        var deleteSequence = new MockSequence();

        methodologyImageService
            .InSequence(deleteSequence)
            .Setup(s => s.DeleteAll(methodologyVersion4Id, false))
            .ReturnsAsync(Unit.Instance);

        methodologyImageService
            .InSequence(deleteSequence)
            .Setup(s => s.DeleteAll(methodologyVersion3Id, false))
            .ReturnsAsync(Unit.Instance);

        methodologyImageService
            .InSequence(deleteSequence)
            .Setup(s => s.DeleteAll(methodologyVersion2Id, false))
            .ReturnsAsync(Unit.Instance);

        methodologyImageService
            .InSequence(deleteSequence)
            .Setup(s => s.DeleteAll(methodologyVersion1Id, false))
            .ReturnsAsync(Unit.Instance);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(context,
                methodologyImageService: methodologyImageService.Object);

            var result = await service.DeleteMethodology(methodology.Id);

            VerifyAllMocks(methodologyImageService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Assert that the methodology and the versions have been successfully deleted
            Assert.Equal(0, context.Methodologies.Count());
            Assert.Equal(0, context.MethodologyVersions.Count());
        }
    }

    [Fact]
    public async Task DeleteMethodologyVersion()
    {
        var methodologyId = Guid.NewGuid();

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = MethodologyPublishingStrategy.Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                Id = methodologyId,
                OwningPublicationTitle = "Pupil absence statistics: methodology",
                OwningPublicationSlug = "pupil-absence-statistics-methodology",
                Publications = ListOf(new PublicationMethodology
                {
                    MethodologyId = methodologyId,
                    PublicationId = Guid.NewGuid(),
                })
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Sanity check that a Methodology, a MethodologyVersion and a PublicationMethodology row were
            // created.
            Assert.NotNull(await context.Methodologies.AsQueryable()
                .SingleAsync(m => m.Id == methodologyId));
            Assert.NotNull(await context.MethodologyVersions.AsQueryable()
                .SingleAsync(m => m.Id == methodologyVersion.Id));
            Assert.NotNull(await context.PublicationMethodologies.AsQueryable()
                .SingleAsync(m => m.MethodologyId == methodologyId));
        }

        var methodologyImageService = new Mock<IMethodologyImageService>(Strict);

        methodologyImageService.Setup(mock => mock.DeleteAll(methodologyVersion.Id, false))
            .ReturnsAsync(Unit.Instance);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(context,
                methodologyImageService: methodologyImageService.Object);

            var result = await service.DeleteMethodologyVersion(methodologyVersion.Id);

            VerifyAllMocks(methodologyImageService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Assert that the version has successfully been deleted and as it was the only version on the
            // methodology, the methodology is deleted too.
            //
            // Also, as the methodology was deleted, then the PublicationMethodology links that linked
            // it with Publications should also be deleted.  This is done with a cascade delete, but in-memory
            // db currently doesn't support this so we can't check that there are no longer those
            // PublicationMethodology rows.
            Assert.False(context.MethodologyVersions.Any(m => m.Id == methodologyVersion.Id));
            Assert.False(context.Methodologies.Any(m => m.Id == methodologyId));
        }
    }

    [Fact]
    public async Task DeleteMethodologyVersion_MoreThanOneVersion()
    {
        var methodologyId = Guid.NewGuid();

        var methodology = new Methodology
        {
            Id = methodologyId,
            OwningPublicationTitle = "Pupil absence statistics: methodology",
            OwningPublicationSlug = "pupil-absence-statistics-methodology",
            Versions = ListOf(new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                    Status = Draft
                },
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                    Status = Draft
                })
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Methodologies.AddAsync(methodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Sanity check that there is a methodology with two versions.
            Assert.NotNull(await context.Methodologies.AsQueryable()
                .SingleAsync(m => m.Id == methodologyId));
            Assert.NotNull(await context.MethodologyVersions.AsQueryable()
                .SingleAsync(m => m.Id == methodology.Versions[0].Id));
            Assert.NotNull(await context.MethodologyVersions.AsQueryable()
                .SingleAsync(m => m.Id == methodology.Versions[1].Id));
        }

        var methodologyImageService = new Mock<IMethodologyImageService>(Strict);

        methodologyImageService.Setup(mock => mock.DeleteAll(methodology.Versions[1].Id, false))
            .ReturnsAsync(Unit.Instance);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext: context,
                methodologyImageService: methodologyImageService.Object);

            var result = await service.DeleteMethodologyVersion(methodology.Versions[1].Id);

            // Verify that the Methodology Image Service was called to remove only the Methodology Files linked to
            // the version being deleted.
            VerifyAllMocks(methodologyImageService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Assert that the version has successfully been deleted and as there was another version attached
            // to the methodology, the methodology itself is not deleted, or the other version.
            Assert.False(context.MethodologyVersions.Any(m => m.Id == methodology.Versions[1].Id));
            Assert.NotNull(await context.MethodologyVersions.AsQueryable()
                .SingleAsync(m => m.Id == methodology.Versions[0].Id));
            Assert.NotNull(await context.Methodologies.AsQueryable()
                .SingleAsync(m => m.Id == methodologyId));
        }
    }

    [Fact]
    public async Task DeleteMethodologyVersion_UnrelatedMethodologiesAreUnaffected()
    {
        var methodologyId = Guid.NewGuid();
        var unrelatedMethodologyId = Guid.NewGuid();

        var methodology = new Methodology
        {
            Id = methodologyId,
            OwningPublicationTitle = "Pupil absence statistics: methodology",
            OwningPublicationSlug = "pupil-absence-statistics-methodology",
            Versions = ListOf(new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = Draft
            })
        };

        var unrelatedMethodology = new Methodology
        {
            Id = unrelatedMethodologyId,
            OwningPublicationTitle = "Pupil absence statistics: methodology",
            OwningPublicationSlug = "pupil-absence-statistics-methodology",
            Versions = ListOf(new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = Draft
            })
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Methodologies.AddRangeAsync(methodology, unrelatedMethodology);
            await context.SaveChangesAsync();
        }

        var methodologyImageService = new Mock<IMethodologyImageService>(Strict);

        methodologyImageService.Setup(mock => mock.DeleteAll(methodology.Versions[0].Id, false))
            .ReturnsAsync(Unit.Instance);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(context,
                methodologyImageService: methodologyImageService.Object);

            var result = await service.DeleteMethodologyVersion(methodology.Versions[0].Id);

            // Verify that the Methodology Image Service was called to remove only the Methodology Files linked to
            // the version being deleted.
            VerifyAllMocks(methodologyImageService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Assert that the methodology and its version are deleted, but the unrelated methodology is unaffected.
            Assert.False(context.MethodologyVersions.Any(m => m.Id == methodology.Versions[0].Id));
            Assert.False(context.Methodologies.Any(m => m.Id == methodologyId));

            Assert.NotNull(
                await context.MethodologyVersions.AsQueryable()
                    .SingleAsync(m => m.Id == unrelatedMethodology.Versions[0].Id));
            Assert.NotNull(
                await context.Methodologies.AsQueryable()
                    .SingleAsync(m => m.Id == unrelatedMethodologyId));
        }
    }

    [Fact]
    public async Task GetMethodologyStatuses()
    {
        var methodologyId = Guid.NewGuid();
        var unrelatedMethodologyId = Guid.NewGuid();

        var methodology = new Methodology
        {
            Id = methodologyId,
            OwningPublicationTitle = "Pupil absence statistics: methodology",
            OwningPublicationSlug = "pupil-absence-statistics-methodology",
            Versions = ListOf(
                new MethodologyVersion
                {
                    PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                    Status = Draft,
                    Version = 0,
                },
                new MethodologyVersion
                {
                    PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                    Status = Approved,
                    Version = 1,
                }
            ),
        };

        var unrelatedMethodology = new Methodology
        {
            Id = unrelatedMethodologyId,
            OwningPublicationTitle = "Pupil absence statistics: methodology",
            OwningPublicationSlug = "pupil-absence-statistics-methodology",
            Versions = ListOf(new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = Draft
            })
        };

        var methodologyStatuses = new List<MethodologyStatus>
        {
            new()
            {
                MethodologyVersion = methodology.Versions[0],
                InternalReleaseNote = "Status 1 note",
                ApprovalStatus = Approved,
                Created = new DateTime(2000, 1, 1),
                CreatedById = User.Id,
            },
            new()
            {
                MethodologyVersion = methodology.Versions[1],
                InternalReleaseNote = "Status 2 note",
                ApprovalStatus = Approved,
                Created = new DateTime(2001, 1, 1),
                CreatedById = User.Id,
            },
            new()
            {
                MethodologyVersion = unrelatedMethodology.Versions[0],
                InternalReleaseNote = "Unrelated note",
                ApprovalStatus = Approved,
                Created = new DateTime(2002, 1, 1),
                CreatedById = Guid.NewGuid(),
            }
        };

        var user = new User
        {
            Id = User.Id,
            Email = "test@test.com",
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Methodologies.AddRangeAsync(methodology, unrelatedMethodology);
            await context.MethodologyStatus.AddRangeAsync(methodologyStatuses);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(context);

            var result = await service.GetMethodologyStatuses(methodology.Versions[0].Id);

            var statuses = result.AssertRight();

            Assert.NotNull(statuses);
            Assert.Equal(2, statuses.Count);

            Assert.Equal(methodologyStatuses[1].Id, statuses[0].MethodologyStatusId); // because OrderByDesc
            Assert.Equal(1, statuses[0].MethodologyVersion);
            Assert.Equal(methodologyStatuses[1].InternalReleaseNote, statuses[0].InternalReleaseNote);
            Assert.Equal(methodologyStatuses[1].ApprovalStatus, statuses[0].ApprovalStatus);
            Assert.Equal(methodologyStatuses[1].Created, statuses[0].Created);
            Assert.Equal("test@test.com", statuses[0].CreatedByEmail);

            Assert.Equal(methodologyStatuses[0].Id, statuses[1].MethodologyStatusId);
            Assert.Equal(0, statuses[1].MethodologyVersion);
            Assert.Equal(methodologyStatuses[0].InternalReleaseNote, statuses[1].InternalReleaseNote);
            Assert.Equal(methodologyStatuses[0].ApprovalStatus, statuses[1].ApprovalStatus);
            Assert.Equal(methodologyStatuses[0].Created, statuses[1].Created);
            Assert.Equal("test@test.com", statuses[1].CreatedByEmail);
        }
    }

    [Fact]
    public async Task GetMethodologyStatuses_NoMethodologyVersion()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(context);

            var result = await service.GetMethodologyStatuses(Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetMethodologyStatuses_NoStatuses()
    {
        var methodologyId = Guid.NewGuid();
        var unrelatedMethodologyId = Guid.NewGuid();

        var methodology = new Methodology
        {
            Id = methodologyId,
            OwningPublicationTitle = "Pupil absence statistics: methodology",
            OwningPublicationSlug = "pupil-absence-statistics-methodology",
            Versions = ListOf(
                new MethodologyVersion
                {
                    PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                    Status = Draft,
                },
                new MethodologyVersion
                {
                    PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                    Status = Approved,
                }
            ),
        };

        var unrelatedMethodology = new Methodology
        {
            Id = unrelatedMethodologyId,
            OwningPublicationTitle = "Pupil absence statistics: methodology",
            OwningPublicationSlug = "pupil-absence-statistics-methodology",
            Versions = ListOf(new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = Draft
            })
        };

        var methodologyStatuses = new List<MethodologyStatus>
        {
            new()
            {
                MethodologyVersion = unrelatedMethodology.Versions[0],
                InternalReleaseNote = "Unrelated note",
                ApprovalStatus = Approved,
                Created = new DateTime(2002, 1, 1),
                CreatedById = Guid.NewGuid(),
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Methodologies.AddRangeAsync(methodology, unrelatedMethodology);
            await context.MethodologyStatus.AddRangeAsync(methodologyStatuses);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(context);

            var result = await service.GetMethodologyStatuses(methodology.Versions[0].Id);

            var statuses = result.AssertRight();

            Assert.NotNull(statuses);
            Assert.Empty(statuses);
        }
    }

    public class ListUsersMethodologyVersionsForApprovalForPublicationRoles
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task UserIsApproverOnOwningPublication_Included()
        {
            var publication = _fixture
                .DefaultPublication()
                .WithReleases(_ =>
                [
                    _fixture.DefaultRelease(publishedVersions: 1, year: 2020)
                ])
                .WithContact(MockContact)
                .Generate();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .Generate(1))
                .Generate();

            var publicationRoleForUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(User)
                .WithPublication(publication)
                .WithRole(PublicationRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.Contacts.AddAsync(MockContact);
                await context.UserPublicationRoles.AddRangeAsync(publicationRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);

                var result = await service.ListUsersMethodologyVersionsForApproval();
                var methodologyVersionsForApproval = result.AssertRight();

                var methodologyForApproval = Assert.Single(methodologyVersionsForApproval);
                Assert.Equal(methodology.Versions[0].Id, methodologyForApproval.Id);

                // Assert that we have a populated view model, including the owning Publication details.
                Assert.Equal(publication.Title, methodologyForApproval.OwningPublication.Title);
            }
        }

        [Fact]
        public async Task MethodologyVersionNotInHigherReview_NotIncluded()
        {
            var publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .Generate();

            // Generate 2 Methodologies that are not in Higher Review.
            var methodologies = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .ForIndex(0, s => s.SetMethodologyVersions(_fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(Draft)
                    .Generate(1)))
                .ForIndex(1, s => s.SetMethodologyVersions(_fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(Approved)
                    .Generate(1)))
                .GenerateList();

            var publicationRoleForUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(User)
                .WithPublication(publication)
                .WithRole(PublicationRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodologies);
                await context.Contacts.AddAsync(MockContact);
                await context.UserPublicationRoles.AddRangeAsync(publicationRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);
                var result = await service.ListUsersMethodologyVersionsForApproval();
                Assert.Empty(result.AssertRight());
            }
        }

        [Fact]
        public async Task UserIsApproverButOnAdoptingPublication_NotIncluded()
        {
            var publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .Generate();

            // Create a Methodology that has only been adopted by the User's Publication.
            var methodology = _fixture
                .DefaultMethodology()
                .WithAdoptingPublications([publication])
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .Generate(1))
                .Generate();

            var publicationRoleForUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(User)
                .WithPublication(publication)
                .WithRole(PublicationRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.UserPublicationRoles.AddRangeAsync(publicationRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);

                var result = await service.ListUsersMethodologyVersionsForApproval();
                Assert.Empty(result.AssertRight());
            }
        }

        [Fact]
        public async Task UserIsOnlyOwnerOnOwningPublication_NotIncluded()
        {
            var publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .Generate();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .Generate(1))
                .Generate();

            // Set up the User as an Owner on the Methodology's Publication rather than an Approver.
            var publicationRoleForUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(User)
                .WithPublication(publication)
                .WithRole(PublicationRole.Owner)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.UserPublicationRoles.AddRangeAsync(publicationRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);
                var result = await service.ListUsersMethodologyVersionsForApproval();
                Assert.Empty(result.AssertRight());
            }
        }

        [Fact]
        public async Task DifferentUserIsApproverOnOwningPublication_NotIncluded()
        {
            // Set up a different User as the Approver for the owning Publication.
            var otherUser = new User();

            var publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .Generate();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .Generate(1))
                .Generate();

            var publicationRoleForUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(otherUser)
                .WithPublication(publication)
                .WithRole(PublicationRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.UserPublicationRoles.AddRangeAsync(publicationRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);
                var result = await service.ListUsersMethodologyVersionsForApproval();
                Assert.Empty(result.AssertRight());
            }
        }
    }

    public class ListUsersMethodologyVersionsForApprovalForReleaseRoles
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task UserIsApproverOnOwningPublicationRelease_Included()
        {
            Publication publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var releaseVersion = publication.Releases.Single().Versions.Single();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .Generate(1))
                .Generate();

            var releaseRoleForUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithReleaseVersion(releaseVersion)
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.UserReleaseRoles.AddRangeAsync(releaseRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);

                var result = await service.ListUsersMethodologyVersionsForApproval();
                var methodologyVersionsForApproval = result.AssertRight();

                var methodologyForApproval = Assert.Single(methodologyVersionsForApproval);
                Assert.Equal(methodology.Versions[0].Id, methodologyForApproval.Id);

                // Assert that we have a populated view model, including the owning Publication details.
                Assert.Equal(publication.Title, methodologyForApproval.OwningPublication.Title);
            }
        }

        [Fact]
        public async Task UserIsApproverOnOwningPublicationOldRelease_Included()
        {
            Publication publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .WithReleases(_fixture.DefaultRelease(publishedVersions: 1, draftVersion: true)
                    .Generate(1));

            var publishedReleaseVersion = publication.Releases.Single().Versions
                .Single(rv => rv is { Published: not null, Version: 0 });

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .Generate(1))
                .Generate();

            var releaseRoleForUserOnOldRelease = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithReleaseVersion(publishedReleaseVersion)
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.Publications.AddAsync(publication);
                await context.Contacts.AddAsync(MockContact);
                await context.UserReleaseRoles.AddRangeAsync(releaseRoleForUserOnOldRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);

                var result = await service.ListUsersMethodologyVersionsForApproval();
                var methodologyVersionsForApproval = result.AssertRight();

                // The user should have access to approve the Methodology if they have Approver permissions
                // on ANY of the Publication's Releases, not just the latest one.
                var methodologyForApproval = Assert.Single(methodologyVersionsForApproval);
                Assert.Equal(methodology.Versions[0].Id, methodologyForApproval.Id);
            }
        }

        [Fact]
        public async Task UserIsApproverOnOwningPublicationRelease_MethodologyVersionNotInHigherReview_NotIncluded()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .Generate();

            // Generate 2 Methodologies that are not in Higher Review.
            var methodologies = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .ForIndex(0, s => s.SetMethodologyVersions(_fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(Draft)
                    .Generate(1)))
                .ForIndex(1, s => s.SetMethodologyVersions(_fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(Approved)
                    .Generate(1)))
                .GenerateList();

            var releaseRoleForUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithReleaseVersion(releaseVersion)
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodologies);
                await context.UserReleaseRoles.AddRangeAsync(releaseRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);
                var result = await service.ListUsersMethodologyVersionsForApproval();
                Assert.Empty(result.AssertRight());
            }
        }

        [Fact]
        public async Task UserIsReleaseApproverOnAdoptingPublication_NotIncluded()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .Generate();

            // Create a Methodology that has only been adopted by the User's Publication.
            var methodology = _fixture
                .DefaultMethodology()
                .WithAdoptingPublications([publication])
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .Generate(1))
                .Generate();

            var releaseRoleForUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithReleaseVersion(releaseVersion)
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.UserReleaseRoles.AddRangeAsync(releaseRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);

                var result = await service.ListUsersMethodologyVersionsForApproval();
                Assert.Empty(result.AssertRight());
            }
        }

        [Fact]
        public async Task UserIsOnlyContributorOnOwningPublicationRelease_NotIncluded()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var publication = _fixture
                .DefaultPublication()
                .Generate();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .Generate(1))
                .Generate();

            // Set up the User as a Contributor on the Methodology's Publication's Release rather than an Approver.
            var releaseRoleForUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithReleaseVersion(releaseVersion)
                .WithRole(ReleaseRole.Contributor)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.UserReleaseRoles.AddRangeAsync(releaseRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);
                var result = await service.ListUsersMethodologyVersionsForApproval();
                Assert.Empty(result.AssertRight());
            }
        }

        [Fact]
        public async Task DifferentUserIsApproverOnOwningPublicationRelease_NotIncluded()
        {
            // Set up a different User as the Approver for the owning Publication.
            var otherUser = new User();

            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .Generate();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .Generate(1))
                .Generate();

            var releaseRoleForOtherUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(otherUser)
                .WithReleaseVersion(releaseVersion)
                .WithRole(ReleaseRole.Contributor)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.UserReleaseRoles.AddRangeAsync(releaseRoleForOtherUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);
                var result = await service.ListUsersMethodologyVersionsForApproval();
                Assert.Empty(result.AssertRight());
            }
        }

        [Fact]
        public async Task UserIsPublicationAndReleaseApprover_NoDuplication()
        {
            Publication publication = _fixture
                .DefaultPublication()
                .WithContact(MockContact)
                .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                    .Generate(1));

            var publishedReleaseVersion = publication.Releases.Single().Versions.Single();

            var methodology = _fixture
                .DefaultMethodology()
                .WithOwningPublication(publication)
                .WithMethodologyVersions(_ => _fixture
                    .DefaultMethodologyVersion()
                    .WithApprovalStatus(HigherLevelReview)
                    .GenerateList(1))
                .Generate();

            var publicationRoleForUser = _fixture
                .DefaultUserPublicationRole()
                .WithUser(User)
                .WithPublication(publication)
                .WithRole(PublicationRole.Approver)
                .Generate();

            var releaseRoleForUser = _fixture
                .DefaultUserReleaseRole()
                .WithUser(User)
                .WithReleaseVersion(publishedReleaseVersion)
                .WithRole(ReleaseRole.Approver)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddRangeAsync(methodology);
                await context.UserPublicationRoles.AddRangeAsync(publicationRoleForUser);
                await context.UserReleaseRoles.AddRangeAsync(releaseRoleForUser);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);

                var result = await service.ListUsersMethodologyVersionsForApproval();
                var methodologyVersionsForApproval = result.AssertRight();

                // Assert that we just get back a single MethodologyVersion with no duplication, despite the user
                // having links to this MethodologyVersion via Publication Roles and Release Roles.
                Assert.Single(methodologyVersionsForApproval);
            }
        }
    }

    [Fact]
    public async Task PublicationTitleOrSlugChanged()
    {
        var publicationId = Guid.NewGuid();
        var originalVersionId = Guid.NewGuid();
        var latestPublishedVersionId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = new PublicationMethodology
            {
                PublicationId = publicationId,
                Owner = true,
                Methodology = new Methodology
                {
                    LatestPublishedVersionId = latestPublishedVersionId,
                    OwningPublicationTitle = "Original Title",
                    OwningPublicationSlug = "original-slug",
                    Versions = new List<MethodologyVersion>
                    {
                        new()
                        {
                            Id = originalVersionId,
                            Version = 0,
                        },
                        new()
                        {
                            Id = latestPublishedVersionId,
                            Version = 1,
                            PreviousVersionId = originalVersionId,
                        },
                    },
                },
            };

            await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsCacheService = new Mock<IRedirectsCacheService>(MockBehavior.Strict);
            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var service = SetupMethodologyService(contentDbContext,
                redirectsCacheService: redirectsCacheService.Object);
            await service.PublicationTitleOrSlugChanged(publicationId, "original-slug", "New Title", "new-slug");
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = await contentDbContext
                .PublicationMethodologies
                .Include(m => m.Methodology.Versions)
                .SingleAsync(m => m.PublicationId == publicationId);

            Assert.Equal("New Title", publicationMethodology.Methodology.OwningPublicationTitle);
            Assert.Equal("new-slug", publicationMethodology.Methodology.OwningPublicationSlug);

            // As no AlternativeTitle or AlternativeSlug set, the MethodologyVersion's title and slug will also change
            Assert.Equal("New Title", publicationMethodology.Methodology.Versions[1].Title);
            Assert.Equal("new-slug", publicationMethodology.Methodology.Versions[1].Slug);

            // As methodology is published and it's slug has changed, a redirect is created for LatestPublishedVersion
            var methodologyRedirects = await contentDbContext.MethodologyRedirects
                .ToListAsync();
            var methodologyRedirect = Assert.Single(methodologyRedirects);
            Assert.Equal(latestPublishedVersionId, methodologyRedirect.MethodologyVersionId);
            Assert.Equal("original-slug", methodologyRedirect.Slug);
        }
    }

    [Fact]
    public async Task PublicationTitleOrSlugChanged_NoMethodologyRedirectAsMethodologyUnpublished()
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
                    LatestPublishedVersionId = null,
                    OwningPublicationTitle = "Original Title",
                    OwningPublicationSlug = "original-slug",
                    Versions = ListOf(new MethodologyVersion()),
                },
            };

            await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsCacheService = new Mock<IRedirectsCacheService>(MockBehavior.Strict);
            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var service = SetupMethodologyService(contentDbContext,
                redirectsCacheService: redirectsCacheService.Object);
            await service.PublicationTitleOrSlugChanged(publicationId,
                "original-slug", "New Title", "new-slug");
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = await contentDbContext
                .PublicationMethodologies
                .Include(m => m.Methodology.Versions)
                .SingleAsync(m => m.PublicationId == publicationId);

            Assert.Equal("New Title", publicationMethodology.Methodology.OwningPublicationTitle);
            Assert.Equal("new-slug", publicationMethodology.Methodology.OwningPublicationSlug);

            // As the Publication's Title and Slug changed, and no AlternativeTitle/Slug set,
            // the methodology's title and slug will also change
            Assert.Equal("New Title", publicationMethodology.Methodology.Versions[0].Title);
            Assert.Equal("new-slug", publicationMethodology.Methodology.Versions[0].Slug);

            // Methodology is unpublished, so no redirect
            var methodologyRedirects = await contentDbContext.MethodologyRedirects
                .ToListAsync();
            Assert.Empty(methodologyRedirects);
        }
    }

    [Fact]
    public async Task PublicationTitleOrSlugChanged_DoesNotAffectUnrelatedMethodologies()
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
                    OwningPublicationTitle = "Original Title",
                    OwningPublicationSlug = "original-slug",
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
                    OwningPublicationTitle = "Original Title",
                    OwningPublicationSlug = "original-slug",
                }
            };

            await contentDbContext.PublicationMethodologies.AddRangeAsync(
                publicationMethodology,
                unrelatedPublicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsCacheService = new Mock<IRedirectsCacheService>(MockBehavior.Strict);
            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var service = SetupMethodologyService(contentDbContext,
                redirectsCacheService: redirectsCacheService.Object);
            await service.PublicationTitleOrSlugChanged(publicationId, "original-slug", "New Title", "new-slug");
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = await contentDbContext
                .PublicationMethodologies
                .Include(m => m.Methodology)
                .SingleAsync(m => m.PublicationId == unrelatedPublicationId);

            // This Methodology was not related to the Publication being updated, and so was not affected by the update.
            Assert.Equal("Original Title", publicationMethodology.Methodology.OwningPublicationTitle);
            Assert.Equal("original-slug", publicationMethodology.Methodology.OwningPublicationSlug);
        }
    }

    [Fact]
    public async Task PublicationTitleOrSlugChanged_DoesNotAffectUnownedMethodologies()
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
                    Versions = ListOf(new MethodologyVersion()),
                    OwningPublicationTitle = "Original Title",
                    OwningPublicationSlug = "original-slug",
                }
            };

            await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext);
            await service.PublicationTitleOrSlugChanged(publicationId,
                "original-slug", "New Title", "new-slug");
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = await contentDbContext
                .PublicationMethodologies
                .Include(m => m.Methodology)
                .SingleAsync(m => m.PublicationId == publicationId);

            // This Methodology was not owned by the Publication being updated, and so was not affected by the update.
            Assert.Equal("Original Title", publicationMethodology.Methodology.OwningPublicationTitle);
            Assert.Equal("original-slug", publicationMethodology.Methodology.OwningPublicationSlug);
        }
    }

    [Fact]
    public async Task PublicationTitleOrSlugChanged_MethodologySlugIsAlternativeSlug()
    {
        var publicationId = Guid.NewGuid();
        var latestPublishedVersionId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = new PublicationMethodology
            {
                PublicationId = publicationId,
                Owner = true,
                Methodology = new Methodology
                {
                    LatestPublishedVersionId = latestPublishedVersionId,
                    OwningPublicationTitle = "Original title",
                    OwningPublicationSlug = "original-slug",
                    Versions = ListOf(new MethodologyVersion
                    {
                        Id = latestPublishedVersionId,
                        AlternativeSlug = "alternative-slug",
                    }),
                }
            };

            await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsCacheService = new Mock<IRedirectsCacheService>(MockBehavior.Strict);
            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var service = SetupMethodologyService(contentDbContext,
                redirectsCacheService: redirectsCacheService.Object);
            await service.PublicationTitleOrSlugChanged(publicationId, "original-slug", "New Title", "new-slug");
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = await contentDbContext
                .PublicationMethodologies
                .Include(m => m.Methodology)
                .ThenInclude(m => m.Versions)
                .SingleAsync(m => m.PublicationId == publicationId);

            Assert.Equal("New Title", publicationMethodology.Methodology.OwningPublicationTitle);
            Assert.Equal("new-slug", publicationMethodology.Methodology.OwningPublicationSlug);

            // The MethodologyVersion has already had an AlternativeSlug set. It doesn't have a AlternativeTitle
            // set. So the MethodologyVersion title is updated, but the slug remains the same.
            Assert.Equal("New Title", publicationMethodology.Methodology.Versions[0].Title);
            Assert.Equal("alternative-slug", publicationMethodology.Methodology.Versions[0].Slug);

            // No redirect created as slug hasn't changed
            var methodologyRedirects = await contentDbContext.MethodologyRedirects
                .ToListAsync();
            Assert.Empty(methodologyRedirects);
        }
    }

    [Fact]
    public async Task PublicationTitleOrSlugChanged_MethodologyIsLive()
    {
        var publicationId = Guid.NewGuid();
        var latestPublishedVersionId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Id = publicationId,
                    LatestPublishedReleaseVersion = new ReleaseVersion()
                },
                Owner = true,
                Methodology = new Methodology
                {
                    LatestPublishedVersionId = latestPublishedVersionId,
                    Versions = ListOf(new MethodologyVersion
                    {
                        Id = latestPublishedVersionId,
                    }),
                    OwningPublicationTitle = "Original title",
                    OwningPublicationSlug = "original-slug",
                }
            };

            await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsCacheService = new Mock<IRedirectsCacheService>(MockBehavior.Strict);
            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var service = SetupMethodologyService(contentDbContext,
                redirectsCacheService: redirectsCacheService.Object);
            await service.PublicationTitleOrSlugChanged(publicationId, "original-slug", "New Title", "new-slug");
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = await contentDbContext
                .PublicationMethodologies
                .Include(m => m.Methodology)
                .SingleAsync(m => m.PublicationId == publicationId);

            Assert.Equal("New Title", publicationMethodology.Methodology.OwningPublicationTitle);
            Assert.Equal("new-slug", publicationMethodology.Methodology.OwningPublicationSlug);

            var redirect = await contentDbContext
                .MethodologyRedirects
                .SingleAsync();

            Assert.Equal(latestPublishedVersionId, redirect.MethodologyVersionId);
            Assert.Equal("original-slug", redirect.Slug);
        }
    }

    [Fact]
    public async Task
        PublicationTitleOrSlugChanged_CurrentInheritedPubSlugChangesWithUnpublishedAmendmentWithAlternativeSlug()
    {
        var publicationId = Guid.NewGuid();
        var latestPublishedVersionId = Guid.NewGuid();
        var latestVersionId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Id = publicationId,
                    LatestPublishedReleaseVersion = new ReleaseVersion()
                },
                Owner = true,
                Methodology = new Methodology
                {
                    LatestPublishedVersionId = latestPublishedVersionId,
                    Versions = new List<MethodologyVersion>
                    {
                        new()
                        {
                            Id = latestPublishedVersionId,
                            Version = 0,
                        },
                        new()
                        {
                            Id = latestVersionId,
                            Version = 1,
                            AlternativeSlug = "methodology-alternative-slug",
                            PreviousVersionId = latestPublishedVersionId,
                        }
                    },
                    OwningPublicationTitle = "Current title",
                    OwningPublicationSlug = "current-slug",
                }
            };

            var methodologyRedirect = new MethodologyRedirect
            {
                // This would have been created when the unpublished amendment's AlternativeSlug was set.
                MethodologyVersionId = latestVersionId,
                Slug = "current-slug",
            };

            await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
            await contentDbContext.MethodologyRedirects.AddAsync(methodologyRedirect);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsCacheService = new Mock<IRedirectsCacheService>(MockBehavior.Strict);
            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var service = SetupMethodologyService(contentDbContext,
                redirectsCacheService: redirectsCacheService.Object);
            await service.PublicationTitleOrSlugChanged(publicationId, "current-slug",
                "New Title", "new-slug");
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = await contentDbContext
                .PublicationMethodologies
                .Include(m => m.Methodology.Versions)
                .SingleAsync(m => m.PublicationId == publicationId);

            Assert.Equal("New Title", publicationMethodology.Methodology.OwningPublicationTitle);
            Assert.Equal("new-slug", publicationMethodology.Methodology.OwningPublicationSlug);

            var redirects = await contentDbContext
                .MethodologyRedirects
                .ToListAsync();

            Assert.Equal(2, redirects.Count);

            // "current-slug" redirect for latestVersion is removed, as a "current-slug" redirect has been added
            // for the latestPublishedVersion. It must be removed to ensure if latestVersion sets an
            // AlternativeSlug in the future, a redirect is created.

            Assert.Equal(latestPublishedVersionId, redirects[0].MethodologyVersionId);
            Assert.Equal("current-slug", redirects[0].Slug);

            // A new redirect for the unpublished amendment, as otherwise we have no redirect for
            // "new-slug" once the amendment is published.
            Assert.Equal(latestVersionId, redirects[1].MethodologyVersionId);
            Assert.Equal("new-slug", redirects[1].Slug);
        }
    }

    [Fact]
    public async Task
        PublicationTitleOrSlugChanged_UnpublishedMethodologyAmendmentDoesNotNeedRedirectIfRedirectAlreadyExists()
    {
        var publicationId = Guid.NewGuid();
        var latestPublishedVersionId = Guid.NewGuid();
        var latestVersionId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = new PublicationMethodology
            {
                Publication = new Publication
                {
                    Id = publicationId,
                    LatestPublishedReleaseVersion = new ReleaseVersion()
                },
                Owner = true,
                Methodology = new Methodology
                {
                    LatestPublishedVersionId = latestPublishedVersionId,
                    Versions = new List<MethodologyVersion>
                    {
                        new()
                        {
                            Id = latestPublishedVersionId,
                            Version = 0,
                        },
                        new()
                        {
                            Id = latestVersionId,
                            Version = 1,
                            AlternativeSlug = "methodology-alternative-slug",
                            PreviousVersionId = latestPublishedVersionId,
                        }
                    },
                    OwningPublicationTitle = "Current title",
                    OwningPublicationSlug = "current-slug",
                }
            };

            var methodologyRedirect1 = new MethodologyRedirect
            {
                // This would have been created when the unpublished amendment's AlternativeSlug was set.
                MethodologyVersionId = latestVersionId,
                Slug = "current-slug",
            };

            var methodologyRedirect2 = new MethodologyRedirect
            {
                // This redirect could exist if the pub slug has been changed multiple times
                MethodologyVersionId = latestPublishedVersionId,
                Slug = "old-title",
            };

            await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
            await contentDbContext.MethodologyRedirects.AddRangeAsync(methodologyRedirect1, methodologyRedirect2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var redirectsCacheService = new Mock<IRedirectsCacheService>(MockBehavior.Strict);
            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var service = SetupMethodologyService(contentDbContext,
                redirectsCacheService: redirectsCacheService.Object);

            // We're changing the slug back to a previously used slug
            await service.PublicationTitleOrSlugChanged(publicationId, "current-slug",
                "Old title", "old-title");
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var publicationMethodology = await contentDbContext
                .PublicationMethodologies
                .Include(m => m.Methodology.Versions)
                .SingleAsync(m => m.PublicationId == publicationId);

            Assert.Equal("Old title", publicationMethodology.Methodology.OwningPublicationTitle);
            Assert.Equal("old-title", publicationMethodology.Methodology.OwningPublicationSlug);

            var redirects = await contentDbContext
                .MethodologyRedirects
                .ToListAsync();

            Assert.Equal(2, redirects.Count);

            // "current-slug" redirect for latestVersion is removed, as a "current-slug" redirect has been added
            // for the latestPublishedVersion

            var latestPublishedVersionRedirect = redirects.Single(mr =>
                mr.MethodologyVersionId == latestPublishedVersionId);
            Assert.Equal("current-slug", latestPublishedVersionRedirect.Slug);

            // We must add an inactive redirect to unpublished amendment, otherwise when it is published
            // there would be no redirect from "old-title".
            var latestVersionRedirect = redirects.Single(mr =>
                mr.MethodologyVersionId == latestVersionId);
            Assert.Equal("old-title", latestVersionRedirect.Slug);
        }
    }

    private static MethodologyService SetupMethodologyService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IMethodologyVersionRepository? methodologyVersionRepository = null,
        IMethodologyRepository? methodologyRepository = null,
        IMethodologyImageService? methodologyImageService = null,
        IMethodologyApprovalService? methodologyApprovalService = null,
        IMethodologyCacheService? methodologyCacheService = null,
        IRedirectsCacheService? redirectsCacheService = null,
        IUserService? userService = null)

    {
        return new(
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            contentDbContext,
            MapperUtils.AdminMapper(),
            methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(Strict),
            methodologyRepository ?? Mock.Of<IMethodologyRepository>(Strict),
            methodologyImageService ?? Mock.Of<IMethodologyImageService>(Strict),
            methodologyApprovalService ?? Mock.Of<IMethodologyApprovalService>(Strict),
            methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
            redirectsCacheService ?? Mock.Of<IRedirectsCacheService>(Strict),
            userService ?? AlwaysTrueUserService(User.Id).Object);
    }
}

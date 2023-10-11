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
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyServiceTests
    {
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
                        Slug = "test-publication",
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
                    Status = MethodologyApprovalStatus.Draft
                };

                methodologyVersionRepository
                    .Setup(s => s.CreateMethodologyForPublication(MockPublication.Id, User.Id))
                    .ReturnsAsync(createdMethodology);

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
                Assert.Equal(MethodologyApprovalStatus.Draft, viewModel.Status);
                Assert.Equal("Test publication", viewModel.Title);

                Assert.Equal(MockPublication.Id, viewModel.OwningPublication.Id);
                Assert.Equal("Test publication", viewModel.OwningPublication.Title);
                Assert.Empty(viewModel.OtherPublications);
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
                Slug = "test-publication",
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
                },
                Contact = MockContact
            };

            var methodologyVersion = new MethodologyVersion
            {
                Methodology = methodology,
                Published = null,
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Draft,
                AlternativeTitle = "Alternative title"
            };

            var methodologyStatus = new MethodologyStatus
            {
                MethodologyVersion = methodologyVersion,
                InternalReleaseNote = "Test approval",
                ApprovalStatus = MethodologyApprovalStatus.Approved,
            };

            var adoptingPublication = new Publication()
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
                Assert.Equal("test-publication", viewModel.Slug);
                Assert.False(viewModel.Amendment);
                Assert.Equal("Test approval", viewModel.InternalReleaseNote);
                Assert.Equal(methodologyVersion.MethodologyId, viewModel.MethodologyId);
                Assert.Null(viewModel.Published);
                Assert.Equal(MethodologyPublishingStrategy.Immediately, viewModel.PublishingStrategy);
                Assert.Equal(MethodologyApprovalStatus.Draft, viewModel.Status);
                Assert.Equal("Alternative title", viewModel.Title);

                Assert.Equal(publication.Id, viewModel.OwningPublication.Id);
                Assert.Equal("Owning publication", viewModel.OwningPublication.Title);
                Assert.Empty(viewModel.OtherPublications);
            }
        }

        [Fact]
        public async Task GetAdoptableMethodologies_NoUnpublishedMethodologies()
        {
            var methodology = new Methodology
            {
                LatestPublishedVersionId = null, // methodology is unpublished
                Slug = "test-publication",
                Versions = new List<MethodologyVersion>
                {
                    new()
                    {
                        Published = null,
                        PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                        Status = MethodologyApprovalStatus.Draft,
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
            var methodology = new Methodology
            {
                Slug = "test-publication"
            };

            var owningPublication = new Publication
            {
                Title = "Owning publication",
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

            var adoptingPublication1 = new Publication
            {
                Title = "Adopting publication 1",
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = methodology,
                        Owner = false
                    }
                ),
                Contact = MockContact
            };

            var adoptingPublication2 = new Publication
            {
                Title = "Adopting publication 2",
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = methodology,
                        Owner = false
                    }
                ),
                Contact = MockContact
            };

            var methodologyVersion = new MethodologyVersion
            {
                Methodology = methodology,
                Published = new DateTime(2020, 5, 25),
                PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                ScheduledWithRelease = new Release
                {
                    Publication = owningPublication,
                    TimePeriodCoverage = CalendarYear,
                    ReleaseName = "2021"
                },
                Status = MethodologyApprovalStatus.Approved,
                AlternativeTitle = "Alternative title"
            };

            var methodologyStatus = new MethodologyStatus
            {
                MethodologyVersion = methodologyVersion,
                InternalReleaseNote = "Test approval",
                ApprovalStatus = MethodologyApprovalStatus.Approved,
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddAsync(methodology);
                await context.Publications.AddRangeAsync(owningPublication, adoptingPublication1, adoptingPublication2);
                await context.Contacts.AddAsync(MockContact);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.MethodologyStatus.AddAsync(methodologyStatus);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var viewModel = (await service.GetMethodology(methodologyVersion.Id)).AssertRight();

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("test-publication", viewModel.Slug);
                Assert.False(viewModel.Amendment);
                Assert.Equal("Test approval", viewModel.InternalReleaseNote);
                Assert.Equal(methodologyVersion.MethodologyId, viewModel.MethodologyId);
                Assert.Equal(new DateTime(2020, 5, 25), viewModel.Published);
                Assert.Equal(MethodologyPublishingStrategy.WithRelease, viewModel.PublishingStrategy);
                Assert.Equal(MethodologyApprovalStatus.Approved, viewModel.Status);
                Assert.Equal("Alternative title", viewModel.Title);

                Assert.Equal(owningPublication.Id, viewModel.OwningPublication.Id);
                Assert.Equal("Owning publication", viewModel.OwningPublication.Title);

                Assert.Equal(2, viewModel.OtherPublications.Count);
                Assert.Equal(adoptingPublication1.Id, viewModel.OtherPublications[0].Id);
                Assert.Equal("Adopting publication 1", viewModel.OtherPublications[0].Title);
                Assert.Equal(adoptingPublication2.Id, viewModel.OtherPublications[1].Id);
                Assert.Equal("Adopting publication 2", viewModel.OtherPublications[1].Title);

                Assert.NotNull(viewModel.ScheduledWithRelease);
                Assert.Equal(methodologyVersion.ScheduledWithReleaseId, viewModel.ScheduledWithRelease!.Id);
                Assert.Equal("Owning publication - Calendar year 2021", viewModel.ScheduledWithRelease.Title);
            }
        }

        [Fact]
        public async Task GetUnpublishedReleasesUsingMethodology()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Methodology = new Methodology()
            };

            // Set up a randomly ordered mix of published and unpublished Releases on owning and adopting publications

            var owningPublication = new Publication
            {
                Title = "Publication B",
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = methodologyVersion.Methodology,
                        Owner = true
                    }
                ),
                Releases = ListOf(
                    new Release
                    {
                        Published = DateTime.UtcNow,
                        TimePeriodCoverage = CalendarYear,
                        ReleaseName = "2018"
                    },
                    new Release
                    {
                        Published = null,
                        TimePeriodCoverage = CalendarYear,
                        ReleaseName = "2021"
                    },
                    new Release
                    {
                        Published = DateTime.UtcNow,
                        TimePeriodCoverage = CalendarYear,
                        ReleaseName = "2019"
                    },
                    new Release
                    {
                        Published = null,
                        TimePeriodCoverage = CalendarYear,
                        ReleaseName = "2020"
                    }
                )
            };

            var adoptingPublication = new Publication
            {
                Title = "Publication A",
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = methodologyVersion.Methodology,
                        Owner = false
                    }
                ),
                Releases = ListOf(
                    new Release
                    {
                        Published = DateTime.UtcNow,
                        TimePeriodCoverage = FinancialYearQ3,
                        ReleaseName = "2020"
                    },
                    new Release
                    {
                        Published = null,
                        TimePeriodCoverage = FinancialYearQ2,
                        ReleaseName = "2021"
                    },
                    new Release
                    {
                        Published = null,
                        TimePeriodCoverage = FinancialYearQ4,
                        ReleaseName = "2020"
                    },
                    new Release
                    {
                        Published = null,
                        TimePeriodCoverage = FinancialYearQ1,
                        ReleaseName = "2021"
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.Publications.AddRangeAsync(owningPublication, adoptingPublication);
                await contentDbContext.Contacts.AddAsync(MockContact);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext);

                var result = (await service.GetUnpublishedReleasesUsingMethodology(methodologyVersion.Id))
                    .AssertRight();

                // Check that only unpublished Releases are included and that they are in the correct order

                var expectedReleaseAtIndex0 = adoptingPublication.Releases.Single(r =>
                    r.Year == 2021 && r.TimePeriodCoverage == FinancialYearQ2);

                var expectedReleaseAtIndex1 = adoptingPublication.Releases.Single(r =>
                    r.Year == 2021 && r.TimePeriodCoverage == FinancialYearQ1);

                var expectedReleaseAtIndex2 = adoptingPublication.Releases.Single(r =>
                    r.Year == 2020 && r.TimePeriodCoverage == FinancialYearQ4);

                var expectedReleaseAtIndex3 = owningPublication.Releases.Single(r =>
                    r.Year == 2021 && r.TimePeriodCoverage == CalendarYear);

                var expectedReleaseAtIndex4 = owningPublication.Releases.Single(r =>
                    r.Year == 2020 && r.TimePeriodCoverage == CalendarYear);

                Assert.Equal(5, result.Count);

                Assert.Equal(expectedReleaseAtIndex0.Id, result[0].Id);
                Assert.Equal(expectedReleaseAtIndex1.Id, result[1].Id);
                Assert.Equal(expectedReleaseAtIndex2.Id, result[2].Id);
                Assert.Equal(expectedReleaseAtIndex3.Id, result[3].Id);
                Assert.Equal(expectedReleaseAtIndex4.Id, result[4].Id);

                Assert.Equal("Publication A - Financial year Q2 2021-22", result[0].Title);
                Assert.Equal("Publication A - Financial year Q1 2021-22", result[1].Title);
                Assert.Equal("Publication A - Financial year Q4 2020-21", result[2].Title);
                Assert.Equal("Publication B - Calendar year 2021", result[3].Title);
                Assert.Equal("Publication B - Calendar year 2020", result[4].Title);
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
            var methodologyVersion = new MethodologyVersion
            {
                Methodology = new Methodology()
            };

            var owningPublication = new Publication
            {
                Title = "Owning publication",
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = methodologyVersion.Methodology,
                        Owner = true
                    }
                )
            };

            var adoptingPublication = new Publication
            {
                Title = "Adopting publication",
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = methodologyVersion.Methodology,
                        Owner = false
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.Publications.AddRangeAsync(owningPublication, adoptingPublication);
                await contentDbContext.Contacts.AddAsync(MockContact);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext);

                var result = (await service.GetUnpublishedReleasesUsingMethodology(methodologyVersion.Id))
                    .AssertRight();

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetUnpublishedReleasesUsingMethodology_PublicationsHaveNoUnpublishedReleases()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Methodology = new Methodology()
            };

            var owningPublication = new Publication
            {
                Title = "Owning publication",
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = methodologyVersion.Methodology,
                        Owner = true
                    }
                ),
                Releases = ListOf(
                    new Release
                    {
                        Published = DateTime.UtcNow,
                        TimePeriodCoverage = CalendarYear,
                        ReleaseName = "2021"
                    }
                )
            };

            var adoptingPublication = new Publication
            {
                Title = "Adopting publication",
                Methodologies = ListOf(
                    new PublicationMethodology
                    {
                        Methodology = methodologyVersion.Methodology,
                        Owner = false
                    }
                ),
                Releases = ListOf(
                    new Release
                    {
                        Published = DateTime.UtcNow,
                        TimePeriodCoverage = CalendarYear,
                        ReleaseName = "2021"
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.Publications.AddRangeAsync(owningPublication, adoptingPublication);
                await contentDbContext.Contacts.AddAsync(MockContact);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext);

                var result = (await service.GetUnpublishedReleasesUsingMethodology(methodologyVersion.Id))
                    .AssertRight();

                Assert.Empty(result);
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
                        Status = MethodologyApprovalStatus.Approved,
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
                        Status = MethodologyApprovalStatus.Approved,
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Version = 1,
                        AlternativeTitle = "Methodology 2 Version 2",
                        Published = null,
                        Status = MethodologyApprovalStatus.Draft,
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
                        Status = MethodologyApprovalStatus.Approved,
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Version = 1,
                        AlternativeTitle = "Methodology 3 Version 2",
                        Published = new DateTime(2022, 1, 1),
                        Status = MethodologyApprovalStatus.Approved,
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
                        Status = MethodologyApprovalStatus.Draft,
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
                Assert.Equal(MethodologyApprovalStatus.Approved, viewModels[0].Status);
                Assert.Equal("Methodology 1 Version 1", viewModels[0].Title);
                Assert.Equal(methodology1.Id, viewModels[0].MethodologyId);
                Assert.Null(viewModels[0].PreviousVersionId);

                Assert.Equal(methodology2.Versions[1].Id, viewModels[1].Id);
                Assert.True(viewModels[1].Amendment);
                Assert.False(viewModels[1].Owned);
                Assert.Null(viewModels[1].Published);
                Assert.Equal(MethodologyApprovalStatus.Draft, viewModels[1].Status);
                Assert.Equal("Methodology 2 Version 2", viewModels[1].Title);
                Assert.Equal(methodology2.Id, viewModels[1].MethodologyId);
                Assert.Equal(methodology2.Versions[0].Id, viewModels[1].PreviousVersionId);

                Assert.Equal(methodology3.Versions[1].Id, viewModels[2].Id);
                Assert.False(viewModels[2].Amendment);
                Assert.False(viewModels[2].Owned);
                Assert.Equal(new DateTime(2022, 1, 1), viewModels[2].Published);
                Assert.Equal(MethodologyApprovalStatus.Approved, viewModels[2].Status);
                Assert.Equal("Methodology 3 Version 2", viewModels[2].Title);
                Assert.Equal(methodology3.Id, viewModels[2].MethodologyId);
                Assert.Equal(methodology3.Versions[0].Id, viewModels[2].PreviousVersionId);

                Assert.Equal(methodology4.Versions[0].Id, viewModels[3].Id);
                Assert.False(viewModels[3].Amendment);
                Assert.True(viewModels[3].Owned);
                Assert.Equal(new DateTime(2021, 1, 1), viewModels[3].Published);
                Assert.Equal(MethodologyApprovalStatus.Draft, viewModels[3].Status);
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
                        Status = MethodologyApprovalStatus.Approved,
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
                        Status = MethodologyApprovalStatus.Approved,
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Version = 1,
                        AlternativeTitle = "Methodology 2 Version 2",
                        Published = null,
                        Status = MethodologyApprovalStatus.Draft,
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
                        Status = MethodologyApprovalStatus.Approved,
                    },
                    new()
                    {
                        // This is in results because it is latest approved
                        Id = Guid.NewGuid(),
                        Version = 1,
                        AlternativeTitle = "Methodology 3 Version 2",
                        Published = new DateTime(2022, 1, 1),
                        Status = MethodologyApprovalStatus.Approved,
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
                        Status = MethodologyApprovalStatus.Draft,
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
                Assert.Equal(MethodologyApprovalStatus.Approved, viewModels[0].Status);
                Assert.Equal("Methodology 1 Version 1", viewModels[0].Title);
                Assert.Equal(methodology1.Id, viewModels[0].MethodologyId);
                Assert.Null(viewModels[0].PreviousVersionId);

                Assert.Equal(methodology2.Versions[0].Id, viewModels[1].Id);
                Assert.False(viewModels[1].Amendment);
                Assert.False(viewModels[1].Owned);
                Assert.Equal(new DateTime(2021, 1, 1), viewModels[1].Published);
                Assert.Equal(MethodologyApprovalStatus.Approved, viewModels[1].Status);
                Assert.Equal("Methodology 2 Version 1", viewModels[1].Title);
                Assert.Equal(methodology2.Id, viewModels[1].MethodologyId);
                Assert.Null(viewModels[1].PreviousVersionId);

                Assert.Equal(methodology3.Versions[1].Id, viewModels[2].Id);
                Assert.False(viewModels[2].Amendment);
                Assert.False(viewModels[2].Owned);
                Assert.Equal(new DateTime(2022, 1, 1), viewModels[2].Published);
                Assert.Equal(MethodologyApprovalStatus.Approved, viewModels[2].Status);
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
                                    Status = MethodologyApprovalStatus.Approved,
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
                Contact = MockContact
            };

            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Draft,
                Methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    Slug = "test-publication",
                    OwningPublicationTitle = "Test publication",
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
                Status = MethodologyApprovalStatus.Draft,
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
                var service = SetupMethodologyService(context);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

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
                Assert.Empty(viewModel.OtherPublications);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(MethodologyApprovalStatus.Draft, updatedMethodology.Status);
                Assert.Equal(MethodologyPublishingStrategy.Immediately, updatedMethodology.PublishingStrategy);
                Assert.Equal("Updated Methodology Title", updatedMethodology.Title);
                Assert.Equal("Updated Methodology Title", updatedMethodology.AlternativeTitle);
                Assert.Equal("updated-methodology-title", updatedMethodology.Slug);
                Assert.Equal("updated-methodology-title", updatedMethodology.Methodology.Slug);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateMethodology_UpdatingAmendmentSoSlugDoesNotChange()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Draft,
                Methodology = new Methodology
                {
                    Slug = "test-publication",
                    OwningPublicationTitle = "Test publication",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = MockPublication
                    })
                },
                PreviousVersionId = Guid.NewGuid()
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Draft,
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
                var service = SetupMethodologyService(context);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

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
                Assert.Empty(viewModel.OtherPublications);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(MethodologyApprovalStatus.Draft, updatedMethodology.Status);
                Assert.Equal(MethodologyPublishingStrategy.Immediately, updatedMethodology.PublishingStrategy);
                Assert.Equal("Updated Methodology Title", updatedMethodology.Title);
                Assert.Equal("Updated Methodology Title", updatedMethodology.AlternativeTitle);
                Assert.Equal("test-publication", updatedMethodology.Slug);
                Assert.Equal("test-publication", updatedMethodology.Methodology.Slug);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateMethodology_UpdatingTitleToMatchPublicationTitleUnsetsAlternativeTitle()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Alternative Methodology Title",
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Draft,
                Methodology = new Methodology
                {
                    Slug = "test-publication",
                    OwningPublicationTitle = "Test publication",
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
                Status = MethodologyApprovalStatus.Draft,
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
                var service = SetupMethodologyService(context);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

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
                Assert.Empty(viewModel.OtherPublications);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(MethodologyApprovalStatus.Draft, updatedMethodology.Status);
                Assert.Equal(MethodologyPublishingStrategy.Immediately, updatedMethodology.PublishingStrategy);
                Assert.Equal(MockPublication.Title, updatedMethodology.Title);

                // Test explicitly that AlternativeTitle has been unset.
                Assert.Null(updatedMethodology.AlternativeTitle);

                Assert.Equal("test-publication", updatedMethodology.Slug);
                Assert.Equal("test-publication", updatedMethodology.Methodology.Slug);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateMethodology_UpdatingAmendmentSoSlugDoesNotChange_AndUnsetsAlternativeTitle()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                AlternativeTitle = "Alternative Methodology Title",
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Draft,
                Methodology = new Methodology
                {
                    Slug = "alternative-methodology-title",
                    OwningPublicationTitle = "Test publication",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = MockPublication
                    })
                },
                PreviousVersionId = Guid.NewGuid()
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Draft,
                Title = MockPublication.Title
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.Contacts.AddAsync(MockContact);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(context);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("alternative-methodology-title", viewModel.Slug);
                Assert.Null(viewModel.InternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(MethodologyPublishingStrategy.Immediately, viewModel.PublishingStrategy);
                Assert.Null(viewModel.ScheduledWithRelease);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
                Assert.Equal(MockPublication.Id, viewModel.OwningPublication.Id);
                Assert.Equal(MockPublication.Title, viewModel.OwningPublication.Title);
                Assert.Empty(viewModel.OtherPublications);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(MethodologyApprovalStatus.Draft, updatedMethodology.Status);
                Assert.Equal(MethodologyPublishingStrategy.Immediately, updatedMethodology.PublishingStrategy);
                Assert.Equal(MockPublication.Title, updatedMethodology.Title);

                // Test that the AlternativeTitle has explicitly be set to null.
                Assert.Null(updatedMethodology.AlternativeTitle);

                Assert.Equal("alternative-methodology-title", updatedMethodology.Slug);
                Assert.Equal("alternative-methodology-title", updatedMethodology.Methodology.Slug);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
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
                Status = MethodologyApprovalStatus.Draft,
                Methodology = new Methodology
                {
                    Slug = "test-publication",
                    OwningPublicationTitle = "Test publication",
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
                Status = MethodologyApprovalStatus.Draft,
                Methodology = new Methodology
                {
                    Slug = "updated-methodology-title",
                    OwningPublicationTitle = "Test publication 2"
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Draft,
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
                var service = SetupMethodologyService(context);

                var result = await service.UpdateMethodology(methodologyVersion.Id, request);
                result.AssertBadRequest(SlugNotUnique);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var notUpdatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(notUpdatedMethodology.Published);
                Assert.Equal(MethodologyApprovalStatus.Draft, notUpdatedMethodology.Status);
                Assert.Equal(MethodologyPublishingStrategy.Immediately, notUpdatedMethodology.PublishingStrategy);
                Assert.Equal("Test publication", notUpdatedMethodology.Title);
                Assert.Null(notUpdatedMethodology.AlternativeTitle);
                Assert.Equal("test-publication", notUpdatedMethodology.Slug);
                Assert.Equal("test-publication", notUpdatedMethodology.Methodology.Slug);
                Assert.False(notUpdatedMethodology.Updated.HasValue);
            }
        }

        [Fact]
        public async Task UpdateMethodology_StatusUpdate()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Draft,
                Methodology = new Methodology
                {
                    Id = Guid.NewGuid(),
                    Slug = "test-publication",
                    OwningPublicationTitle = "Test publication",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = MockPublication
                    })
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Approved",
                PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                Status = MethodologyApprovalStatus.Approved,
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
                var methodologyApprovalService = new Mock<IMethodologyApprovalService>();

                methodologyApprovalService
                    .Setup(s => s.UpdateApprovalStatus(methodologyVersion.Id, request))
                    .ReturnsAsync(methodologyVersion);

                var service = SetupMethodologyService(
                    context,
                    methodologyApprovalService: methodologyApprovalService.Object);

                await service.UpdateMethodology(methodologyVersion.Id, request);

                // Verify that the call to update the approval status happened.
                VerifyAllMocks(methodologyApprovalService);
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
                Assert.Equal("updated-methodology-title", updatedMethodology.Methodology.Slug);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
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
                Status = MethodologyApprovalStatus.Draft,
                Methodology = new Methodology
                {
                    Id = methodologyId,
                    Slug = "pupil-absence-statistics-methodology",
                    OwningPublicationTitle = "Pupil absence statistics: methodology",
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
                Slug = "pupil-absence-statistics-methodology",
                OwningPublicationTitle = "Pupil absence statistics: methodology",
                Versions = ListOf(new MethodologyVersion
                    {
                        Id = Guid.NewGuid(),
                        PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                        Status = MethodologyApprovalStatus.Draft
                    },
                    new MethodologyVersion
                    {
                        Id = Guid.NewGuid(),
                        PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                        Status = MethodologyApprovalStatus.Draft
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
                Slug = "pupil-absence-statistics-methodology",
                OwningPublicationTitle = "Pupil absence statistics: methodology",
                Versions = ListOf(new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                    Status = MethodologyApprovalStatus.Draft
                })
            };

            var unrelatedMethodology = new Methodology
            {
                Id = unrelatedMethodologyId,
                Slug = "pupil-absence-statistics-methodology",
                OwningPublicationTitle = "Pupil absence statistics: methodology",
                Versions = ListOf(new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                    Status = MethodologyApprovalStatus.Draft
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
                Slug = "pupil-absence-statistics-methodology",
                OwningPublicationTitle = "Pupil absence statistics: methodology",
                Versions = ListOf(
                    new MethodologyVersion
                    {
                        PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                        Status = MethodologyApprovalStatus.Draft,
                        Version = 0,
                    },
                    new MethodologyVersion
                    {
                        PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                        Status = MethodologyApprovalStatus.Approved,
                        Version = 1,
                    }
                ),
            };

            var unrelatedMethodology = new Methodology
            {
                Id = unrelatedMethodologyId,
                Slug = "pupil-absence-statistics-methodology",
                OwningPublicationTitle = "Pupil absence statistics: methodology",
                Versions = ListOf(new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                    Status = MethodologyApprovalStatus.Draft
                })
            };

            var methodologyStatuses = new List<MethodologyStatus>
            {
                new()
                {
                    MethodologyVersion = methodology.Versions[0],
                    InternalReleaseNote = "Status 1 note",
                    ApprovalStatus = MethodologyApprovalStatus.Approved,
                    Created = new DateTime(2000, 1, 1),
                    CreatedById = User.Id,
                },
                new()
                {
                    MethodologyVersion = methodology.Versions[1],
                    InternalReleaseNote = "Status 2 note",
                    ApprovalStatus = MethodologyApprovalStatus.Approved,
                    Created = new DateTime(2001, 1, 1),
                    CreatedById = User.Id,
                },
                new()
                {
                    MethodologyVersion = unrelatedMethodology.Versions[0],
                    InternalReleaseNote = "Unrelated note",
                    ApprovalStatus = MethodologyApprovalStatus.Approved,
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
                Slug = "pupil-absence-statistics-methodology",
                OwningPublicationTitle = "Pupil absence statistics: methodology",
                Versions = ListOf(
                    new MethodologyVersion
                    {
                        PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                        Status = MethodologyApprovalStatus.Draft,
                    },
                    new MethodologyVersion
                    {
                        PublishingStrategy = MethodologyPublishingStrategy.WithRelease,
                        Status = MethodologyApprovalStatus.Approved,
                    }
                ),
            };

            var unrelatedMethodology = new Methodology
            {
                Id = unrelatedMethodologyId,
                Slug = "pupil-absence-statistics-methodology",
                OwningPublicationTitle = "Pupil absence statistics: methodology",
                Versions = ListOf(new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    PublishingStrategy = MethodologyPublishingStrategy.Immediately,
                    Status = MethodologyApprovalStatus.Draft
                })
            };

            var methodologyStatuses = new List<MethodologyStatus>
            {
                new()
                {
                    MethodologyVersion = unrelatedMethodology.Versions[0],
                    InternalReleaseNote = "Unrelated note",
                    ApprovalStatus = MethodologyApprovalStatus.Approved,
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
                    .WithContact(MockContact)
                    .Generate();
                
                var methodology = _fixture
                    .DefaultMethodology()
                    .WithOwningPublication(publication)
                    .WithMethodologyVersions(_ => _fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
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
                        .WithApprovalStatus(MethodologyApprovalStatus.Draft)
                        .Generate(1)))
                    .ForIndex(1, s => s.SetMethodologyVersions(_fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.Approved)
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
                    .WithAdoptingPublication(publication)
                    .WithMethodologyVersions(_ => _fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
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
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
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
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
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
                var release = _fixture.DefaultRelease().Generate();

                var publication = _fixture
                    .DefaultPublication()
                    .WithContact(MockContact)
                    .WithReleases(ListOf(release))
                    .Generate();

                var methodology = _fixture
                    .DefaultMethodology()
                    .WithOwningPublication(publication)
                    .WithMethodologyVersions(_ => _fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
                        .Generate(1))
                    .Generate();

                var releaseRoleForUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(User)
                    .WithRelease(release)
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
                var releases = _fixture
                    .DefaultRelease()
                    .WithApprovalStatuses(ListOf(
                        ReleaseApprovalStatus.Approved, 
                        ReleaseApprovalStatus.Draft))
                    .GenerateList();

                var publication = _fixture
                    .DefaultPublication()
                    .WithContact(MockContact)
                    .WithReleases(releases)
                    .Generate();

                var methodology = _fixture
                    .DefaultMethodology()
                    .WithOwningPublication(publication)
                    .WithMethodologyVersions(_ => _fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
                        .Generate(1))
                    .Generate();

                var releaseRoleForUserOnOldRelease = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(User)
                    .WithRelease(releases[0])
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
                var release = _fixture.DefaultRelease().Generate();

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
                        .WithApprovalStatus(MethodologyApprovalStatus.Draft)
                        .Generate(1)))
                    .ForIndex(1, s => s.SetMethodologyVersions(_fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.Approved)
                        .Generate(1)))
                    .GenerateList();

                var releaseRoleForUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(User)
                    .WithRelease(release)
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
                var release = _fixture.DefaultRelease().Generate();

                var publication = _fixture
                    .DefaultPublication()
                    .WithContact(MockContact)
                    .Generate();

                // Create a Methodology that has only been adopted by the User's Publication.
                var methodology = _fixture
                    .DefaultMethodology()
                    .WithAdoptingPublication(publication)
                    .WithMethodologyVersions(_ => _fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
                        .Generate(1))
                    .Generate();

                var releaseRoleForUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(User)
                    .WithRelease(release)
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
                var release = _fixture.DefaultRelease().Generate();

                var publication = _fixture
                    .DefaultPublication()
                    .Generate();

                var methodology = _fixture
                    .DefaultMethodology()
                    .WithOwningPublication(publication)
                    .WithMethodologyVersions(_ => _fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
                        .Generate(1))
                    .Generate();

                // Set up the User as a Contributor on the Methodology's Publication's Release rather than an Approver.
                var releaseRoleForUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(User)
                    .WithRelease(release)
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

                var release = _fixture.DefaultRelease().Generate();

                var publication = _fixture
                    .DefaultPublication()
                    .WithContact(MockContact)
                    .Generate();

                var methodology = _fixture
                    .DefaultMethodology()
                    .WithOwningPublication(publication)
                    .WithMethodologyVersions(_ => _fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
                        .Generate(1))
                    .Generate();

                var releaseRoleForOtherUser = _fixture
                    .DefaultUserReleaseRole()
                    .WithUser(otherUser)
                    .WithRelease(release)
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
                var release = _fixture.DefaultRelease().Generate();

                var publication = _fixture
                    .DefaultPublication()
                    .WithContact(MockContact)
                    .WithReleases(ListOf(release))
                    .Generate();

                var methodology = _fixture
                    .DefaultMethodology()
                    .WithOwningPublication(publication)
                    .WithMethodologyVersions(_ => _fixture
                        .DefaultMethodologyVersion()
                        .WithApprovalStatus(MethodologyApprovalStatus.HigherLevelReview)
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
                    .WithRelease(release)
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

        private static MethodologyService SetupMethodologyService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IMethodologyRepository? methodologyRepository = null,
            IMethodologyImageService? methodologyImageService = null,
            IMethodologyApprovalService? methodologyApprovalService = null,
            IMethodologyCacheService? methodologyCacheService = null,
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
                userService ?? AlwaysTrueUserService(User.Id).Object);
        }
    }
}

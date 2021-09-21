#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyServiceTests
    {
        private static readonly Guid UserId = Guid.NewGuid();

        [Fact]
        public async Task AdoptMethodology()
        {
            var publication = new Publication();

            // Setup methodology owned by a different publication
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
                await context.Publications.AddAsync(publication);
                await context.Methodologies.AddAsync(methodology);
                await context.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(
                    contentDbContext: context);

                var result = await service.AdoptMethodology(publication.Id, methodology.Id);

                VerifyAllMocks(methodologyRepository);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationMethodologies = await context.PublicationMethodologies.ToListAsync();

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
        public async Task AdoptMethodology_AlreadyAdoptedByPublicationFails()
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
                await context.Methodologies.AddAsync(methodology);
                await context.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(
                    contentDbContext: context);

                var result = await service.AdoptMethodology(publication.Id, methodology.Id);

                VerifyAllMocks(methodologyRepository);

                result.AssertBadRequest(CannotAdoptMethodologyAlreadyLinkedToPublication);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationMethodologies = await context.PublicationMethodologies.ToListAsync();

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
                Publications = new List<PublicationMethodology>
                {
                    new()
                    {
                        Publication = publication,
                        Owner = true
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.Methodologies.AddAsync(methodology);
                await context.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(
                    contentDbContext: context);

                var result = await service.AdoptMethodology(publication.Id, methodology.Id);

                VerifyAllMocks(methodologyRepository);

                result.AssertBadRequest(CannotAdoptMethodologyAlreadyLinkedToPublication);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationMethodologies = await context.PublicationMethodologies.ToListAsync();

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

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(
                    contentDbContext: context);

                var result = await service.AdoptMethodology(Guid.NewGuid(), methodology.Id);

                VerifyAllMocks(methodologyRepository);

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
            var publication = new Publication
            {
                Title = "Test publication"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new Mock<IMethodologyVersionRepository>(Strict);

                var service = SetupMethodologyService(
                    context,
                    methodologyVersionRepository: repository.Object);

                var createdMethodology = new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    Methodology = new Methodology
                    {
                        Id = Guid.NewGuid(),
                        Slug = "test-publication",
                        OwningPublicationTitle = publication.Title,
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

                repository
                    .Setup(s => s.CreateMethodologyForPublication(publication.Id, UserId))
                    .ReturnsAsync(createdMethodology);

                context.Attach(createdMethodology);

                var viewModel = (await service.CreateMethodology(publication.Id)).AssertRight();
                VerifyAllMocks(repository);

                Assert.Equal(createdMethodology.Id, viewModel.Id);
                Assert.Equal("test-publication", viewModel.Slug);
                Assert.False(viewModel.Amendment);
                Assert.Null(viewModel.LatestInternalReleaseNote);
                Assert.Equal(createdMethodology.Methodology.Id, viewModel.MethodologyId);
                Assert.Null(viewModel.Published);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
                Assert.Equal(Draft, viewModel.Status);
                Assert.Equal("Test publication", viewModel.Title);

                Assert.Equal(publication.Id, viewModel.OwningPublication.Id);
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
                await context.Methodologies.AddAsync(methodology);
                await context.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(
                    contentDbContext: context);

                var result = await service.DropMethodology(publication.Id, methodology.Id);

                VerifyAllMocks(methodologyRepository);

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationMethodologies = await context.PublicationMethodologies.ToListAsync();

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
                await context.Methodologies.AddAsync(methodology);
                await context.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(
                    contentDbContext: context);

                var result = await service.DropMethodology(publication.Id, methodology.Id);

                VerifyAllMocks(methodologyRepository);

                result.AssertNotFound();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var publicationMethodologies = await context.PublicationMethodologies.ToListAsync();

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

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(
                    contentDbContext: context);

                var result = await service.DropMethodology(Guid.NewGuid(), methodology.Id);

                VerifyAllMocks(methodologyRepository);

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
                await context.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(
                    contentDbContext: context);

                var result = await service.DropMethodology(publication.Id, Guid.NewGuid());

                VerifyAllMocks(methodologyRepository);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetAdoptableMethodologies()
        {
            var methodology = new Methodology
            {
                Slug = "test-publication"
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

            var methodologyVersion = new MethodologyVersion
            {
                Methodology = methodology,
                InternalReleaseNote = "Test approval",
                Published = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                AlternativeTitle = "Alternative title"
            };

            var adoptingPublication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddRangeAsync(publication, adoptingPublication);
                await context.Methodologies.AddAsync(methodology);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            methodologyRepository.Setup(mock =>
                    mock.GetUnrelatedToPublication(adoptingPublication.Id))
                .ReturnsAsync(ListOf(methodology));

            methodologyVersionRepository.Setup(mock => mock.GetLatestVersion(methodology.Id))
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
                Assert.Equal("Test approval", viewModel.LatestInternalReleaseNote);
                Assert.Equal(methodologyVersion.MethodologyId, viewModel.MethodologyId);
                Assert.Null(viewModel.Published);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
                Assert.Equal(Draft, viewModel.Status);
                Assert.Equal("Alternative title", viewModel.Title);

                Assert.Equal(publication.Id, viewModel.OwningPublication.Id);
                Assert.Equal("Owning publication", viewModel.OwningPublication.Title);
                Assert.Empty(viewModel.OtherPublications);
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
                await context.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);

            methodologyRepository.Setup(mock =>
                    mock.GetUnrelatedToPublication(publication.Id))
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
        public async Task GetSummary()
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
                }
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
                )
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
                )
            };

            var methodologyVersion = new MethodologyVersion
            {
                Methodology = methodology,
                InternalReleaseNote = "Test approval",
                Published = new DateTime(2020, 5, 25),
                PublishingStrategy = WithRelease,
                ScheduledWithRelease = new Release
                {
                    Publication = owningPublication,
                    TimePeriodCoverage = CalendarYear,
                    ReleaseName = "2021"
                },
                Status = Approved,
                AlternativeTitle = "Alternative title"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddAsync(methodology);
                await context.Publications.AddRangeAsync(owningPublication, adoptingPublication1, adoptingPublication2);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var viewModel = (await service.GetSummary(methodologyVersion.Id)).AssertRight();

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("test-publication", viewModel.Slug);
                Assert.False(viewModel.Amendment);
                Assert.Equal("Test approval", viewModel.LatestInternalReleaseNote);
                Assert.Equal(methodologyVersion.MethodologyId, viewModel.MethodologyId);
                Assert.Equal(new DateTime(2020, 5, 25), viewModel.Published);
                Assert.Equal(WithRelease, viewModel.PublishingStrategy);
                Assert.Equal(Approved, viewModel.Status);
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
                Assert.Equal("Owning publication - Calendar Year 2021", viewModel.ScheduledWithRelease.Title);
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

                Assert.Equal("Publication A - Financial Year Q2 2021-22", result[0].Title);
                Assert.Equal("Publication A - Financial Year Q1 2021-22", result[1].Title);
                Assert.Equal("Publication A - Financial Year Q4 2020-21", result[2].Title);
                Assert.Equal("Publication B - Calendar Year 2021", result[3].Title);
                Assert.Equal("Publication B - Calendar Year 2020", result[4].Title);
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
        public async Task UpdateMethodology()
        {
            var publication = new Publication
            {
                Title = "Test publication",
                Slug = "test-publication"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
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

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
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
                var service = SetupMethodologyService(context);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("updated-methodology-title", viewModel.Slug);
                Assert.Null(viewModel.LatestInternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
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
                Assert.Equal(Draft, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
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
            var publication = new Publication
            {
                Title = "Test publication",
                Slug = "test-publication"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    Slug = "test-publication",
                    OwningPublicationTitle = "Test publication",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = publication
                    })
                },
                PreviousVersionId = Guid.NewGuid()
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
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
                var service = SetupMethodologyService(context);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("test-publication", viewModel.Slug);
                Assert.Null(viewModel.LatestInternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
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
                Assert.Equal(Draft, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
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
            var publication = new Publication
            {
                Title = "Test publication",
                Slug = "test-publication"
            };

            var methodologyVersion = new MethodologyVersion
            {
                AlternativeTitle = "Alternative Methodology Title",
                PublishingStrategy = Immediately,
                Status = Draft,
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

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
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
                var service = SetupMethodologyService(context);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("test-publication", viewModel.Slug);
                Assert.Null(viewModel.LatestInternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
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
                Assert.Equal(Draft, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.Equal(publication.Title, updatedMethodology.Title);

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
            var publication = new Publication
            {
                Title = "Test publication",
                Slug = "test-publication"
            };

            var methodologyVersion = new MethodologyVersion
            {
                AlternativeTitle = "Alternative Methodology Title",
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    Slug = "alternative-methodology-title",
                    OwningPublicationTitle = "Test publication",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = publication
                    })
                },
                PreviousVersionId = Guid.NewGuid()
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Title = publication.Title
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
                Assert.Equal("alternative-methodology-title", viewModel.Slug);
                Assert.Null(viewModel.LatestInternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
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
                Assert.Equal(Draft, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.Equal(publication.Title, updatedMethodology.Title);

                // Test that the AlternativeTitle has explicitly be set to null.
                Assert.Null(updatedMethodology.AlternativeTitle);

                Assert.Equal("alternative-methodology-title", updatedMethodology.Slug);
                Assert.Equal("alternative-methodology-title", updatedMethodology.Methodology.Slug);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateMethodologyStatus_MethodologyHasImages()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var imageFile1 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = FileType.Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = FileType.Image
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved,
                Title = "Publication title"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>
                {
                    new()
                    {
                        Body = $@"
    <img src=""/api/methodologies/{{methodologyId}}/images/{imageFile1.File.Id}""/>
    <img src=""/api/methodologies/{{methodologyId}}/images/{imageFile2.File.Id}""/>"
                    }
                });

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(contentService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("Test approval", viewModel.LatestInternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
                Assert.Null(viewModel.ScheduledWithRelease);
                Assert.Equal(request.Status, viewModel.Status);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingMethodologyWithUnusedImages()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var imageFile1 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = FileType.Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = FileType.Image
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved,
                Title = "Publication title"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var imageService = new Mock<IMethodologyImageService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            imageService.Setup(mock =>
                    mock.Delete(methodologyVersion.Id, new List<Guid>
                    {
                        imageFile1.File.Id,
                        imageFile2.File.Id
                    }, false))
                .ReturnsAsync(Unit.Instance);

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyImageService: imageService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                imageService.Verify(mock =>
                    mock.Delete(methodologyVersion.Id, new List<Guid>
                    {
                        imageFile1.File.Id,
                        imageFile2.File.Id
                    }, false), Times.Once);

                VerifyAllMocks(contentService, imageService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("Test approval", viewModel.LatestInternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
                Assert.Null(viewModel.ScheduledWithRelease);
                Assert.Equal(request.Status, viewModel.Status);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingUsingImmediateStrategy()
        {
            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved,
                Title = "Publication title",
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            var cacheService = new Mock<IBlobCacheService>(Strict);
            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);

            cacheService.Setup(mock =>
                    mock.DeleteItem(new AllMethodologiesCacheKey()))
                .Returns(Task.CompletedTask);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(true);

            publishingService.Setup(mock => mock.PublishMethodologyFiles(methodologyVersion.Id))
                .ReturnsAsync(Unit.Instance);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    blobCacheService: cacheService.Object,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object,
                    publishingService: publishingService.Object);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(cacheService, contentService, methodologyVersionRepository, publishingService);

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("Test approval", viewModel.LatestInternalReleaseNote);
                Assert.True(viewModel.Published.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(viewModel.Published!.Value).Milliseconds, 0, 1500);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
                Assert.Null(viewModel.ScheduledWithRelease);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.LatestInternalReleaseNote, viewModel.LatestInternalReleaseNote);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.True(updatedMethodology.Published.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Published!.Value).Milliseconds, 0, 1500);
                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.Equal(request.LatestInternalReleaseNote, updatedMethodology.InternalReleaseNote);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingUsingImmediateStrategy_ScheduledWithReleaseIsCleared()
        {
            var scheduledWithRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = WithRelease,
                Status = Approved,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                },
                // Existing ScheduledWithRelease should be cleared
                ScheduledWithRelease = scheduledWithRelease
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved,
                Title = "Publication title",
                // Requested id should be ignored
                WithReleaseId = scheduledWithRelease.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(contentService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
                Assert.Null(viewModel.ScheduledWithRelease);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                // Existing ScheduledWithReleaseId is cleared as requested publishing strategy is not WithRelease
                Assert.Null(updatedMethodology.ScheduledWithReleaseId);
            }
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingUsingWithReleaseStrategy_NonLiveRelease()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            var scheduledWithRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                TimePeriodCoverage = CalendarYear,
                ReleaseName = "2021"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = publication.Title,
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = publication
                    })
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = scheduledWithRelease.Id,
                Status = Approved,
                Title = publication.Title
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.Releases.AddAsync(scheduledWithRelease);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(contentService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, viewModel.Id);
                Assert.Equal("Test approval", viewModel.LatestInternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(WithRelease, viewModel.PublishingStrategy);
                Assert.Equal(request.Status, viewModel.Status);

                Assert.NotNull(viewModel.ScheduledWithRelease);
                Assert.Equal(scheduledWithRelease.Id, viewModel.ScheduledWithRelease.Id);
                Assert.Equal("Publication title - Calendar Year 2021", viewModel.ScheduledWithRelease.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(Approved, updatedMethodology.Status);
                Assert.Equal(WithRelease, updatedMethodology.PublishingStrategy);
                Assert.Equal(scheduledWithRelease.Id, updatedMethodology.ScheduledWithReleaseId);
                Assert.True(updatedMethodology.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(updatedMethodology.Updated!.Value).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingUsingWithReleaseStrategy_ReleaseIdMissing()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = publication.Title,
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = publication
                    })
                }
            };

            // Create a request with a release Id that is null
            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = null,
                Status = Approved,
                Title = publication.Title
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var result = await service.UpdateMethodology(methodologyVersion.Id, request);
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingUsingWithReleaseStrategy_ReleaseIdNotFound()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = publication.Title,
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            // Create a request with a random release Id that won't exist
            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = Guid.NewGuid(),
                Status = Approved,
                Title = publication.Title
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);


                var result = await service.UpdateMethodology(methodologyVersion.Id, request);
                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingUsingWithReleaseStrategy_ReleaseAlreadyPublished()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            // Create a release that is already published which the methodology cannot be made dependant on
            var scheduledWithRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                TimePeriodCoverage = CalendarYear,
                ReleaseName = "2021",
                Published = DateTime.UtcNow
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = publication.Title,
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = publication
                    })
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = scheduledWithRelease.Id,
                Status = Approved,
                Title = publication.Title
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.Releases.AddAsync(scheduledWithRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var result = await service.UpdateMethodology(methodologyVersion.Id, request);
                result.AssertBadRequest(MethodologyCannotDependOnPublishedRelease);
            }
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingUsingWithReleaseStrategy_ReleaseNotRelated()
        {
            // Release is not from the same publication as the one linked to the methodology
            var scheduledWithRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication(),
                TimePeriodCoverage = CalendarYear,
                ReleaseName = "2021"
            };

            var methodologyVersion = new MethodologyVersion
            {
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                WithReleaseId = scheduledWithRelease.Id,
                Status = Approved,
                Title = "Publication title"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.Releases.AddAsync(scheduledWithRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var result = await service.UpdateMethodology(methodologyVersion.Id, request);
                result.AssertBadRequest(MethodologyCannotDependOnRelease);
            }
        }

        [Fact]
        public async Task UpdateMethodology_UnapprovingMethodology()
        {
            var methodologyVersion = new MethodologyVersion
            {
                InternalReleaseNote = "Test approval",
                Published = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Methodology = new Methodology
                {
                    OwningPublicationTitle = "Publication title",
                    Publications = ListOf(new PublicationMethodology
                    {
                        Owner = true,
                        Publication = new Publication()
                    })
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "A release note to be ignored",
                PublishingStrategy = Immediately,
                Status = Draft,
                Title = "Publication title"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.MethodologyVersions.AddAsync(methodologyVersion);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyVersionRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodologyVersion.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                // Un-approving is allowed for users that can approve the methodology providing it's not publicly accessible
                // Test that un-approving alters the status
                var viewModel = (await service.UpdateMethodology(methodologyVersion.Id, request)).AssertRight();

                VerifyAllMocks(contentService, methodologyVersionRepository);

                Assert.Equal(methodologyVersion.Id, viewModel.Id);

                // Original release note is cleared if unapproving
                Assert.Null(viewModel.LatestInternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(Immediately, viewModel.PublishingStrategy);
                Assert.Null(viewModel.ScheduledWithRelease);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedMethodology = await context
                    .MethodologyVersions
                    .Include(m => m.Methodology)
                    .SingleAsync(m => m.Id == methodologyVersion.Id);

                Assert.Null(updatedMethodology.Published);
                Assert.Equal(Draft, updatedMethodology.Status);
                Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
                Assert.Null(updatedMethodology.InternalReleaseNote);
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
                PublishingStrategy = Immediately,
                Status = Draft,
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
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    Slug = "updated-methodology-title",
                    OwningPublicationTitle = "Test publication 2"
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
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
                Assert.Equal(Draft, notUpdatedMethodology.Status);
                Assert.Equal(Immediately, notUpdatedMethodology.PublishingStrategy);
                Assert.Equal("Test publication", notUpdatedMethodology.Title);
                Assert.Null(notUpdatedMethodology.AlternativeTitle);
                Assert.Equal("test-publication", notUpdatedMethodology.Slug);
                Assert.Equal("test-publication", notUpdatedMethodology.Methodology.Slug);
                Assert.False(notUpdatedMethodology.Updated.HasValue);
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
                PublishingStrategy = Immediately,
                Status = Draft,
                Methodology = new Methodology
                {
                    Id = methodologyId,
                    Slug = "pupil-absence-statistics-methodology",
                    OwningPublicationTitle = "Pupil absence statistics: methodology",
                    Publications = ListOf(new PublicationMethodology
                    {
                        MethodologyId = methodologyId
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
                Assert.NotNull(await context.Methodologies.SingleAsync(m => m.Id == methodologyId));
                Assert.NotNull(await context.MethodologyVersions.SingleAsync(m => m.Id == methodologyVersion.Id));
                Assert.NotNull(await context.PublicationMethodologies.SingleAsync(
                    m => m.MethodologyId == methodologyId));
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
                        PublishingStrategy = Immediately,
                        Status = Draft
                    },
                    new MethodologyVersion
                    {
                        Id = Guid.NewGuid(),
                        PublishingStrategy = Immediately,
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
                Assert.NotNull(await context.Methodologies.SingleAsync(m => m.Id == methodologyId));
                Assert.NotNull(await context.MethodologyVersions.SingleAsync(m => m.Id == methodology.Versions[0].Id));
                Assert.NotNull(await context.MethodologyVersions.SingleAsync(m => m.Id == methodology.Versions[1].Id));
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
                Assert.NotNull(await context.MethodologyVersions.SingleAsync(m => m.Id == methodology.Versions[0].Id));
                Assert.NotNull(await context.Methodologies.SingleAsync(m => m.Id == methodologyId));
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
                    PublishingStrategy = Immediately,
                    Status = Draft
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
                    PublishingStrategy = Immediately,
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
                    await context.MethodologyVersions.SingleAsync(m => m.Id == unrelatedMethodology.Versions[0].Id));
                Assert.NotNull(await context.Methodologies.SingleAsync(m => m.Id == unrelatedMethodologyId));
            }
        }

        private static MethodologyService SetupMethodologyService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IBlobCacheService? blobCacheService = null,
            IMethodologyContentService? methodologyContentService = null,
            IMethodologyFileRepository? methodologyFileRepository = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IMethodologyRepository? methodologyRepository = null,
            IMethodologyImageService? methodologyImageService = null,
            IPublishingService? publishingService = null,
            IUserService? userService = null)
        {
            return new(
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                contentDbContext,
                AdminMapper(),
                blobCacheService ?? Mock.Of<IBlobCacheService>(Strict),
                methodologyContentService ?? Mock.Of<IMethodologyContentService>(Strict),
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(Strict),
                methodologyRepository ?? Mock.Of<IMethodologyRepository>(Strict),
                methodologyImageService ?? Mock.Of<IMethodologyImageService>(Strict),
                publishingService ?? Mock.Of<IPublishingService>(Strict),
                userService ?? AlwaysTrueUserService(UserId).Object);
        }
    }
}

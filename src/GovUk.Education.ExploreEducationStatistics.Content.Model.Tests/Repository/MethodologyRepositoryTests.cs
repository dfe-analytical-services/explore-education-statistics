#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository
{
    public class MethodologyRepositoryTests
    {
        [Fact]
        public async Task GetByPublication()
        {
            var publication = new Publication();
            var methodology1 = new Methodology();
            var methodology2 = new Methodology();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
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
                    },
                    // Add a methodology linked to a different publication to check its not returned
                    new PublicationMethodology
                    {
                        Publication = new Publication(),
                        Methodology = methodology1,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                var result = await service.GetByPublication(publication.Id);

                Assert.Equal(2, result.Count);
                Assert.True(result.Exists(m => m.Id == methodology1.Id));
                Assert.True(result.Exists(m => m.Id == methodology2.Id));
            }
        }

        [Fact]
        public async Task GetByPublication_PublicationNotFoundIsEmpty()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Add a methodology linked to a different publication to check its not returned
                await contentDbContext.PublicationMethodologies.AddAsync(new PublicationMethodology
                {
                    Publication = new Publication(),
                    Methodology = new Methodology(),
                    Owner = true
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                var result = await service.GetByPublication(Guid.NewGuid());
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetByPublication_PublicationHasNoMethodologies()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);

                // Add a methodology linked to a different publication to check its not returned
                await contentDbContext.PublicationMethodologies.AddAsync(new PublicationMethodology
                {
                    Publication = new Publication(),
                    Methodology = new Methodology(),
                    Owner = true
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                var result = await service.GetByPublication(publication.Id);

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetOwningPublication()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Methodology = new Methodology
                {
                    Publications = new List<PublicationMethodology>
                    {
                        new()
                        {
                            Publication = new Publication
                            {
                                Title = "Adopting publication 1"
                            },
                            Owner = false
                        },
                        new()
                        {
                            Publication = new Publication
                            {
                                Title = "Owning publication"
                            },
                            Owner = true
                        },
                        new()
                        {
                            Publication = new Publication
                            {
                                Title = "Adopting publication 2"
                            },
                            Owner = false
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                var result = await service.GetOwningPublication(methodologyVersion.MethodologyId);

                Assert.Equal("Owning publication", result.Title);
            }
        }

        [Fact]
        public async Task GetUnrelatedToPublication()
        {
            var publication = new Publication();

            var methodologyOwnedByThisPublication = new Methodology
            {
                Publications = new List<PublicationMethodology>
                {
                    new()
                    {
                        Publication = publication,
                        Owner = true
                    },
                    new()
                    {
                        Publication = new Publication(),
                        Owner = false
                    }
                }
            };

            var methodologyAdoptedByThisPublication = new Methodology
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

            var methodologyUnrelatedToThisPublication = new Methodology
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Methodologies.AddRangeAsync(
                    methodologyOwnedByThisPublication,
                    methodologyAdoptedByThisPublication,
                    methodologyUnrelatedToThisPublication
                );

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                var result = await service.GetUnrelatedToPublication(publication.Id);

                Assert.Single(result);
                Assert.Equal(methodologyUnrelatedToThisPublication.Id, result[0].Id);
            }
        }

        private static MethodologyRepository BuildMethodologyRepository(ContentDbContext contentDbContext)
        {
            return new(contentDbContext);
        }
    }
}

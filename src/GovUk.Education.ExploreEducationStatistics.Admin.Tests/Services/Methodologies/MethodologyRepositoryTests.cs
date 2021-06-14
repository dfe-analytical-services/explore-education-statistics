using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            Guid methodologyId;
            
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);
                var methodology = await service.CreateMethodologyForPublication(publication.Id);
                await contentDbContext.SaveChangesAsync();
                methodologyId = methodology.Id;
            }
            
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodology = await contentDbContext
                    .Methodologies
                    .Include(m => m.MethodologyParent)
                    .ThenInclude(p => p.Publications)
                    .ThenInclude(p => p.Publication)
                    .SingleAsync(m => m.Id == methodologyId);
                
                var savedPublication = await contentDbContext.Publications.SingleAsync(p => p.Id == publication.Id);
                
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

            var methodology1 = new MethodologyParent
            {
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        PreviousVersionId = null,
                        Version = 0
                    },
                    new Methodology
                    {
                        Id = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                        PreviousVersionId = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        Version = 1
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
                        Version = 0
                    },
                    new Methodology
                    {
                        Id = Guid.Parse("2d6632b9-2480-4c34-b298-123064b6f04e"),
                        PreviousVersionId = Guid.Parse("4baee814-4c36-4850-9dc7-cfdae6026815"),
                        Version = 1
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.AttachRange(methodology1, methodology2);

                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                methodologyParentRepository.Setup(mock => mock.GetByPublication(publication.Id))
                    .ReturnsAsync(AsList(methodology1, methodology2));

                var result = await service.GetLatestByPublication(publication.Id);

                // Check the result contains the latest versions of the methodologies
                Assert.Equal(2, result.Count);
                Assert.True(result.Exists(mv => mv.Id == methodology1.Versions[1].Id));
                Assert.True(result.Exists(mv => mv.Id == methodology2.Versions[1].Id));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestByPublication_PublicationNotFoundThrowsException()
        {
            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = BuildMethodologyRepository(contentDbContext: contentDbContext,
                    methodologyParentRepository: methodologyParentRepository.Object);

                await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetLatestByPublication(Guid.NewGuid()));
            }

            MockUtils.VerifyAllMocks(methodologyParentRepository);
        }

        [Fact]
        public async Task GetLatestByPublication_PublicationHasNoMethodologies()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyParentRepository = new Mock<IMethodologyParentRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
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

        private static MethodologyRepository BuildMethodologyRepository(ContentDbContext contentDbContext,
            IMethodologyParentRepository methodologyParentRepository = null)
        {
            return new MethodologyRepository(
                contentDbContext,
                methodologyParentRepository ?? new Mock<IMethodologyParentRepository>().Object);
        }
    }
}

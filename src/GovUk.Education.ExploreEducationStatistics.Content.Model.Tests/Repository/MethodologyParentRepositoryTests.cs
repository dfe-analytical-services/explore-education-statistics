using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Repository
{
    public class MethodologyParentRepositoryTests
    {
        [Fact]
        public async Task GetByPublication()
        {
            var publication = new Publication();
            var methodology1 = new MethodologyParent();
            var methodology2 = new MethodologyParent();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
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
                    },
                    // Add a methodology linked to a different publication to check its not returned
                    new PublicationMethodology
                    {
                        Publication = new Publication(),
                        MethodologyParent = methodology1,
                        Owner = true
                    });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyParentRepository(contentDbContext);

                var result = await service.GetByPublication(publication.Id);

                Assert.Equal(2, result.Count);
                Assert.True(result.Exists(m => m.Id == methodology1.Id));
                Assert.True(result.Exists(m => m.Id == methodology2.Id));
            }
        }

        [Fact]
        public async Task GetByPublication_PublicationNotFoundThrowsException()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Add a methodology linked to a different publication to check its not returned
                await contentDbContext.PublicationMethodologies.AddAsync(new PublicationMethodology
                {
                    Publication = new Publication(),
                    MethodologyParent = new MethodologyParent(),
                    Owner = true
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyParentRepository(contentDbContext);

                await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetByPublication(Guid.NewGuid()));
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
                    MethodologyParent = new MethodologyParent(),
                    Owner = true
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildMethodologyParentRepository(contentDbContext);

                var result = await service.GetByPublication(publication.Id);

                Assert.Empty(result);
            }
        }

        private static MethodologyParentRepository BuildMethodologyParentRepository(ContentDbContext contentDbContext)
        {
            return new MethodologyParentRepository(contentDbContext);
        }
    }
}

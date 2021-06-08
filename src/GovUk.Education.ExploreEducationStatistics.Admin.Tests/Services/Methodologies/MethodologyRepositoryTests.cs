using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyRepositoryTests
    {
        [Fact]
        public async Task GetLatestMethodologiesByRelease()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildMethodologyRepository(contentDbContext);

                var result = await service.GetLatestMethodologiesByRelease(release.Id);

                // TODO SOW4 EES-2379 Test getting latest methodologies for a Release for approval checklist
                Assert.Empty(result);
            }
        }

        private static MethodologyRepository BuildMethodologyRepository(ContentDbContext contentDbContext)
        {
            return new MethodologyRepository(contentDbContext);
        }
    }
}

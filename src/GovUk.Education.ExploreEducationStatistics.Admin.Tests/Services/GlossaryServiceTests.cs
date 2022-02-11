#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class GlossaryServiceTests
    {
        [Fact]
        public async Task GetGlossaryEntry()
        {
            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddAsync(
                    new GlossaryEntry
                    {
                        Title = "Exclusion",
                        Slug = "exclusion",
                        Body = "Exclusion body",
                    }
                );
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var glossaryService = new GlossaryService(
                    new PersistenceHelper<ContentDbContext>(context));

                var result = await glossaryService.GetGlossaryEntry("exclusion");

                var viewModel = result.AssertRight();

                Assert.Equal("Exclusion", viewModel.Title);
                Assert.Equal("exclusion", viewModel.Slug);
                Assert.Equal("Exclusion body", viewModel.Body);
            }
        }

        [Fact]
        public async Task GetGlossaryEntry_NotFound()
        {
            await using var context = InMemoryContentDbContext();
            var glossaryService = new GlossaryService(
                new PersistenceHelper<ContentDbContext>(context));

            var result = await glossaryService.GetGlossaryEntry("absence");

            result.AssertNotFound();
        }
    }
}

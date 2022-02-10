#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class GlossaryServiceTests
    {
        [Fact]
        public async Task GetAllGlossaryEntries()
        {
            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new GlossaryEntry
                    {
                        Title = "Exclusion",
                        Slug = "exclusion",
                        Body = "Exclusion body",
                    },
                    new GlossaryEntry
                    {
                        Title = "Absence",
                        Slug = "absence",
                        Body = "Absence body",
                    },
                    new GlossaryEntry
                    {
                        Title = "expedient", // lowercase first letter
                        Slug = "expedient-slug",
                        Body = "Expedient body"
                    });
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var glossaryService = new GlossaryService(context);

                var result = await glossaryService.GetAllGlossaryEntries();

                Assert.Equal(26, result.Count);

                Assert.Equal("A", result[0].Heading);
                Assert.Single(result[0].Entries);
                Assert.Equal("Absence", result[0].Entries[0].Title);
                Assert.Equal("absence", result[0].Entries[0].Slug);
                Assert.Equal("Absence body", result[0].Entries[0].Body);

                Assert.Equal("B", result[1].Heading);
                Assert.Empty(result[1].Entries);

                Assert.Equal("E", result[4].Heading);
                Assert.Equal(2, result[4].Entries.Count);
                Assert.Equal("Exclusion", result[4].Entries[0].Title);
                Assert.Equal("exclusion", result[4].Entries[0].Slug);
                Assert.Equal("Exclusion body", result[4].Entries[0].Body);
                Assert.Equal("expedient", result[4].Entries[1].Title);
                Assert.Equal("expedient-slug", result[4].Entries[1].Slug);
                Assert.Equal("Expedient body", result[4].Entries[1].Body);

                Assert.Equal("Z", result[25].Heading);
                Assert.Empty(result[25].Entries);
            }
        }

        [Fact]
        public async Task GetAllGlossaryEntries_NoEntries()
        {
            await using var context = InMemoryContentDbContext();
            var glossaryService = new GlossaryService(context);

            var result = await glossaryService.GetAllGlossaryEntries();

            Assert.Equal(26, result.Count);

            result.ForEach(category => Assert.Empty(category.Entries));
        }

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
                var glossaryService = new GlossaryService(context);

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
            var glossaryService = new GlossaryService(context);

            var result = await glossaryService.GetGlossaryEntry("absence");

            result.AssertNotFound();
        }
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class GlossaryServiceTests
    {
        [Fact]
        public async Task GetGlossary()
        {
            var entries = new List<GlossaryEntry>
            {
                new()
                {
                    Title = "Exclusions",
                    Slug = "exclusions-slug",
                    Body = "Exclusions body"
                },
                new()
                {
                    Title = "Absence",
                    Slug = "absence-slug",
                    Body = "Absence body"
                }
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.GlossaryEntries.AddRangeAsync(entries);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var glossaryService = BuildService(contentDbContext: context);

                var result = await glossaryService.GetGlossary();

                Assert.Equal(26, result.Count);

                var categoryA = result[0];
                Assert.Single(categoryA.Entries);

                AssertGlossaryEntry(entries[1], categoryA.Entries[0]);

                var categoryE = result[4];
                Assert.Single(categoryE.Entries);

                AssertGlossaryEntry(entries[0], categoryE.Entries[0]);
            }
        }

        [Fact]
        public async Task GetGlossary_NoEntries()
        {
            var glossaryService = BuildService();

            var result = await glossaryService.GetGlossary();

            Assert.All(result, category => Assert.Empty(category.Entries));
        }

        [Fact]
        public async Task GetGlossaryEntry()
        {
            var entry = new GlossaryEntry
            {
                Title = "Exclusions",
                Slug = "exclusions-slug",
                Body = "Exclusions body"
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                await context.GlossaryEntries.AddRangeAsync(entry);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var glossaryService = BuildService(contentDbContext: context);

                var result = await glossaryService.GetGlossaryEntry("exclusions-slug");

                var viewModel = result.AssertRight();

                AssertGlossaryEntry(entry, viewModel);
            }
        }

        [Fact]
        public async Task GetGlossaryEntry_NotFound()
        {
            var glossaryService = BuildService();

            var result = await glossaryService.GetGlossaryEntry("absence-slug");

            result.AssertNotFound();
        }

        private static void AssertGlossaryEntry(GlossaryEntry glossaryEntry, GlossaryEntryViewModel viewModel)
        {
            Assert.Equal(glossaryEntry.Title, viewModel.Title);
            Assert.Equal(glossaryEntry.Slug, viewModel.Slug);
            Assert.Equal(glossaryEntry.Body, viewModel.Body);
        }

        private static GlossaryService BuildService(
            ContentDbContext? contentDbContext = null)
        {
            return new GlossaryService(
                contentDbContext ?? InMemoryContentDbContext()
            );
        }
    }
}

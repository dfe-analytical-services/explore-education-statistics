using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ThemeRepositoryTests
    {
        [Fact]
        public void GetThemes()
        {
            using (var context = DbUtils.InMemoryApplicationDbContext("Find"))
            {
                var themeToSave =
                    new Theme
                    {
                        Id = Guid.NewGuid(),
                        Summary = "Summary A",
                        Title = "Title A",
                        Topics = new List<Topic>
                        {
                            new Topic
                            {
                                Id = Guid.NewGuid(),
                                Description = "Description A",
                                Slug = "Slug A",
                                Summary = "Summary A",
                                Title = "Title A"
                            },
                            new Topic
                            {
                                Id = Guid.NewGuid(),
                                Description = "Description B",
                                Slug = "Slug B",
                                Summary = "Summary B",
                                Title = "Title B"
                            }
                        }
                    };

                context.Add(themeToSave);
                context.SaveChanges();

                var repository = new ThemeRepository(context);
                // Method under test
                var retrievedUserTheme = repository.GetAllThemesAsync().Result;
                Assert.True(retrievedUserTheme.Exists(t => t.Title == "Title A"));
                var themeA = retrievedUserTheme.Single(t => t.Title == "Title A");
                Assert.True(themeA.Topics.Exists(t => t.Title == "Title A"));
                Assert.True(themeA.Topics.Exists(t => t.Title == "Title B"));
            }
        }
    }
}
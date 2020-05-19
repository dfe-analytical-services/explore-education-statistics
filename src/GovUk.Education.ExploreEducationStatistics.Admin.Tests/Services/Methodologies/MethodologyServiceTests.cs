using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyServiceTests
    {
        [Fact]
        public void ListAsync()
        {
            using (var context = DbUtils.InMemoryApplicationDbContext("List"))
            {
                var methodologies = new List<Methodology>
                {
                    new Methodology
                    {
                        Id = new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"),
                        Title = "Methodology 1",
                        Published = DateTime.UtcNow,
                        Status = MethodologyStatus.Approved
                    },
                    new Methodology
                    {
                        Id = new Guid("ebeb2b2d-fc6b-4734-9420-4e4dd37816ba"),
                        Title = "Methodology 2",
                        Status = MethodologyStatus.Draft
                    },
                };

                context.AddRange(methodologies);
                context.SaveChanges();
            }

            using (var context = DbUtils.InMemoryApplicationDbContext("List"))
            {
                // Method under test
                var methodologies = new MethodologyService(
                        context,
                        AdminMapper(),
                        MockUtils.AlwaysTrueUserService().Object,
                        new PersistenceHelper<ContentDbContext>(context))
                    .ListAsync().Result.Right;

                Assert.Contains(methodologies,
                    m => m.Id == new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46") && m.Title == "Methodology 1" &&
                         m.Status == MethodologyStatus.Approved);
                
                Assert.Contains(methodologies,
                    m => m.Id == new Guid("ebeb2b2d-fc6b-4734-9420-4e4dd37816ba") && m.Title == "Methodology 2" &&
                         m.Status == MethodologyStatus.Draft);
            }
        }

        [Fact]
        public async Task GetStatusAsync()
        {
            var methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Status = MethodologyStatus.Approved,
                Title = "Pupil absence statistics: methodology"
            };

            await using (var context = DbUtils.InMemoryApplicationDbContext("GetStatus"))
            {
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext("GetStatus"))
            {
                var viewModel = (await new MethodologyService(
                        context,
                        AdminMapper(),
                        MockUtils.AlwaysTrueUserService().Object,
                        new PersistenceHelper<ContentDbContext>(context))
                    .GetStatusAsync(methodology.Id)).Right;
                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Equal(methodology.Status, viewModel.Status);
                Assert.Equal(methodology.Title, viewModel.Title);
            }
        }
        
        [Fact]
        public async Task GetSummaryAsync()
        {
            var methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Published = new DateTime(2020, 5, 25),
                PublishScheduled = new DateTime(2020, 6, 1),
                Status = MethodologyStatus.Approved,
                Title = "Pupil absence statistics: methodology"
            };

            await using (var context = DbUtils.InMemoryApplicationDbContext("GetSummary"))
            {
                context.Add(methodology);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext("GetSummary"))
            {
                var viewModel = (await new MethodologyService(
                        context,
                        AdminMapper(),
                        MockUtils.AlwaysTrueUserService().Object,
                        new PersistenceHelper<ContentDbContext>(context))
                    .GetSummaryAsync(methodology.Id)).Right;
                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Equal(methodology.Published, viewModel.Published);
                Assert.Equal(methodology.PublishScheduled, viewModel.PublishScheduled);
                Assert.Equal(methodology.Status, viewModel.Status);
                Assert.Equal(methodology.Title, viewModel.Title);
            }
        }
    }
}
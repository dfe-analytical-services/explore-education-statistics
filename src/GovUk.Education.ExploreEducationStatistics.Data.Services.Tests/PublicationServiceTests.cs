using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class PublicationServiceTests
    {
        [Fact]
        public async Task GetPublication()
        {
            var publicationId = Guid.NewGuid();

            var release1 = new Release
            {
                PublicationId = publicationId,
                Published = DateTime.Parse("2018-06-01T09:00:00Z"),
                TimeIdentifier = AcademicYearQ1,
                Year = 2018
            };

            var release2 = new Release
            {
                PublicationId = publicationId,
                Published = DateTime.Parse("2018-03-01T09:00:00Z"),
                TimeIdentifier = AcademicYearQ4,
                Year = 2017
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddRangeAsync(release1, release2);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var subject1 = new SubjectViewModel(
                    id: Guid.NewGuid(),
                    name: "Subject 1",
                    content: "Subject 1 content",
                    timePeriods: new TimePeriodLabels("2018", "2021"),
                    geographicLevels: new List<string>
                    {
                        "National",
                        "Local Authority"
                    }
                );
                var subject2 = new SubjectViewModel(
                    id: Guid.NewGuid(),
                    name: "Subject 2",
                    content: "Subject 2 content",
                    timePeriods: new TimePeriodLabels("2015", "2020"),
                    geographicLevels: new List<string>
                    {
                        "Local Authority District"
                    }
                );

                var highlight1 = new TableHighlightViewModel(
                    id: Guid.NewGuid(),
                    name: "Highlight 1",
                    description: "Highlight 1 description"
                );

                var highlight2 = new TableHighlightViewModel(
                    id: Guid.NewGuid(),
                    name: "Highlight 2",
                    description: "Highlight 2 description"
                );

                var releaseService = new Mock<IReleaseService>();

                releaseService
                    .Setup(s => s.GetRelease(release1.Id))
                    .ReturnsAsync(new ReleaseViewModel
                    {
                        Subjects = new List<SubjectViewModel>
                        {
                            subject1,
                            subject2
                        },
                        Highlights = new List<TableHighlightViewModel>
                        {
                            highlight1,
                            highlight2,
                        }
                    });

                var service = BuildPublicationService(context, releaseService: releaseService.Object);

                var result = await service.GetLatestPublicationSubjectsAndHighlights(publicationId);
                var viewModel = result.Right;

                var highlights = viewModel.Highlights.ToList();
                var subjects = viewModel.Subjects.ToList();

                Assert.Equal(2, highlights.Count);
                Assert.Equal(highlight1, highlights[0]);
                Assert.Equal(highlight2, highlights[1]);

                Assert.Equal(2, subjects.Count);
                Assert.Equal(subject1, subjects[0]);
                Assert.Equal(subject2, subjects[1]);

                MockUtils.VerifyAllMocks(releaseService);
            }
        }

        [Fact]
        public async Task GetPublication_PublicationNotFound()
        {
            await using var context = StatisticsDbUtils.InMemoryStatisticsDbContext();
            var service = BuildPublicationService(context);

            var result = await service.GetLatestPublicationSubjectsAndHighlights(Guid.NewGuid());
            result.AssertNotFound();
        }

        private static PublicationService BuildPublicationService(
            StatisticsDbContext context,
            IReleaseRepository releaseRepository = null,
            IReleaseService releaseService = null)
        {
            return new PublicationService(
                releaseRepository ?? new ReleaseRepository(context),
                releaseService ?? new Mock<IReleaseService>().Object
            );
        }
    }
}

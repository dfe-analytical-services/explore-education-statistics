using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseRepositoryTests
    {
        [Fact]
        public async Task ReleaseHierarchyCreated()
        {
            var release = new Release
            {
                ReleaseName = "2000",
                Published = DateTime.UtcNow,
                TimePeriodCoverage = TimeIdentifier.April,
                PreviousVersionId = Guid.NewGuid(),
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Title = "Test theme"
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseRepository = new ReleaseRepository(contentDbContext, statisticsDbContext,
                    Common.Services.MapperUtils.MapperForProfile<MappingProfiles>());

                await releaseRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(
                    release.Id);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var statsTheme = await statisticsDbContext.Theme.FindAsync(release.Publication.Topic.Theme.Id);
                Assert.NotNull(statsTheme);
                Assert.Equal(release.Publication.Topic.Theme.Title, statsTheme.Title);

                var statsTopic = await statisticsDbContext.Topic.FindAsync(release.Publication.Topic.Id);
                Assert.NotNull(statsTopic);
                Assert.Equal(release.Publication.Topic.Title, statsTopic.Title);

                var statsPub = await statisticsDbContext.Publication.FindAsync(release.Publication.Id);
                Assert.NotNull(statsPub);
                Assert.Equal(release.Publication.Title, statsPub.Title);

                var statsRelease = await statisticsDbContext.Release.FindAsync(release.Id);
                Assert.NotNull(statsRelease);
                Assert.Equal(release.Published, statsRelease.Published);
                Assert.Equal(release.Slug, statsRelease.Slug);
                Assert.Equal(release.PublicationId, statsRelease.PublicationId);
                Assert.Equal(release.TimePeriodCoverage, statsRelease.TimeIdentifier);
                Assert.Equal(release.Year, statsRelease.Year);
                Assert.Equal(release.PreviousVersionId, statsRelease.PreviousVersionId);
            }
            
        }
        
        [Fact]
        public async Task ReleaseHierarchyUpdated()
        {
            var release = new Release
            {
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.Week5,
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Title = "Test theme"
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(new Data.Model.Theme
                {
                    Id = release.Publication.Topic.Theme.Id,
                    Title = "Incorrect theme title",
                    Slug = release.Publication.Topic.Theme.Slug
                });
                await statisticsDbContext.AddAsync(new Data.Model.Topic
                {
                    Id = release.Publication.Topic.Id,
                    Title = "Incorrect topic title"
                });
                await statisticsDbContext.AddAsync(new Data.Model.Publication
                {
                    Id = release.Publication.Id,
                    Title = "Incorrect publication title"
                });
                await statisticsDbContext.AddAsync(new Data.Model.Release
                {
                    Id = release.Id,
                    Year = 1234,
                    TimeIdentifier = TimeIdentifier.CalendarYearQ1
                });
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseRepository = new ReleaseRepository(contentDbContext, statisticsDbContext,
                    Common.Services.MapperUtils.MapperForProfile<MappingProfiles>());

                await releaseRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(
                    release.Id);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var statsTheme = await statisticsDbContext.Theme.FindAsync(release.Publication.Topic.Theme.Id);
                Assert.NotNull(statsTheme);
                Assert.Equal(release.Publication.Topic.Theme.Title, statsTheme.Title);

                var statsTopic = await statisticsDbContext.Topic.FindAsync(release.Publication.Topic.Id);
                Assert.NotNull(statsTopic);
                Assert.Equal(release.Publication.Topic.Title, statsTopic.Title);

                var statsPub = await statisticsDbContext.Publication.FindAsync(release.Publication.Id);
                Assert.NotNull(statsPub);
                Assert.Equal(release.Publication.Title, statsPub.Title);

                var statsRelease = await statisticsDbContext.Release.FindAsync(release.Id);
                Assert.NotNull(statsRelease);
                Assert.Equal(release.Year, statsRelease.Year);
                Assert.Equal(release.TimePeriodCoverage, statsRelease.TimeIdentifier);
            }
        }
        
        [Fact]
        public async Task SubjectHierarchyCreated()
        {
            var release = new Release
            {
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.April,
                Publication = new Publication
                {
                    Title = "Test publication",
                    Topic = new Topic
                    {
                        Title = "Test topic",
                        Theme = new Theme
                        {
                            Title = "Test theme"
                        }
                    }
                }
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var releaseRepository = new ReleaseRepository(contentDbContext, statisticsDbContext,
                    Common.Services.MapperUtils.MapperForProfile<MappingProfiles>());

                await releaseRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(
                    release.Id);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var subjects = statisticsDbContext.Subject.ToList();
                Assert.Single(subjects);

                var releaseSubjects = statisticsDbContext.ReleaseSubject.ToList();
                Assert.Single(releaseSubjects);
                Assert.Equal(subjects[0].Id, releaseSubjects[0].SubjectId);
                Assert.Equal(release.Id, releaseSubjects[0].ReleaseId);
            }
        }
    }
}

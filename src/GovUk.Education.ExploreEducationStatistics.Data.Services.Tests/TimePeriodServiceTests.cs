#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class TimePeriodServiceTests
    {
        [Fact]
        public async Task GetTimePeriods_Subject_CorrectlyOrderWeeklyTimeIdentifiers()
        {
            var subject = new Subject
            {
                Observations = new List<Observation>
                {
                    new() {Year = 2001, TimeIdentifier = Week1},
                    new() {Year = 2000, TimeIdentifier = Week20},
                    new() {Year = 2000, TimeIdentifier = Week2},
                    new() {Year = 2000, TimeIdentifier = Week1},
                    new() {Year = 2000, TimeIdentifier = Week10},
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = new TimePeriodService(statisticsDbContext);
                var result = await service.GetTimePeriods(subject.Id);

                Assert.Equal(5, result.Count);
                Assert.Equal((2000, Week1), result[0]);
                Assert.Equal((2000, Week2), result[1]);
                Assert.Equal((2000, Week10), result[2]);
                Assert.Equal((2000, Week20), result[3]);
                Assert.Equal((2001, Week1), result[4]);
            }
        }

        [Fact]
        public async Task GetTimePeriods_Subject_CorrectlyOrderMonthlyTimeIdentifiers()
        {
            var subject = new Subject
            {
                Observations = new List<Observation>
                {
                    new() {Year = 2001, TimeIdentifier = January},
                    new() {Year = 2000, TimeIdentifier = February},
                    new() {Year = 2000, TimeIdentifier = December},
                    new() {Year = 2000, TimeIdentifier = October},
                    new() {Year = 2000, TimeIdentifier = March},
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = new TimePeriodService(statisticsDbContext);
                var result = await service.GetTimePeriods(subject.Id);

                Assert.Equal(5, result.Count);
                Assert.Equal((2000, February), result[0]);
                Assert.Equal((2000, March), result[1]);
                Assert.Equal((2000, October), result[2]);
                Assert.Equal((2000, December), result[3]);
                Assert.Equal((2001, January), result[4]);
            }
        }

        [Fact]
        public async Task GetTimePeriods_Observations_CorrectlyOrderWeeklyTimeIdentifiers()
        {
            var obs1 = new Observation {Year = 2001, TimeIdentifier = Week1};
            var obs2 = new Observation {Year = 2000, TimeIdentifier = Week20};
            var obs3 = new Observation {Year = 2000, TimeIdentifier = Week2};
            var obs4 = new Observation {Year = 2000, TimeIdentifier = Week1};
            var obs5 = new Observation {Year = 2000, TimeIdentifier = Week10};

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(obs1, obs2, obs3, obs4, obs5);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = new TimePeriodService(statisticsDbContext);
                var queryableObservations = statisticsDbContext.Observation.AsQueryable();
                var result = await service.GetTimePeriods(queryableObservations);

                Assert.Equal(5, result.Count);
                Assert.Equal((2000, Week1), result[0]);
                Assert.Equal((2000, Week2), result[1]);
                Assert.Equal((2000, Week10), result[2]);
                Assert.Equal((2000, Week20), result[3]);
                Assert.Equal((2001, Week1), result[4]);
            }
        }

        [Fact]
        public async Task GetTimePeriodLabels()
        {
            var release = new Release();

            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
                DataGuidance = "Subject 1 Guidance"
            };

            var subjectObservation1 = new Observation
            {
                Subject = subject,
                Year = 2030,
                TimeIdentifier = AcademicYearQ3
            };

            var subjectObservation2 = new Observation
            {
                Subject = subject,
                Year = 2020,
                TimeIdentifier = AcademicYearQ4
            };

            var subjectObservation3 = new Observation
            {
                Subject = subject,
                Year = 2021,
                TimeIdentifier = AcademicYearQ1
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.AddRangeAsync(
                    subjectObservation1,
                    subjectObservation2,
                    subjectObservation3);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = new TimePeriodService(statisticsDbContext);

                var result = await service.GetTimePeriodLabels(subject.Id);

                Assert.Equal("2020/21 Q4", result.From);
                Assert.Equal("2030/31 Q3", result.To);
            }
        }

        [Fact]
        public async Task GetTimePeriodLabels_CorrectlyOrderWeeks()
        {
            var release = new Release();

            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
                DataGuidance = "Subject 1 Guidance"
            };

            var subjectObservation1 = new Observation
            {
                Subject = subject,
                Year = 2020,
                TimeIdentifier = Week9,
            };

            var subjectObservation2 = new Observation
            {
                Subject = subject,
                Year = 2020,
                TimeIdentifier = Week37,
            };

            var subjectObservation3 = new Observation
            {
                Subject = subject,
                Year = 2020,
                TimeIdentifier = Week8,
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.AddRangeAsync(
                    subjectObservation1,
                    subjectObservation2,
                    subjectObservation3);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = new TimePeriodService(statisticsDbContext);

                var result = await service.GetTimePeriodLabels(subject.Id);

                Assert.Equal("2020 Week 8", result.From);
                Assert.Equal("2020 Week 37", result.To);
            }
        }

        [Fact]
        public async Task GetTimePeriodLabels_NoObservations()
        {
            var release = new Release();

            var subject = new Subject();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
                DataGuidance = "Subject 1 Guidance"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.AddAsync(releaseSubject1);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = new TimePeriodService(statisticsDbContext);

                var result = await service.GetTimePeriodLabels(subject.Id);

                Assert.Empty(result.From);
                Assert.Empty(result.To);
            }
        }
    }
}

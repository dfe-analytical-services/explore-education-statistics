using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class MetaGuidanceSubjectServiceTests
    {
        [Fact]
        public async Task Get()
        {
            var release = new Release();

            var subject1 = new Subject
            {
                Filename = "file1.csv",
                Name = "Subject 1"
            };

            var subject2 = new Subject
            {
                Filename = "file2.csv",
                Name = "Subject 2"
            };

            var subject1Filter = new Filter
            {
                Subject = subject1,
                Name = "subject1_filter",
                Label = "Subject 1 Filter",
                Hint = "Hint"
            };

            var subject2Filter = new Filter
            {
                Subject = subject2,
                Name = "subject2_filter",
                Label = "Subject 2 Filter",
                Hint = null
            };

            var subject1IndicatorGroup = new IndicatorGroup
            {
                Subject = subject1,
                Indicators = new List<Indicator>
                {
                    new Indicator
                    {
                        Name = "subject1_indicator",
                        Label = "Subject 1 Indicator"
                    }
                }
            };

            var subject2IndicatorGroup = new IndicatorGroup
            {
                Subject = subject2,
                Indicators = new List<Indicator>
                {
                    new Indicator
                    {
                        Name = "subject2_indicator",
                        Label = "Subject 2 Indicator"
                    }
                }
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = subject1,
                MetaGuidance = "Subject 1 Meta Guidance"
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = subject2,
                MetaGuidance = "Subject 2 Meta Guidance"
            };

            var subject1Observation1 = new Observation
            {
                GeographicLevel = GeographicLevel.Country,
                Subject = subject1,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ3
            };

            var subject1Observation2 = new Observation
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Subject = subject1,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ4
            };

            var subject1Observation3 = new Observation
            {
                GeographicLevel = GeographicLevel.LocalAuthorityDistrict,
                Subject = subject1,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.AcademicYearQ1
            };

            var subject2Observation1 = new Observation
            {
                GeographicLevel = GeographicLevel.Country,
                Subject = subject2,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.SummerTerm
            };

            var subject2Observation2 = new Observation
            {
                GeographicLevel = GeographicLevel.Region,
                Subject = subject2,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.AutumnTerm
            };

            var subject2Observation3 = new Observation
            {
                GeographicLevel = GeographicLevel.Region,
                Subject = subject2,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.SpringTerm
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.AddRangeAsync(subject1Filter, subject2Filter);
                await statisticsDbContext.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
                await statisticsDbContext.AddRangeAsync(subject1Observation1, subject1Observation2,
                    subject1Observation3, subject2Observation1, subject2Observation2, subject2Observation3);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var result = await service.GetSubjects(release.Id);

                // Assert there are two Subjects with the correct content
                Assert.True(result.IsRight);

                Assert.Equal(2, result.Right.Count);

                Assert.Equal(subject1.Id, result.Right[0].Id);
                Assert.Equal("Subject 1 Meta Guidance", result.Right[0].Content);
                Assert.Equal("file1.csv", result.Right[0].Filename);
                Assert.Equal("Subject 1", result.Right[0].Name);
                Assert.Equal("2020/21 Q3", result.Right[0].TimePeriods.From);
                Assert.Equal("2021/22 Q1", result.Right[0].TimePeriods.To);
                Assert.Equal(new List<string>
                {
                    "National", "Local Authority", "Local Authority District"
                }, result.Right[0].GeographicLevels);
                Assert.Equal(2, result.Right[0].Variables.Count);
                Assert.Equal("Subject 1 Filter - Hint", result.Right[0].Variables[0].Label);
                Assert.Equal("subject1_filter", result.Right[0].Variables[0].Value);
                Assert.Equal("Subject 1 Indicator", result.Right[0].Variables[1].Label);
                Assert.Equal("subject1_indicator", result.Right[0].Variables[1].Value);

                Assert.Equal(subject2.Id, result.Right[1].Id);
                Assert.Equal("Subject 2 Meta Guidance", result.Right[1].Content);
                Assert.Equal("file2.csv", result.Right[1].Filename);
                Assert.Equal("Subject 2", result.Right[1].Name);
                Assert.Equal("2020/21 Summer Term", result.Right[1].TimePeriods.From);
                Assert.Equal("2021/22 Spring Term", result.Right[1].TimePeriods.To);
                Assert.Equal(new List<string>
                {
                    "National", "Regional"
                }, result.Right[1].GeographicLevels);
                Assert.Equal(2, result.Right[1].Variables.Count);
                Assert.Equal("Subject 2 Filter", result.Right[1].Variables[0].Label);
                Assert.Equal("subject2_filter", result.Right[1].Variables[0].Value);
                Assert.Equal("Subject 2 Indicator", result.Right[1].Variables[1].Label);
                Assert.Equal("subject2_indicator", result.Right[1].Variables[1].Value);
            }
        }

        [Fact]
        public async Task Get_OrderedByName()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject
                {
                    Filename = "file1.csv",
                    Name = "Subject 1"
                },
                MetaGuidance = "Subject 1 Meta Guidance"
            };
            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject
                {
                    Filename = "file2.csv",
                    Name = "Subject 2"
                },
                MetaGuidance = "Subject 2 Meta Guidance"
            };
            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject
                {
                    Filename = "file3.csv",
                    Name = "Subject 3"
                },
                MetaGuidance = "Subject 3 Meta Guidance"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                // Saved in random order
                await statisticsDbContext.AddRangeAsync(releaseSubject3, releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var result = await service.GetSubjects(release.Id);

                Assert.True(result.IsRight);

                Assert.Equal(3, result.Right.Count);

                Assert.Equal("Subject 1", result.Right[0].Name);
                Assert.Equal("Subject 2", result.Right[1].Name);
                Assert.Equal("Subject 3", result.Right[2].Name);
            }
        }

        [Fact]
        public async Task Get_SpecificSubjects()
        {
            var release = new Release();

            var subject1 = new Subject
            {
                Filename = "file1.csv",
                Name = "Subject 1"
            };

            var subject2 = new Subject
            {
                Filename = "file2.csv",
                Name = "Subject 2"
            };

            var subject3 = new Subject
            {
                Filename = "file3.csv",
                Name = "Subject 3"
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = subject1,
                MetaGuidance = "Subject 1 Meta Guidance"
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = subject2,
                MetaGuidance = "Subject 2 Meta Guidance"
            };

            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = subject3,
                MetaGuidance = "Subject 3 Meta Guidance"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(subject1, subject2, subject3);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2, releaseSubject3);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var result = await service.GetSubjects(release.Id, new List<Guid>
                {
                    subject1.Id, subject3.Id
                });

                // Assert only the specified Subjects are returned
                Assert.True(result.IsRight);

                Assert.Equal(2, result.Right.Count);

                Assert.Equal(subject1.Id, result.Right[0].Id);
                Assert.Equal("Subject 1 Meta Guidance", result.Right[0].Content);
                Assert.Equal("file1.csv", result.Right[0].Filename);
                Assert.Equal("Subject 1", result.Right[0].Name);
                Assert.Null(result.Right[0].TimePeriods.From);
                Assert.Null(result.Right[0].TimePeriods.To);
                Assert.Empty(result.Right[0].GeographicLevels);
                Assert.Empty(result.Right[0].Variables);

                Assert.Equal(subject3.Id, result.Right[1].Id);
                Assert.Equal("Subject 3 Meta Guidance", result.Right[1].Content);
                Assert.Equal("file3.csv", result.Right[1].Filename);
                Assert.Equal("Subject 3", result.Right[1].Name);
                Assert.Null(result.Right[1].TimePeriods.From);
                Assert.Null(result.Right[1].TimePeriods.To);
                Assert.Empty(result.Right[1].GeographicLevels);
                Assert.Empty(result.Right[1].Variables);
            }
        }

        [Fact]
        public async Task Get_NoRelease()
        {
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var result = await service.GetSubjects(Guid.NewGuid());

                Assert.True(result.IsRight);
                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task Get_NoSubjects()
        {
            var release = new Release();

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var result = await service.GetSubjects(release.Id);

                Assert.True(result.IsRight);
                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task Get_AmendedRelease()
        {
            var releaseVersion1 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var releaseVersion2 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = releaseVersion1.Id
            };

            var subject1 = new Subject
            {
                Filename = "file1.csv",
                Name = "Subject 1"
            };

            var subject2 = new Subject
            {
                Filename = "file2.csv",
                Name = "Subject 2"
            };

            // Version 1 has one Subject, Version 2 adds another Subject

            var releaseVersion1Subject1 = new ReleaseSubject
            {
                Release = releaseVersion1,
                Subject = subject1,
                MetaGuidance = "Version 1 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject1 = new ReleaseSubject
            {
                Release = releaseVersion2,
                Subject = subject1,
                MetaGuidance = "Version 2 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject2 = new ReleaseSubject
            {
                Release = releaseVersion2,
                Subject = subject2,
                MetaGuidance = "Version 2 Subject 2 Meta Guidance"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseVersion1, releaseVersion2);
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseVersion1Subject1, releaseVersion2Subject1,
                    releaseVersion2Subject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var version1Result = await service.GetSubjects(releaseVersion1.Id);

                // Assert version 1 has one Subject with the correct content
                Assert.True(version1Result.IsRight);

                Assert.Single(version1Result.Right);

                Assert.Equal(subject1.Id, version1Result.Right[0].Id);
                Assert.Equal("Version 1 Subject 1 Meta Guidance", version1Result.Right[0].Content);
                Assert.Equal("file1.csv", version1Result.Right[0].Filename);
                Assert.Equal("Subject 1", version1Result.Right[0].Name);
                Assert.Null(version1Result.Right[0].TimePeriods.From);
                Assert.Null(version1Result.Right[0].TimePeriods.To);
                Assert.Empty(version1Result.Right[0].GeographicLevels);
                Assert.Empty(version1Result.Right[0].Variables);

                var version2Result = await service.GetSubjects(releaseVersion2.Id);

                // Assert version 2 has two Subjects with the correct content
                Assert.True(version2Result.IsRight);

                Assert.Equal(2, version2Result.Right.Count);

                Assert.Equal(subject1.Id, version2Result.Right[0].Id);
                Assert.Equal("Version 2 Subject 1 Meta Guidance", version2Result.Right[0].Content);
                Assert.Equal("file1.csv", version2Result.Right[0].Filename);
                Assert.Equal("Subject 1", version2Result.Right[0].Name);
                Assert.Null(version2Result.Right[0].TimePeriods.From);
                Assert.Null(version2Result.Right[0].TimePeriods.To);
                Assert.Empty(version2Result.Right[0].GeographicLevels);

                Assert.Equal(subject2.Id, version2Result.Right[1].Id);
                Assert.Equal("Version 2 Subject 2 Meta Guidance", version2Result.Right[1].Content);
                Assert.Equal("file2.csv", version2Result.Right[1].Filename);
                Assert.Equal("Subject 2", version2Result.Right[1].Name);
                Assert.Null(version2Result.Right[1].TimePeriods.From);
                Assert.Null(version2Result.Right[1].TimePeriods.To);
                Assert.Empty(version2Result.Right[1].GeographicLevels);
                Assert.Empty(version2Result.Right[1].Variables);
            }
        }

        [Fact]
        public async Task Validate_NoRelease()
        {
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var result = await service.Validate(Guid.NewGuid());

                Assert.True(result.IsRight);
                Assert.True(result.Right);
            }
        }

        [Fact]
        public async Task Validate_NoSubjects()
        {
            var release = new Release();

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
                Assert.True(result.Right);
            }
        }

        [Fact]
        public async Task Validate_MetaGuidancePopulated()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                MetaGuidance = "Subject 1 Meta Guidance",
                Release = release,
                Subject = new Subject
                {
                    Filename = "file1.csv",
                    Name = "Subject 1"
                }
            };

            var releaseSubject2 = new ReleaseSubject
            {
                MetaGuidance = "Subject 2 Meta Guidance",
                Release = release,
                Subject = new Subject
                {
                    Filename = "file2.csv",
                    Name = "Subject 2",
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
                Assert.True(result.Right);
            }
        }

        [Fact]
        public async Task Validate_MetaGuidanceNotPopulated()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                MetaGuidance = "Subject 1 Meta Guidance",
                Release = release,
                Subject = new Subject
                {
                    Filename = "file1.csv",
                    Name = "Subject 1"
                }
            };

            // Meta guidance is not populated for Subject 2
            var releaseSubject2 = new ReleaseSubject
            {
                MetaGuidance = null,
                Release = release,
                Subject = new Subject
                {
                    Filename = "file2.csv",
                    Name = "Subject 2",
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(context: statisticsDbContext);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
                Assert.False(result.Right);
            }
        }

        private static MetaGuidanceSubjectService SetupMetaGuidanceSubjectService(
            StatisticsDbContext context,
            IFilterService filterService = null,
            IIndicatorService indicatorService = null,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper = null)
        {
            return new MetaGuidanceSubjectService(
                filterService ?? new FilterService(context, new Mock<ILogger<FilterService>>().Object),
                indicatorService ?? new IndicatorService(context, new Mock<ILogger<IndicatorService>>().Object),
                context,
                persistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(context)
            );
        }
    }
}
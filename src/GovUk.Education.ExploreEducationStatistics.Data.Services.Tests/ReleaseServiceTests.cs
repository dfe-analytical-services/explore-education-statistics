#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class ReleaseServiceTests
    {
        [Fact]
        public async Task ListSubjects()
        {
            var statisticsRelease = new Data.Model.Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
                DataGuidance = "Guidance 1"

            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
                DataGuidance = "Guidance 2"
            };

            var subject1Filter1 = new Filter
            {
                Subject = releaseSubject1.Subject,
                Label = "subject 1 filter 1",
            };

            var subject1Filter2 = new Filter
            {
                Subject = releaseSubject1.Subject,
                Label = "subject 1 filter 2",
            };

            var subject1IndicatorGroup1 = new IndicatorGroup
            {
                Subject = releaseSubject1.Subject,
                Label = "subject 1 indicator group 1",
                Indicators = new List<Indicator>
                {
                    new Indicator { Label = "subject 1 indicator group 1 indicator 1" },
                    new Indicator { Label = "subject 1 indicator group 1 indicator 2" },
                    new Indicator { Label = "subject 1 indicator group 1 indicator 3" },
                }
            };

            var subject1IndicatorGroup2 = new IndicatorGroup
            {
                Subject = releaseSubject1.Subject,
                Label = "subject 1 indicator group 2",
                Indicators = new List<Indicator>
                {
                    new Indicator { Label = "subject 1 indicator group 2 indicator 1" },
                    new Indicator { Label = "subject 1 indicator group 2 indicator 2" },
                }
            };

            var subject2Filter1 = new Filter
            {
                Subject = releaseSubject2.Subject,
                Label = "subject 2 filter 1",
            };

            var subject2IndicatorGroup1 = new IndicatorGroup
            {
                Subject = releaseSubject2.Subject,
                Label = "subject 2 indicator group 1",
                Indicators = new List<Indicator>
                {
                    new Indicator { Label = "subject 2 indicator group 1 indicator 1" },
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.Filter.AddRangeAsync(
                    subject1Filter1, subject1Filter2, subject2Filter1);
                await statisticsDbContext.IndicatorGroup.AddRangeAsync(
                    subject1IndicatorGroup1, subject1IndicatorGroup2, subject2IndicatorGroup1);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    Filename = "data1.csv",
                    ContentLength = 10240,
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                },
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 2",
                File = new File
                {
                    Filename = "data2.csv",
                    ContentLength = 20480,
                    Type = FileType.Data,
                    SubjectId = releaseSubject2.Subject.Id,
                }
            };

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.COMPLETE
            };

            var import2 = new DataImport
            {
                File = releaseFile2.File,
                Status = DataImportStatus.COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.AddRangeAsync(import1, import2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();
                var timePeriodService = new Mock<ITimePeriodService>();

                timePeriodService
                    .Setup(s => s.GetTimePeriodLabels(releaseSubject1.SubjectId))
                    .ReturnsAsync(new TimePeriodLabels("2020/21", "2021/22"));

                timePeriodService
                    .Setup(s => s.GetTimePeriodLabels(releaseSubject2.SubjectId))
                    .ReturnsAsync(new TimePeriodLabels("2030", "2031"));

                dataGuidanceSubjectService
                    .Setup(s => s.GetGeographicLevels(releaseSubject1.SubjectId))
                    .ReturnsAsync(ListOf("Local Authority", "Local Authority District"));

                dataGuidanceSubjectService
                    .Setup(s => s.GetGeographicLevels(releaseSubject2.SubjectId))
                    .ReturnsAsync(ListOf("National"));

                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object,
                    timePeriodService: timePeriodService.Object
                );

                var result = await service.ListSubjects(contentRelease.Id);

                MockUtils.VerifyAllMocks(dataGuidanceSubjectService, timePeriodService);

                var subjects = result.AssertRight();

                Assert.NotNull(subjects);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(releaseSubject1.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseFile1.Name, subjects[0].Name);
                Assert.Equal(releaseFile1.File.Id, subjects[0].File.Id);
                Assert.Equal(releaseFile1.File.Filename, subjects[0].File.FileName);
                Assert.Equal("10 Kb", subjects[0].File.Size);
                Assert.Equal("csv", subjects[0].File.Extension);

                Assert.Equal(releaseSubject1.DataGuidance, subjects[0].Content);

                Assert.Equal("2020/21", subjects[0].TimePeriods.From);
                Assert.Equal("2021/22", subjects[0].TimePeriods.To);

                Assert.Equal(2, subjects[0].GeographicLevels.Count);
                Assert.Equal("Local Authority", subjects[0].GeographicLevels[0]);
                Assert.Equal("Local Authority District", subjects[0].GeographicLevels[1]);

                Assert.Equal(2, subjects[0].Filters.Count);
                Assert.Equal("subject 1 filter 1", subjects[0].Filters[0]);
                Assert.Equal("subject 1 filter 2", subjects[0].Filters[1]);

                Assert.Equal(5, subjects[0].Indicators.Count);
                Assert.Equal("subject 1 indicator group 1 indicator 1", subjects[0].Indicators[0]);
                Assert.Equal("subject 1 indicator group 1 indicator 2", subjects[0].Indicators[1]);
                Assert.Equal("subject 1 indicator group 1 indicator 3", subjects[0].Indicators[2]);
                Assert.Equal("subject 1 indicator group 2 indicator 1", subjects[0].Indicators[3]);
                Assert.Equal("subject 1 indicator group 2 indicator 2", subjects[0].Indicators[4]);

                Assert.Equal(releaseSubject2.Subject.Id, subjects[1].Id);
                Assert.Equal(releaseFile2.Name, subjects[1].Name);
                Assert.Equal(releaseFile2.File.Id, subjects[1].File.Id);
                Assert.Equal(releaseFile2.File.Filename, subjects[1].File.FileName);
                Assert.Equal("20 Kb", subjects[1].File.Size);
                Assert.Equal(releaseSubject2.DataGuidance, subjects[1].Content);
                Assert.Equal("csv", subjects[1].File.Extension);

                Assert.Equal("2030", subjects[1].TimePeriods.From);
                Assert.Equal("2031", subjects[1].TimePeriods.To);

                Assert.Single(subjects[1].GeographicLevels);
                Assert.Equal("National", subjects[1].GeographicLevels[0]);

                Assert.Single(subjects[1].Filters);
                Assert.Equal("subject 2 filter 1", subjects[1].Filters[0]);

                Assert.Single(subjects[1].Indicators);
                Assert.Equal("subject 2 indicator group 1 indicator 1", subjects[1].Indicators[0]);
            }
        }

        [Fact]
        public async Task ListSubjects_FiltersPendingReplacementSubjects()
        {
            var statisticsRelease = new Data.Model.Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
            };

            var releaseSubject2Replacement = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
            };
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2, releaseSubject2Replacement);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };

            var file2 = new File
            {
                Filename = "data2.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject2.Subject.Id,
            };

            var file2Replacement = new File
            {
                Filename = "data2_replacement.csv",
                Type = FileType.Data,
                SubjectId =  releaseSubject2Replacement.Subject.Id,
                Replacing = file2
            };

            file2.ReplacedBy = file2Replacement;

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 2",
                File = file2
            };

            var releaseFile2Replacement = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 2 Replacement",
                File = file2Replacement
            };

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.COMPLETE
            };

            var import2 = new DataImport
            {
                File = releaseFile2.File,
                Status = DataImportStatus.COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2, releaseFile2Replacement);
                await contentDbContext.AddRangeAsync(import1, import2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.ListSubjects(contentRelease.Id);

                var subjects = result.AssertRight();

                Assert.NotNull(subjects);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(releaseSubject1.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseFile1.Name, subjects[0].Name);
                Assert.Equal(releaseSubject2.Subject.Id, subjects[1].Id);
                Assert.Equal(releaseFile2.Name, subjects[1].Name);
            }
        }

        [Fact]
        public async Task ListSubjects_FiltersImportingSubjects()
        {
            var statisticsRelease = new Data.Model.Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid(),
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Name = "Data 1",
                Release = contentRelease,
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };

            var releaseFile2 = new ReleaseFile
            {
                Name = "Data 2",
                Release = contentRelease,
                File = new File
                {
                    Filename = "data2.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject2.Subject.Id,
                }
            };

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.STAGE_1
            };

            var import2 = new DataImport
            {
                File = releaseFile2.File,
                Status = DataImportStatus.COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.AddRangeAsync(import1, import2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.ListSubjects(contentRelease.Id);

                var subjects = result.AssertRight();

                Assert.Single(subjects);
                Assert.Equal(releaseSubject2.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseFile2.Name, subjects[0].Name);
            }
        }

        [Fact]
        public async Task ListSubjects_FiltersSubjectsWithNoImport()
        {
            var statisticsRelease = new Data.Model.Release();
            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject.Subject.Id
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.ListSubjects(contentRelease.Id);

                var subjects = result.AssertRight();

                Assert.Empty(subjects);
            }
        }

        [Fact]
        public async Task ListSubjects_FiltersSubjectsWithNoFileSubjectId()
        {
            var releaseId = Guid.NewGuid();

            var release = new Release
            {
                Id = releaseId
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                }
            };

            var import = new DataImport
            {
                File = releaseFile.File,
                Status = DataImportStatus.COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(releaseFile);
                await contentDbContext.AddRangeAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext);

                var result = await service.ListSubjects(release.Id);

                var subjects = result.AssertRight();

                Assert.Empty(subjects);
            }
        }

        [Fact]
        public async Task ListSubjects_FilterOrder()
        {
            var statisticsRelease = new Data.Model.Release();

            var subject1Filter1Id = Guid.NewGuid();
            var subject1Filter2Id = Guid.NewGuid();
            var subject1Filter3Id = Guid.NewGuid();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
                DataGuidance = "Guidance 1",
                FilterSequence = new List<FilterSequenceEntry>
                {
                    new (subject1Filter2Id, new List<FilterGroupSequenceEntry>()),
                    new (subject1Filter1Id, new List<FilterGroupSequenceEntry>()),
                    new (subject1Filter3Id, new List<FilterGroupSequenceEntry>()),
                },
            };

            var subject1Filter1 = new Filter
            {
                Id = subject1Filter1Id,
                Subject = releaseSubject1.Subject,
                Label = "subject 1 filter 1",
            };

            var subject1Filter2 = new Filter
            {
                Id = subject1Filter2Id,
                Subject = releaseSubject1.Subject,
                Label = "subject 1 filter 2",
            };

            var subject1Filter3 = new Filter
            {
                Id = subject1Filter3Id,
                Subject = releaseSubject1.Subject,
                Label = "subject 1 filter 3",
            };

            var subject1IndicatorGroup1 = new IndicatorGroup
            {
                Subject = releaseSubject1.Subject,
                Label = "subject 1 indicator group 1",
                Indicators = new List<Indicator>
                {
                    new() { Label = "subject 1 indicator group 1 indicator 1" },
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1);
                await statisticsDbContext.Filter.AddRangeAsync(
                    subject1Filter1, subject1Filter2, subject1Filter3);
                await statisticsDbContext.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup1);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    Filename = "data1.csv",
                    ContentLength = 10240,
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                },
            };

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1);
                await contentDbContext.AddRangeAsync(import1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.ListSubjects(contentRelease.Id);
                var subjects = result.AssertRight();
                var subject = Assert.Single(subjects);

                Assert.Equal(3, subject.Filters.Count);
                Assert.Equal("subject 1 filter 2", subject.Filters[0]);
                Assert.Equal("subject 1 filter 1", subject.Filters[1]);
                Assert.Equal("subject 1 filter 3", subject.Filters[2]);
            }
        }

        [Fact]
        public async Task ListSubjects_IndicatorOrder()
        {
            var statisticsRelease = new Data.Model.Release();

            var subject1Indicator1 = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "subject 1 indicator 1",
            };
            var subject1Indicator2 = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "subject 1 indicator 2",
            };
            var subject1Indicator3 = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "subject 1 indicator 3",
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
                DataGuidance = "Guidance 1",
                IndicatorSequence = new List<IndicatorGroupSequenceEntry>
                {
                    new(Guid.NewGuid(), new List<Guid> { subject1Indicator2.Id, subject1Indicator1.Id, }),
                    new(Guid.NewGuid(), new List<Guid> { subject1Indicator3.Id, }),
                },
            };

            var subject1Filter1 = new Filter
            {
                Subject = releaseSubject1.Subject,
                Label = "subject 1 filter 1",
            };

            var subject1IndicatorGroup1 = new IndicatorGroup
            {
                Subject = releaseSubject1.Subject,
                Label = "subject 1 indicator group 1",
                Indicators = new List<Indicator>
                {
                    subject1Indicator1, subject1Indicator2,
                },
            };

            var subject1IndicatorGroup2 = new IndicatorGroup
            {
                Subject = releaseSubject1.Subject,
                Label = "subject 1 indicator group 2",
                Indicators = new List<Indicator> { subject1Indicator3 },
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1);
                await statisticsDbContext.Filter.AddRangeAsync(
                    subject1Filter1);
                await statisticsDbContext.IndicatorGroup.AddRangeAsync(
                    subject1IndicatorGroup1, subject1IndicatorGroup2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    Filename = "data1.csv",
                    ContentLength = 10240,
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                },
            };

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1);
                await contentDbContext.AddRangeAsync(import1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.ListSubjects(contentRelease.Id);
                var subjects = result.AssertRight();
                var subject = Assert.Single(subjects);

                Assert.Equal(3, subject.Indicators.Count);
                Assert.Equal("subject 1 indicator 2", subject.Indicators[0]);
                Assert.Equal("subject 1 indicator 1", subject.Indicators[1]);
                Assert.Equal("subject 1 indicator 3", subject.Indicators[2]);
            }
        }

        [Fact]
        public async Task ListFeaturedTables()
        {
            var releaseId = Guid.NewGuid();
            var release = new Release { Id = releaseId };
            var statRelease = new Data.Model.Release { Id = releaseId };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };
            var releaseSubject2 = new ReleaseSubject
            {
                Release = statRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };

            var releaseFile1 = new ReleaseFile
            {
                Name = "Data 1",
                Release = release,
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Name = "Data 2",
                Release = release,
                File = new File
                {
                    Filename = "data2.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject2.Subject.Id
                }
            };

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.COMPLETE
            };
            var import2 = new DataImport
            {
                File = releaseFile2.File,
                Status = DataImportStatus.COMPLETE
            };

            var dataBlock1 = new DataBlock
            {
                Name = "Test data block 1",
                Query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject1.Subject.Id,
                }
            };
            var featuredTable1 = new FeaturedTable
            {
                DataBlock = dataBlock1,
                Name = "Test featured table name 1",
                Description = "Test featured table description 1",
            };

            var dataBlock2 = new DataBlock
            {
                Name = "Test data block 2",
                Query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject2.Subject.Id,
                }
            };
            var featuredTable2 = new FeaturedTable
            {
                DataBlock = dataBlock2,
                Name = "Test featured table name 2",
                Description = "Test featured table description 2",
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.FeaturedTables.AddRangeAsync(featuredTable1, featuredTable2);
                await contentDbContext.DataImports.AddRangeAsync(import1, import2);
                // Order is reversed
                await contentDbContext.ReleaseContentBlocks.AddRangeAsync(
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock2
                    },
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock1
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.ListFeaturedTables(release.Id);

                var featuredTables = result.AssertRight();

                Assert.Equal(2, featuredTables.Count);

                Assert.Equal(featuredTable1.Id, featuredTables[0].Id);
                Assert.Equal(featuredTable1.Name, featuredTables[0].Name);
                Assert.Equal(featuredTable1.Description, featuredTables[0].Description);
                Assert.Equal(releaseSubject1.SubjectId, featuredTables[0].SubjectId);
                Assert.Equal(dataBlock1.Id, featuredTables[0].DataBlockId);

                Assert.Equal(featuredTable2.Id, featuredTables[1].Id);
                Assert.Equal(featuredTable2.Name, featuredTables[1].Name);
                Assert.Equal(featuredTable2.Description, featuredTables[1].Description);
                Assert.Equal(releaseSubject2.SubjectId, featuredTables[1].SubjectId);
                Assert.Equal(dataBlock2.Id, featuredTables[1].DataBlockId);
            }
        }

        [Fact]
        public async Task ListFeaturedTables_FiltersImportingSubjects()
        {
            var releaseId = Guid.NewGuid();
            var release = new Release
            {
                Id = releaseId
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = new Data.Model.Release
                {
                    Id = releaseId
                },
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };

            var releaseFile1 = new ReleaseFile
            {
                Name = "Data 1",
                Release = release,
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.STAGE_1
            };

            var dataBlock1 = new DataBlock
            {
                Name = "Test data block",
                Query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject1.Subject.Id,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile1);
                await contentDbContext.AddAsync(import1);
                await contentDbContext.AddRangeAsync(
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock1
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject1);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.ListFeaturedTables(release.Id);

                var featuredTables = result.AssertRight();

                Assert.Empty(featuredTables);
            }
        }

        [Fact]
        public async Task ListFeaturedTables_FiltersNonFeaturedTables()
        {
            var releaseId = Guid.NewGuid();
            var release = new Release
            {
                Id = releaseId
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = new Data.Model.Release
                {
                    Id = releaseId
                },
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };

            var releaseFile1 = new ReleaseFile
            {
                Name = "Data 1",
                Release = release,
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.NOT_FOUND
            };

            var dataBlock1 = new DataBlock
            {
                Name = "Test data block",
                Query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject1.Subject.Id,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile1);
                await contentDbContext.AddAsync(import1);
                await contentDbContext.AddRangeAsync(
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock1
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject1);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.ListFeaturedTables(release.Id);

                var featuredTables = result.AssertRight();

                Assert.Empty(featuredTables);
            }
        }

        [Fact]
        public async Task ListFeaturedTables_FiltersNonMatchingSubjects()
        {
            var releaseId = Guid.NewGuid();
            var release = new Release
            {
                Id = releaseId
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = new Data.Model.Release
                {
                    Id = releaseId
                },
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };

            var releaseFile1 = new ReleaseFile
            {
                Name = "Data 1",
                Release = release,
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.COMPLETE
            };

            // Subject does not match
            var dataBlock1 = new DataBlock
            {
                Name = "Test data block",
                Query = new ObservationQueryContext
                {
                    SubjectId = Guid.NewGuid(),
                }
            };
            var featuredTable1 = new FeaturedTable
            {
                DataBlock = dataBlock1,
                Name = "Test featured table name",
                Description = "Test featured table description",
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddAsync(releaseFile1);
                await contentDbContext.FeaturedTables.AddAsync(featuredTable1);
                await contentDbContext.DataImports.AddAsync(import1);
                await contentDbContext.ReleaseContentBlocks.AddRangeAsync(
                    new ReleaseContentBlock
                    {
                        Release = release,
                        ContentBlock = dataBlock1
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject1);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.ListFeaturedTables(release.Id);

                var featuredTables = result.AssertRight();

                Assert.Empty(featuredTables);
            }
        }

        private static ReleaseService BuildReleaseService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
            StatisticsDbContext? statisticsDbContext = null,
            IUserService? userService = null,
            IDataGuidanceSubjectService? dataGuidanceSubjectService = null,
            ITimePeriodService? timePeriodService = null)
        {
            return new ReleaseService(
                contentDbContext,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                dataGuidanceSubjectService ?? Mock.Of<IDataGuidanceSubjectService>(),
                timePeriodService ?? Mock.Of<ITimePeriodService>()
            );
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class DataGuidanceDataSetServiceTests
{
    [Fact]
    public async Task ListDataSets()
    {
        var releaseVersion = new ReleaseVersion();

        var subject1 = new Subject();

        var subject2 = new Subject();

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
                new()
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
                new()
                {
                    Name = "subject2_indicator",
                    Label = "Subject 2 Indicator"
                }
            }
        };

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = subject1
        };

        var releaseSubject2 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = subject2
        };

        var subject1Observation1 = new Observation
        {
            Location = new Location
            {
                GeographicLevel = GeographicLevel.Country
            },
            Subject = subject1,
            Year = 2020,
            TimeIdentifier = TimeIdentifier.AcademicYearQ3
        };

        var subject1Observation2 = new Observation
        {
            Location = new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority
            },
            Subject = subject1,
            Year = 2020,
            TimeIdentifier = TimeIdentifier.AcademicYearQ4
        };

        var subject1Observation3 = new Observation
        {
            Location = new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthorityDistrict
            },
            Subject = subject1,
            Year = 2021,
            TimeIdentifier = TimeIdentifier.AcademicYearQ1
        };

        var subject2Observation1 = new Observation
        {
            Location = new Location
            {
                GeographicLevel = GeographicLevel.Country
            },
            Subject = subject2,
            Year = 2020,
            TimeIdentifier = TimeIdentifier.SummerTerm
        };

        var subject2Observation2 = new Observation
        {
            Location = new Location
            {
                GeographicLevel = GeographicLevel.Region
            },
            Subject = subject2,
            Year = 2021,
            TimeIdentifier = TimeIdentifier.AutumnTerm
        };

        var subject2Observation3 = new Observation
        {
            Location = new Location
            {
                GeographicLevel = GeographicLevel.Region
            },
            Subject = subject2,
            Year = 2021,
            TimeIdentifier = TimeIdentifier.SpringTerm
        };

        var subject1Footnote1 = new SubjectFootnote
        {
            Subject = subject1,
            Footnote = new Footnote
            {
                Content = "Subject 1 Footnote 1",
                Order = 0
            }
        };
        var subject1Footnote2 = new FilterFootnote
        {
            Filter = subject1Filter,
            Footnote = new Footnote
            {
                Content = "Subject 1 Footnote 2",
                Order = 1
            }
        };
        var subject1Footnote3 = new FilterGroupFootnote
        {
            FilterGroup = new FilterGroup
            {
                Filter = subject1Filter
            },
            Footnote = new Footnote
            {
                Content = "Subject 1 Footnote 3",
                Order = 2
            }
        };

        var subject2Footnote1 = new FilterItemFootnote
        {
            FilterItem = new FilterItem
            {
                FilterGroup = new FilterGroup
                {
                    Filter = subject2Filter
                }
            },
            Footnote = new Footnote
            {
                Content = "Subject 2 Footnote 1",
                Order = 0
            }
        };
        var subject2Footnote2 = new IndicatorFootnote
        {
            Indicator = subject2IndicatorGroup.Indicators[0],
            Footnote = new Footnote
            {
                Content = "Subject 2 Footnote 2",
                Order = 1
            }
        };

        var releaseFootnote1 = new ReleaseFootnote
        {
            Footnote = subject1Footnote1.Footnote,
            ReleaseVersion = releaseVersion
        };
        var releaseFootnote2 = new ReleaseFootnote
        {
            Footnote = subject1Footnote2.Footnote,
            ReleaseVersion = releaseVersion
        };
        var releaseFootnote3 = new ReleaseFootnote
        {
            Footnote = subject1Footnote3.Footnote,
            ReleaseVersion = releaseVersion
        };
        var releaseFootnote4 = new ReleaseFootnote
        {
            Footnote = subject2Footnote1.Footnote,
            ReleaseVersion = releaseVersion
        };
        var releaseFootnote5 = new ReleaseFootnote
        {
            Footnote = subject2Footnote2.Footnote,
            ReleaseVersion = releaseVersion
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(releaseVersion);
            statisticsDbContext.Subject.AddRange(subject1, subject2);
            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject1, releaseSubject2);
            statisticsDbContext.Filter.AddRange(subject1Filter, subject2Filter);
            statisticsDbContext.IndicatorGroup.AddRange(subject1IndicatorGroup, subject2IndicatorGroup);
            statisticsDbContext.Observation.AddRange(
                subject1Observation1,
                subject1Observation2,
                subject1Observation3,
                subject2Observation1,
                subject2Observation2,
                subject2Observation3
            );
            statisticsDbContext.SubjectFootnote.AddRange(subject1Footnote1);
            statisticsDbContext.FilterFootnote.AddRange(subject1Footnote2);
            statisticsDbContext.FilterGroupFootnote.AddRange(subject1Footnote3);
            statisticsDbContext.FilterItemFootnote.AddRange(subject2Footnote1);
            statisticsDbContext.IndicatorFootnote.AddRange(subject2Footnote2);
            statisticsDbContext.ReleaseFootnote.AddRange(
                releaseFootnote1,
                releaseFootnote2,
                releaseFootnote3,
                releaseFootnote4,
                releaseFootnote5
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = releaseVersion.Id
        };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 1",
            File = new File
            {
                SubjectId = subject1.Id,
                Filename = "file1.csv",
                Type = FileType.Data,
            },
            Summary = "Data set 1 guidance"
        };

        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Subject 2",
            File = new File
            {
                SubjectId = subject2.Id,
                Filename = "file2.csv",
                Type = FileType.Data,
            },
            Summary = "Data set 2 guidance"
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(contentReleaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFile1, releaseFile2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext);

            var result = (await service.ListDataSets(releaseVersion.Id)).AssertRight();

            // Assert there are two data sets with the correct content

            Assert.Equal(2, result.Count);

            Assert.Equal(releaseFile1.FileId, result[0].FileId);
            Assert.Equal("Data set 1 guidance", result[0].Content);
            Assert.Equal("file1.csv", result[0].Filename);
            Assert.Equal("Subject 1", result[0].Name);

            Assert.Equal("2020/21 Q3", result[0].TimePeriods.From);
            Assert.Equal("2021/22 Q1", result[0].TimePeriods.To);
            Assert.Equal(new List<string>
            {
                "National",
                "Local authority",
                "Local authority district"
            }, result[0].GeographicLevels);

            Assert.Equal(2, result[0].Variables.Count);
            Assert.Equal("Subject 1 Filter - Hint", result[0].Variables[0].Label);
            Assert.Equal("subject1_filter", result[0].Variables[0].Value);
            Assert.Equal("Subject 1 Indicator", result[0].Variables[1].Label);
            Assert.Equal("subject1_indicator", result[0].Variables[1].Value);

            Assert.Equal(3, result[0].Footnotes.Count);
            Assert.Equal("Subject 1 Footnote 1", result[0].Footnotes[0].Label);
            Assert.Equal(subject1Footnote1.FootnoteId, result[0].Footnotes[0].Id);
            Assert.Equal("Subject 1 Footnote 2", result[0].Footnotes[1].Label);
            Assert.Equal(subject1Footnote2.FootnoteId, result[0].Footnotes[1].Id);
            Assert.Equal("Subject 1 Footnote 3", result[0].Footnotes[2].Label);
            Assert.Equal(subject1Footnote3.FootnoteId, result[0].Footnotes[2].Id);

            Assert.Equal(releaseFile2.FileId, result[1].FileId);
            Assert.Equal("Data set 2 guidance", result[1].Content);
            Assert.Equal("file2.csv", result[1].Filename);
            Assert.Equal("Subject 2", result[1].Name);

            Assert.Equal("2020/21 Summer term", result[1].TimePeriods.From);
            Assert.Equal("2021/22 Spring term", result[1].TimePeriods.To);
            Assert.Equal(new List<string>
            {
                "National",
                "Regional"
            }, result[1].GeographicLevels);

            Assert.Equal(2, result[1].Variables.Count);
            Assert.Equal("Subject 2 Filter", result[1].Variables[0].Label);
            Assert.Equal("subject2_filter", result[1].Variables[0].Value);
            Assert.Equal("Subject 2 Indicator", result[1].Variables[1].Label);
            Assert.Equal("subject2_indicator", result[1].Variables[1].Value);

            Assert.Equal(2, result[1].Footnotes.Count);
            Assert.Equal("Subject 2 Footnote 1", result[1].Footnotes[0].Label);
            Assert.Equal(subject2Footnote1.FootnoteId, result[1].Footnotes[0].Id);
            Assert.Equal("Subject 2 Footnote 2", result[1].Footnotes[1].Label);
            Assert.Equal(subject2Footnote2.FootnoteId, result[1].Footnotes[1].Id);
        }
    }

    [Fact]
    public async Task ListDataSets_OrderedByName()
    {
        var releaseVersion = new ReleaseVersion();

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = new Subject()
        };
        var releaseSubject2 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = new Subject()
        };
        var releaseSubject3 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = new Subject()
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(releaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject1,
                releaseSubject2,
                releaseSubject3);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = releaseVersion.Id,
        };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            File = new File
            {
                SubjectId = releaseSubject1.SubjectId,
                Filename = "file1.csv",
                Type = FileType.Data,
            },
            Name = "Data set 1",
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            File = new File
            {
                SubjectId = releaseSubject2.SubjectId,
                Filename = "file2.csv",
                Type = FileType.Data,
            },
            Name = "Data set 2",
        };
        var releaseFile3 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            File = new File
            {
                SubjectId = releaseSubject3.SubjectId,
                Filename = "file3.csv",
                Type = FileType.Data,
            },
            Name = "Data set 3",
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(contentReleaseVersion);
            contentDbContext.ReleaseFiles.AddRange(
                // Saved in random order
                releaseFile3,
                releaseFile1,
                releaseFile2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext);

            var result = await service.ListDataSets(releaseVersion.Id);

            var viewModels = result.AssertRight();

            Assert.Equal(3, viewModels.Count);

            Assert.Equal("Data set 1", viewModels[0].Name);
            Assert.Equal("Data set 2", viewModels[1].Name);
            Assert.Equal("Data set 3", viewModels[2].Name);
        }
    }

    [Fact]
    public async Task ListDataSets_FilterDataSetsByFileId()
    {
        var releaseVersion = new ReleaseVersion();

        var releaseSubject1 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = new Subject()
        };

        var releaseSubject2 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = new Subject()
        };

        var releaseSubject3 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = new Subject()
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(releaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject1,
                releaseSubject2,
                releaseSubject3);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = releaseVersion.Id,
        };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Data set 1",
            File = new File
            {
                SubjectId = releaseSubject1.SubjectId,
                Filename = "file1.csv",
                Type = FileType.Data,
            }
        };

        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Data set 2",
            File = new File
            {
                SubjectId = releaseSubject2.SubjectId,
                Filename = "file2.csv",
                Type = FileType.Data,
            }
        };

        var releaseFile3 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            Name = "Data set 3",
            File = new File
            {
                SubjectId = releaseSubject3.SubjectId,
                Filename = "file3.csv",
                Type = FileType.Data,
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(
                releaseFile1, releaseFile2, releaseFile3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext);

            var result = await service.ListDataSets(releaseVersion.Id, new List<Guid>
            {
                releaseFile1.FileId, releaseFile3.FileId
            });

            // Assert only the specified data sets are returned
            var viewModels = result.AssertRight();

            Assert.Equal(2, viewModels.Count);

            Assert.Equal(releaseFile1.FileId, viewModels[0].FileId);
            Assert.Equal(releaseFile3.FileId, viewModels[1].FileId);
        }
    }

    [Fact]
    public async Task ListDataSets_ReplacementInProgressIsIgnored()
    {
        var releaseVersion = new ReleaseVersion();

        var originalSubject = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = new Subject()
        };

        var replacementSubject = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = new Subject()
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(releaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalSubject,
                replacementSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        var contentReleaseVersion = new Content.Model.ReleaseVersion
        {
            Id = releaseVersion.Id
        };

        var originalFile = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            File = new File
            {
                Filename = "file1.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.SubjectId
            }
        };

        var replacementFile = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion,
            File = new File
            {
                Filename = "file1.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.SubjectId,
                Replacing = originalFile.File
            }
        };

        originalFile.File.ReplacedBy = replacementFile.File;

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(contentReleaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalFile, replacementFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext);

            var result = await service.ListDataSets(contentReleaseVersion.Id);

            var viewModels = result.AssertRight();

            // Replacement data sets should not be included when the replacement is still in progress
            Assert.Single(viewModels);
            Assert.Equal(originalFile.FileId, viewModels[0].FileId);
        }
    }

    [Fact]
    public async Task ListDataSets_NoRelease()
    {
        var service = SetupService();

        var result = await service.ListDataSets(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task ListDataSets_NoDataSets()
    {
        var releaseVersion = new Content.Model.ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext);

            var result = await service.ListDataSets(releaseVersion.Id);

            var viewModels = result.AssertRight();
            Assert.Empty(viewModels);
        }
    }

    [Fact]
    public async Task ListDataSets_AmendedRelease()
    {
        var releaseVersion1 = new ReleaseVersion
        {
            Id = Guid.NewGuid()
        };

        var releaseVersion2 = new ReleaseVersion
        {
            Id = Guid.NewGuid()
        };

        var subject1 = new Subject();
        var subject2 = new Subject();

        // Version 1 has one data set, Version 2 adds another data set

        var releaseVersion1Subject1 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion1,
            Subject = subject1
        };

        var releaseVersion2Subject1 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion2,
            Subject = subject1
        };

        var releaseVersion2Subject2 = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion2,
            Subject = subject2
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(releaseVersion1, releaseVersion2);
            statisticsDbContext.Subject.AddRange(subject1, subject2);
            statisticsDbContext.ReleaseSubject.AddRange(releaseVersion1Subject1, releaseVersion2Subject1,
                releaseVersion2Subject2);
            await statisticsDbContext.SaveChangesAsync();
        }

        var file1 = new File
        {
            SubjectId = subject1.Id,
            Filename = "file1.csv",
            Type = FileType.Data,
        };

        var file2 = new File
        {
            SubjectId = subject2.Id,
            Filename = "file2.csv",
            Type = FileType.Data,
        };

        var contentReleaseVersion1 = new Content.Model.ReleaseVersion
        {
            Id = releaseVersion1.Id,
            PreviousVersionId = null,
        };

        var contentReleaseVersion2 = new Content.Model.ReleaseVersion
        {
            Id = releaseVersion2.Id,
            PreviousVersionId = releaseVersion1.Id,
        };

        var releaseVersion1File1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion1,
            File = file1,
            Name = "Version 1 data set 1",
            Summary = "Version 1 data set 1 guidance"
        };

        var releaseVersion2File1 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion2,
            File = file1,
            Name = "Version 2 data set 1",
            Summary = "Version 2 data set 1 guidance"
        };

        var releaseVersion2File2 = new ReleaseFile
        {
            ReleaseVersion = contentReleaseVersion2,
            File = file2,
            Name = "Version 2 data set 2",
            Summary = "Version 2 data set 2 guidance"
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(
                contentReleaseVersion1,
                contentReleaseVersion2);
            contentDbContext.ReleaseFiles.AddRange(
                releaseVersion1File1,
                releaseVersion2File1,
                releaseVersion2File2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext);

            var version1Result = await service.ListDataSets(releaseVersion1.Id);
            var version1ViewModels = version1Result.AssertRight();

            // Assert version 1 has one data set with the correct content
            Assert.Single(version1ViewModels);

            Assert.Equal(releaseVersion1File1.FileId, version1ViewModels[0].FileId);
            Assert.Equal("Version 1 data set 1 guidance", version1ViewModels[0].Content);
            Assert.Equal("file1.csv", version1ViewModels[0].Filename);
            Assert.Equal("Version 1 data set 1", version1ViewModels[0].Name);
            Assert.Empty(version1ViewModels[0].TimePeriods.From);
            Assert.Empty(version1ViewModels[0].TimePeriods.To);
            Assert.Empty(version1ViewModels[0].GeographicLevels);
            Assert.Empty(version1ViewModels[0].Variables);

            var version2Result = await service.ListDataSets(releaseVersion2.Id);
            var version2ViewModels = version2Result.AssertRight();

            // Assert version 2 has two data sets with the correct content
            Assert.Equal(2, version2ViewModels.Count);

            Assert.Equal(releaseVersion2File1.FileId, version2ViewModels[0].FileId);
            Assert.Equal("Version 2 data set 1 guidance", version2ViewModels[0].Content);
            Assert.Equal("file1.csv", version2ViewModels[0].Filename);
            Assert.Equal("Version 2 data set 1", version2ViewModels[0].Name);
            Assert.Empty(version2ViewModels[0].TimePeriods.From);
            Assert.Empty(version2ViewModels[0].TimePeriods.To);
            Assert.Empty(version2ViewModels[0].GeographicLevels);
            Assert.Empty(version2ViewModels[0].Variables);

            Assert.Equal(releaseVersion2File2.FileId, version2ViewModels[1].FileId);
            Assert.Equal("Version 2 data set 2 guidance", version2ViewModels[1].Content);
            Assert.Equal("file2.csv", version2ViewModels[1].Filename);
            Assert.Equal("Version 2 data set 2", version2ViewModels[1].Name);
            Assert.Empty(version2ViewModels[1].TimePeriods.From);
            Assert.Empty(version2ViewModels[1].TimePeriods.To);
            Assert.Empty(version2ViewModels[1].GeographicLevels);
            Assert.Empty(version2ViewModels[1].Variables);
        }
    }

    [Fact]
    public async Task ListGeographicLevels()
    {
        var releaseVersion = new ReleaseVersion();

        var subject = new Subject();

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = subject
        };

        var subjectObservation1 = new Observation
        {
            Location = new Location
            {
                GeographicLevel = GeographicLevel.Country
            },
            Subject = subject,
            Year = 2020,
            TimeIdentifier = TimeIdentifier.AcademicYearQ3
        };

        var subjectObservation2 = new Observation
        {
            Location = new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthority
            },
            Subject = subject,
            Year = 2020,
            TimeIdentifier = TimeIdentifier.AcademicYearQ4
        };

        var subjectObservation3 = new Observation
        {
            Location = new Location
            {
                GeographicLevel = GeographicLevel.LocalAuthorityDistrict
            },
            Subject = subject,
            Year = 2021,
            TimeIdentifier = TimeIdentifier.AcademicYearQ1
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(releaseVersion);
            statisticsDbContext.Subject.AddRange(subject);
            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject);
            statisticsDbContext.Observation.AddRange(
                subjectObservation1,
                subjectObservation2,
                subjectObservation3);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(statisticsDbContext: statisticsDbContext);

            var result = await service.ListGeographicLevels(subject.Id);

            Assert.Equal(3, result.Count);
            Assert.Equal("National", result[0]);
            Assert.Equal("Local authority", result[1]);
            Assert.Equal("Local authority district", result[2]);
        }
    }

    [Fact]
    public async Task ListGeographicLevels_NoObservations()
    {
        var releaseVersion = new ReleaseVersion();

        var subject = new Subject();

        var releaseSubject = new ReleaseSubject
        {
            ReleaseVersion = releaseVersion,
            Subject = subject
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(releaseVersion);
            statisticsDbContext.Subject.AddRange(subject);
            statisticsDbContext.ReleaseSubject.AddRange(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var service = SetupService(statisticsDbContext: statisticsDbContext);

            var result = await service.ListGeographicLevels(subject.Id);

            Assert.Empty(result);
        }
    }

    private static DataGuidanceDataSetService SetupService(
        StatisticsDbContext? statisticsDbContext = null,
        ContentDbContext? contentDbContext = null,
        IIndicatorRepository? indicatorRepository = null,
        IFootnoteRepository? footnoteRepository = null,
        ITimePeriodService? timePeriodService = null)
    {
        statisticsDbContext ??= InMemoryStatisticsDbContext();
        contentDbContext ??= InMemoryContentDbContext();

        return new(
            statisticsDbContext,
            contentDbContext,
            indicatorRepository ?? new IndicatorRepository(statisticsDbContext),
            footnoteRepository ?? new FootnoteRepository(statisticsDbContext),
            timePeriodService ?? new TimePeriodService(statisticsDbContext)
        );
    }
}

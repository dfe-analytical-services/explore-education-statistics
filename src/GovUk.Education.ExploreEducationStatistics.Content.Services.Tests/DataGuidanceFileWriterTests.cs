using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Snapshooter.Xunit;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using File = System.IO.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class DataGuidanceFileWriterTests : IDisposable
{
    private readonly DataFixture _dataFixture = new();

    private const string TestDataGuidance = @"
            <h2>Description</h2>
            <p>
                This document describes the data included in the ‘Children looked after in England 
                including adoptions: 2019 to 2020’ Accredited Official Statistics release’s underlying data files. 
                This data is released under the terms of the <a href=""https://the-license.gov.uk"">Open Government License</a> 
                and is intended to meet at least three stars for Open Data.
            </p>
            <h2>Coverage</h2>
            <p>
                This data is based upon information collected in the SSDA903 (CLA) data collection. It is a 
                child level dataset collected from local authorities in England annually. The data includes 
                information on:
            </p>
            <ul>
                <li>children looked after at 31 March in each year, including unaccompanied asylum seeking children</li>
                <li>children looked after at any time</li>
                <li>children who started to be looked after</li>
                <li>children who ceased to be looked after</li>
            </ul>
            ";

    private const string TestBasicDataGuidance = @"
            <p>
                This document describes the data included in the ‘Children looked after in England'
            </p>
        ";

    private readonly List<string> _filePaths = new();

    public void Dispose()
    {
        // Cleanup any files that have been
        // written to the filesystem.
        _filePaths.ForEach(File.Delete);
    }

    [Fact]
    public async Task WriteToStream_ListDataSetsReturnsNotFound()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestDataGuidance);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(new NotFoundResult());

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            var path = GenerateFilePath();
            await using var stream = File.OpenWrite(path);

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => { await writer.WriteToStream(stream, releaseVersion); }
            );

            Assert.Equal($"Could not find data sets for release version: {releaseVersion.Id}", exception.Message);
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_MultipleDataSets()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                Content = @"
                        <p>
                            Local authority level data on care leavers aged 17 to 21, by accommodation type (as 
                            measured on or around their birthday).
                        </p>",
                GeographicLevels = new List<string>
                {
                    "Local Authority",
                    "National",
                    "Regional"
                },
                TimePeriods = new TimePeriodLabels("2018", "2020"),
                Variables = new List<LabelValue>
                {
                    new("Accommodation type", "accommodation_type"),
                    new("Age at end", "age_end"),
                    new("Number of leavers", "number_of_leavers"),
                    new("Percentage of leavers by accommodation type", "percentage_of_leavers"),
                },
                Footnotes = new List<FootnoteViewModel>
                {
                    new(Guid.NewGuid(), "Data set 1 footnote 1"),
                    new(Guid.NewGuid(), "Data set 1 footnote 2"),
                }
            },
            new()
            {
                Filename = "test-2.csv",
                Name = "Test data 2",
                Content = @"
                        <p>
                            Number and proportion of population participating in education, training and employment 
                            by age, gender and labour market status.
                        </p>",
                GeographicLevels = new List<string>
                {
                    "National",
                },
                TimePeriods = new TimePeriodLabels("2018", "2018"),
                Variables = new List<LabelValue>
                {
                    new("Academic age", "age"),
                    new("Activity", "category"),
                    new("Gender", "gender"),
                    new("Labour market status", "labour_market_status"),
                },
                Footnotes = new List<FootnoteViewModel>
                {
                    new(Guid.NewGuid(), "Data set 2 footnote 1"),
                    new(Guid.NewGuid(), "Data set 2 footnote 2"),
                    new(Guid.NewGuid(), "Data set 2 footnote 3"),
                    new(Guid.NewGuid(), "Data set 2 footnote 4"),
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_SingleDataSet()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                Content = @"
                        <p>
                            Local authority level data on care leavers aged 17 to 21, by accommodation type (as 
                            measured on or around their birthday). See <a href=""https://test.com"">reference information</a>.
                        </p>",
                GeographicLevels = new List<string>
                {
                    "Local Authority",
                    "National",
                    "Regional"
                },
                TimePeriods = new TimePeriodLabels("2018", "2020"),
                Variables = new List<LabelValue>
                {
                    new("Accommodation type", "accommodation_type"),
                    new("Age at end", "age_end"),
                    new("Number of leavers", "number_of_leavers"),
                    new("Percentage of leavers by accommodation type", "percentage_of_leavers"),
                },
                Footnotes = new List<FootnoteViewModel>
                {
                    new(Guid.NewGuid(), "Footnote 1"),
                    new(Guid.NewGuid(), "Footnote 2"),
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_NoDataGuidance()
    {
        // Release has no data guidance (aka data guidance)
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(string.Empty);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                Content = @"
                        <p>
                            Local authority level data on care leavers aged 17 to 21, by accommodation type (as 
                            measured on or around their birthday).
                        </p>",
                GeographicLevels = new List<string>
                {
                    "Local Authority",
                    "National",
                    "Regional"
                },
                TimePeriods = new TimePeriodLabels("2018", "2020"),
                Variables = new List<LabelValue>
                {
                    new("Accommodation type", "accommodation_type"),
                    new("Age at end", "age_end"),
                    new("Number of leavers", "number_of_leavers"),
                    new("Percentage of leavers by accommodation type", "percentage_of_leavers"),
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_EmptyDataSets()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestDataGuidance);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(new List<DataGuidanceDataSetViewModel>());

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_FileWithSingleProperties()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestBasicDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                Content = "<p>Test file content</p>",
                GeographicLevels = new List<string>
                {
                    "Local Authority",
                },
                TimePeriods = new TimePeriodLabels("2018", "2018"),
                Variables = new List<LabelValue>
                {
                    new("Accommodation type", "accommodation_type"),
                },
                Footnotes = new List<FootnoteViewModel>
                {
                    new(Guid.NewGuid(), "Footnote 1")
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_FileWithEmptyProperties()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestBasicDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_FileWithEmptyTimePeriodStart()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestBasicDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                Content = "",
                GeographicLevels = new List<string>(),
                TimePeriods = new TimePeriodLabels("", "2019"),
                Variables = new List<LabelValue>()
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_FileWithEmptyTimePeriodEnd()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestBasicDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                TimePeriods = new TimePeriodLabels("2018", ""),
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_FileWithEmptyVariable()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestBasicDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                TimePeriods = new TimePeriodLabels("2018", ""),
                Variables = new List<LabelValue>
                {
                    new("", "")
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_FileWithOverTenFootnotesAndMultiline()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestBasicDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                Content = "",
                GeographicLevels = new List<string>(),
                TimePeriods = new TimePeriodLabels("2018", ""),
                Variables = new List<LabelValue>
                {
                    new("Accommodation type", "accommodation_type"),
                },
                Footnotes = new List<FootnoteViewModel>
                {
                    new(Guid.NewGuid(), "Footnote 1"),
                    new(Guid.NewGuid(), "Footnote 2"),
                    new(Guid.NewGuid(), "Footnote 3"),
                    new(Guid.NewGuid(), "Footnote 4"),
                    new(Guid.NewGuid(), "Footnote 5"),
                    new(Guid.NewGuid(), "Footnote 6"),
                    new(Guid.NewGuid(), "Footnote 7"),
                    new(Guid.NewGuid(), "Footnote 8"),
                    // New lines are stripped out by text conversion
                    new(
                        Guid.NewGuid(),
                        string.Join("\n", "Footnote 9", "over some", "lines.")
                    ),
                    new(Guid.NewGuid(), "Footnote 10"),
                    new(
                        Guid.NewGuid(),
                        string.Join("\n", "Footnote 11", "over some", "other lines.")
                    ),
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_FileWithHtmlFootnotes()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestBasicDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                Content = "",
                GeographicLevels = new List<string>(),
                TimePeriods = new TimePeriodLabels("2018", ""),
                Variables = new List<LabelValue>
                {
                    new("Accommodation type", "accommodation_type"),
                },
                Footnotes = new List<FootnoteViewModel>
                {
                    new(Guid.NewGuid(), "<p>Footnote with paragraph 1</p><p>And paragraph 2</p>"),
                    new(Guid.NewGuid(), @"Footnote with <a href=""https://test.com"">some link</a> embedded"),
                    new(Guid.NewGuid(), @"<a href=""https://test-2.com"">Another footnote link</a>"),
                    new(Guid.NewGuid(), "A plain footnote"),
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_FileWithEmptyFootnote()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestBasicDataGuidance);

        var dataSets = new List<DataGuidanceDataSetViewModel>
        {
            new()
            {
                Filename = "test-1.csv",
                Name = "Test data 1",
                Content = "",
                GeographicLevels = new List<string>(),
                TimePeriods = new TimePeriodLabels("2018", ""),
                Variables = new List<LabelValue>
                {
                    new("Accommodation type", "accommodation_type"),
                },
                Footnotes = new List<FootnoteViewModel>
                {
                    new(Guid.NewGuid(), "")
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(dataSets);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            await using var stream = new MemoryStream();
            await writer.WriteToStream(stream, releaseVersion);

            Snapshot.Match(stream.ReadToEnd());
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    [Fact]
    public async Task WriteToStream_FileStream()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()))
            .WithDataGuidance(TestBasicDataGuidance);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, CancellationToken.None))
            .ReturnsAsync(new List<DataGuidanceDataSetViewModel>());

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Entry(releaseVersion).ReloadAsync();

            var writer = BuildDataGuidanceFileWriter(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            var path = GenerateFilePath();
            await using var stream = File.OpenWrite(path);

            await writer.WriteToStream(stream, releaseVersion);

            var text = await File.ReadAllTextAsync(path);

            Assert.Contains(releaseVersion.Release.Publication.Title, text);
        }

        VerifyAllMocks(dataGuidanceDataSetService);
    }

    private string GenerateFilePath()
    {
        var path = Path.GetTempPath() + Guid.NewGuid() + ".txt";
        _filePaths.Add(path);

        return path;
    }

    private static DataGuidanceFileWriter BuildDataGuidanceFileWriter(
        ContentDbContext contentDbContext,
        IDataGuidanceDataSetService? dataGuidanceDataSetService = null)
    {
        return new DataGuidanceFileWriter(
            contentDbContext,
            dataGuidanceDataSetService ?? Mock.Of<IDataGuidanceDataSetService>(Strict)
        );
    }
}

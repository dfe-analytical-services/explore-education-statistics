#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReplacementPlanServiceTests
{
    private readonly Country _england = new("E92000001", "England");
    private readonly LocalAuthority _derby = new("E06000015", "", "Derby");

    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task GetReplacementPlan_LinkedReplacementReleaseFilesNotFound()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();
        var nonExistantFileId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository.Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersion.Id, nonExistantFileId))
            .ReturnsAsync(new NotFoundResult());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                releaseFileRepository: releaseFileRepository.Object);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: nonExistantFileId);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetReplacementPlan_ReleaseNotFound()
    {
        var originalFile = new File
        {
            Type = FileType.Data,
            SubjectId = Guid.NewGuid(),
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = Guid.NewGuid(),
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Files.AddRangeAsync(originalFile, replacementFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(contentDbContext,
                statisticsDbContext);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: Guid.NewGuid(),
                originalFileId: originalFile.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetReplacementPlan_OriginalFileIsNotUsedByAnyDependentData()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile
        };

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>());

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository.Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                replacementReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                replacementReleaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(contentDbContext,
                statisticsDbContext,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id);

            VerifyAllMocks(locationRepository,
                timePeriodService);

            var replacementPlan = result.AssertRight();

            Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

            Assert.Empty(replacementPlan.DataBlocks);
            Assert.Empty(replacementPlan.Footnotes);
            Assert.True(replacementPlan.Valid);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_NoReplacementDataPresent_ReplacementInvalid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile
        };

        var originalFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Original Test filter item"
        };

        var replacementFilterItem = new FilterItem
        {
            Label = "Replacement Test filter item"
        };

        var originalFilterGroup = new FilterGroup
        {
            Label = "Original Default group",
            FilterItems = new List<FilterItem>
            {
                originalFilterItem
            }
        };

        var replacementFilterGroup = new FilterGroup
        {
            Label = "Replacement Default group",
            FilterItems = new List<FilterItem>
            {
                replacementFilterItem
            }
        };

        var originalFilter = new Filter
        {
            Label = "Original Test filter",
            Name = "original_test_filter",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                originalFilterGroup
            }
        };

        var replacementFilter = new Filter
        {
            Label = "Replacement Test filter",
            Name = "replacement_test_filter",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                replacementFilterGroup
            }
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Original Indicator",
            Name = "original_indicator"
        };

        var replacementIndicator = new Indicator
        {
            Label = "Replacement Indicator",
            Name = "replacement_indicator"
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Original Default group",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                originalIndicator
            }
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Replacement Default group",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                replacementIndicator
            }
        };

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] {originalFilterItem.Id},
                Indicators = new[] {originalIndicator.Id},
                LocationIds = ListOf(originalLocation.Id),
                TimePeriod = timePeriod
            },
            ReleaseVersion = releaseVersion
        };

        var footnoteForSubject = CreateFootnote(statsReleaseVersion,
            "Test footnote for Subject",
            subject: originalReleaseSubject.Subject);

        var footnoteForFilter = CreateFootnote(statsReleaseVersion,
            "Test footnote for Filter",
            filterFootnotes: new List<FilterFootnote>
            {
                new()
                {
                    Filter = originalFilter
                }
            });

        var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion,
            "Test footnote for Filter group",
            filterGroupFootnotes: new List<FilterGroupFootnote>
            {
                new()
                {
                    FilterGroup = originalFilterGroup
                }
            });

        var footnoteForFilterItem = CreateFootnote(statsReleaseVersion,
            "Test footnote for Filter item",
            filterItemFootnotes: new List<FilterItemFootnote>
            {
                new()
                {
                    FilterItem = originalFilterItem
                }
            });

        var footnoteForIndicator = CreateFootnote(statsReleaseVersion,
            "Test footnote for Filter item",
            indicatorFootnotes: new List<IndicatorFootnote>
            {
                new()
                {
                    Indicator = originalIndicator
                }
            });

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>());

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository.Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalFilter, replacementFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                replacementIndicatorGroup);
            statisticsDbContext.Footnote.AddRange(footnoteForFilter, footnoteForFilterGroup,
                footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
            statisticsDbContext.Location.AddRange(originalLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(contentDbContext,
                statisticsDbContext,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id);

            VerifyAllMocks(locationRepository,
                timePeriodService);

            var replacementPlan = result.AssertRight();

            Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
            Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

            Assert.Single(dataBlockPlan.IndicatorGroups);

            var dataBlockIndicatorGroupPlan = dataBlockPlan.IndicatorGroups.First();

            Assert.Equal(originalIndicator.IndicatorGroup.Id, dataBlockIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicator.IndicatorGroup.Label, dataBlockIndicatorGroupPlan.Value.Label);
            Assert.Single(dataBlockIndicatorGroupPlan.Value.Indicators);
            Assert.False(dataBlockIndicatorGroupPlan.Value.Valid);

            var dataBlockIndicatorPlan = dataBlockIndicatorGroupPlan.Value.Indicators.First();
            Assert.Equal(originalIndicator.Id, dataBlockIndicatorPlan.Id);
            Assert.Equal(originalIndicator.Label, dataBlockIndicatorPlan.Label);
            Assert.Equal(originalIndicator.Name, dataBlockIndicatorPlan.Name);
            Assert.Null(dataBlockIndicatorPlan.Target);
            Assert.False(dataBlockIndicatorPlan.Valid);

            Assert.Single(dataBlockPlan.Filters);

            var dataBlockFilterPlan = dataBlockPlan.Filters.First();

            Assert.Equal(originalFilter.Id, dataBlockFilterPlan.Key);
            Assert.Equal(originalFilter.Id, dataBlockFilterPlan.Value.Id);
            Assert.Equal(originalFilter.Label, dataBlockFilterPlan.Value.Label);
            Assert.Equal(originalFilter.Name, dataBlockFilterPlan.Value.Name);
            Assert.False(dataBlockFilterPlan.Value.Valid);

            Assert.Single(dataBlockFilterPlan.Value.Groups);

            var dataBlockFilterGroupPlan = dataBlockFilterPlan.Value.Groups.First();

            Assert.Equal(originalFilterGroup.Id, dataBlockFilterGroupPlan.Key);
            Assert.Equal(originalFilterGroup.Id, dataBlockFilterGroupPlan.Value.Id);
            Assert.Equal(originalFilterGroup.Label, dataBlockFilterGroupPlan.Value.Label);
            Assert.False(dataBlockFilterGroupPlan.Value.Valid);

            Assert.Single(dataBlockFilterGroupPlan.Value.Filters);

            var dataBlockFilterItemPlan = dataBlockFilterGroupPlan.Value.Filters.First();

            Assert.Equal(originalFilterItem.Id, dataBlockFilterItemPlan.Id);
            Assert.Equal(originalFilterItem.Label, dataBlockFilterItemPlan.Label);
            Assert.Null(dataBlockFilterItemPlan.Target);
            Assert.False(dataBlockFilterItemPlan.Valid);

            Assert.NotNull(dataBlockPlan.Locations);
            Assert.Single(dataBlockPlan.Locations);
            Assert.True(dataBlockPlan.Locations.ContainsKey(GeographicLevel.Country.ToString()));
            Assert.Single(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].LocationAttributes);
            Assert.False(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Valid);

            var dataBlockLocationPlan = dataBlockPlan
                .Locations[GeographicLevel.Country.ToString()]
                .LocationAttributes
                .First();

            Assert.Equal(originalLocation.Id, dataBlockLocationPlan.Id);
            Assert.Equal(_england.Code, dataBlockLocationPlan.Code);
            Assert.Equal(_england.Name, dataBlockLocationPlan.Label);
            Assert.Null(dataBlockLocationPlan.Target);
            Assert.False(dataBlockLocationPlan.Valid);

            Assert.NotNull(dataBlockPlan.TimePeriods);
            Assert.False(dataBlockPlan.TimePeriods!.Valid);

            Assert.Equal(timePeriod.StartYear, dataBlockPlan.TimePeriods.Start.Year);
            Assert.Equal(timePeriod.StartCode, dataBlockPlan.TimePeriods.Start.Code);
            Assert.False(dataBlockPlan.TimePeriods.Start.Valid);

            Assert.Equal(timePeriod.EndYear, dataBlockPlan.TimePeriods.End.Year);
            Assert.Equal(timePeriod.EndCode, dataBlockPlan.TimePeriods.End.Code);
            Assert.False(dataBlockPlan.TimePeriods.End.Valid);

            Assert.False(dataBlockPlan.Valid);
            Assert.False(dataBlockPlan.Fixable);

            Assert.Equal(5, replacementPlan.Footnotes.Count());

            var footnoteForFilterPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilter.Id);

            Assert.Equal(footnoteForFilter.Content, footnoteForFilterPlan.Content);
            Assert.Single(footnoteForFilterPlan.Filters);
            Assert.Empty(footnoteForFilterPlan.FilterGroups);
            Assert.Empty(footnoteForFilterPlan.FilterItems);
            Assert.Empty(footnoteForFilterPlan.IndicatorGroups);

            var footnoteForFilterFilterPlan = footnoteForFilterPlan.Filters.First();

            Assert.Equal(originalFilter.Id, footnoteForFilterFilterPlan.Id);
            Assert.Equal(originalFilter.Label, footnoteForFilterFilterPlan.Label);
            Assert.Null(footnoteForFilterFilterPlan.Target);
            Assert.False(footnoteForFilterFilterPlan.Valid);

            Assert.False(footnoteForFilterPlan.Valid);

            var footnoteForFilterGroupPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterGroup.Id);

            Assert.False(footnoteForFilterGroupPlan.Valid);

            Assert.Equal(footnoteForFilterGroup.Content, footnoteForFilterGroupPlan.Content);
            Assert.Empty(footnoteForFilterGroupPlan.Filters);
            Assert.Single(footnoteForFilterGroupPlan.FilterGroups);
            Assert.Empty(footnoteForFilterGroupPlan.FilterItems);
            Assert.Empty(footnoteForFilterGroupPlan.IndicatorGroups);

            var footnoteForFilterGroupFilterGroupPlan = footnoteForFilterGroupPlan.FilterGroups.First();

            Assert.Equal(originalFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Id);
            Assert.Equal(originalFilterGroup.Label, footnoteForFilterGroupFilterGroupPlan.Label);
            Assert.Equal(originalFilterGroup.Filter.Id, footnoteForFilterGroupFilterGroupPlan.FilterId);
            Assert.Equal(originalFilterGroup.Filter.Label, footnoteForFilterGroupFilterGroupPlan.FilterLabel);
            Assert.Null(footnoteForFilterGroupFilterGroupPlan.Target);
            Assert.False(footnoteForFilterGroupFilterGroupPlan.Valid);

            Assert.False(footnoteForFilterGroupPlan.Valid);

            var footnoteForFilterItemPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterItem.Id);

            Assert.Equal(footnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
            Assert.Empty(footnoteForFilterItemPlan.Filters);
            Assert.Empty(footnoteForFilterItemPlan.FilterGroups);
            Assert.Single(footnoteForFilterItemPlan.FilterItems);
            Assert.Empty(footnoteForFilterItemPlan.IndicatorGroups);

            var footnoteForFilterItemFilterItemPlan = footnoteForFilterItemPlan.FilterItems.First();

            Assert.Equal(originalFilterItem.Id, footnoteForFilterItemFilterItemPlan.Id);
            Assert.Equal(originalFilterItem.Label, footnoteForFilterItemFilterItemPlan.Label);
            Assert.Equal(originalFilterItem.FilterGroup.Filter.Id, footnoteForFilterItemFilterItemPlan.FilterId);
            Assert.Equal(originalFilterItem.FilterGroup.Filter.Label,
                footnoteForFilterItemFilterItemPlan.FilterLabel);
            Assert.Equal(originalFilterItem.FilterGroup.Id, footnoteForFilterItemFilterItemPlan.FilterGroupId);
            Assert.Equal(originalFilterItem.FilterGroup.Label,
                footnoteForFilterItemFilterItemPlan.FilterGroupLabel);
            Assert.Null(footnoteForFilterItemFilterItemPlan.Target);
            Assert.False(footnoteForFilterItemFilterItemPlan.Valid);

            Assert.False(footnoteForFilterItemPlan.Valid);

            var footnoteForIndicatorPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);

            Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);
            Assert.Empty(footnoteForIndicatorPlan.Filters);
            Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
            Assert.Empty(footnoteForIndicatorPlan.FilterItems);
            Assert.Single(footnoteForIndicatorPlan.IndicatorGroups);

            var footnoteForIndicatorIndicatorGroupPlan = footnoteForIndicatorPlan.IndicatorGroups.First();

            Assert.Equal(originalIndicator.IndicatorGroup.Id, footnoteForIndicatorIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicator.IndicatorGroup.Label,
                footnoteForIndicatorIndicatorGroupPlan.Value.Label);
            Assert.Single(footnoteForIndicatorIndicatorGroupPlan.Value.Indicators);
            Assert.False(footnoteForIndicatorIndicatorGroupPlan.Value.Valid);

            var footnoteForIndicatorIndicatorPlan =
                footnoteForIndicatorIndicatorGroupPlan.Value.Indicators.First();

            Assert.Equal(originalIndicator.Id, footnoteForIndicatorIndicatorPlan.Id);
            Assert.Equal(originalIndicator.Label, footnoteForIndicatorIndicatorPlan.Label);
            Assert.Equal(originalIndicator.Name, footnoteForIndicatorIndicatorPlan.Name);
            Assert.Null(footnoteForIndicatorIndicatorPlan.Target);
            Assert.False(footnoteForIndicatorIndicatorPlan.Valid);

            Assert.False(footnoteForIndicatorPlan.Valid);

            var footnoteForSubjectPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForSubject.Id);

            Assert.Equal(footnoteForSubject.Content, footnoteForSubjectPlan.Content);
            Assert.Empty(footnoteForSubjectPlan.Filters);
            Assert.Empty(footnoteForSubjectPlan.FilterGroups);
            Assert.Empty(footnoteForSubjectPlan.FilterItems);
            Assert.Empty(footnoteForSubjectPlan.IndicatorGroups);

            Assert.True(footnoteForSubjectPlan.Valid);

            Assert.False(replacementPlan.Valid);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_SelectedFilterItemsNoLongerExistButSomeDo_ReplacementInvalidButFixable()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile
        };

        var originalDefaultFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item"
        };

        var originalDefaultFilterItem2 = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item 2"
        };

        var replacementDefaultFilterItem = new FilterItem
        {
            Label = "Test filter item",
        };

        var originalDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                originalDefaultFilterItem,
                originalDefaultFilterItem2
            }
        };

        var replacementDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                replacementDefaultFilterItem
            }
        };

        var originalDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                originalDefaultFilterGroup
            }
        };

        var replacementDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                replacementDefaultFilterGroup
            }
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                originalIndicator
            }
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                replacementIndicator
            }
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[]
                {
                    originalDefaultFilterItem.Id,
                    originalDefaultFilterItem2.Id
                },
                Indicators = new[] {originalIndicator.Id},
                LocationIds = ListOf(location.Id),
                TimePeriod = timePeriod
            },
            ReleaseVersion = releaseVersion
        };

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>
            {
                location
            });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2019, CalendarYear),
                (2020, CalendarYear)
            });

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository.Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalDefaultFilter, replacementDefaultFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(location);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(contentDbContext,
                statisticsDbContext,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id);

            VerifyAllMocks(locationRepository,
                timePeriodService);

            var replacementPlan = result.AssertRight();
            Assert.False(replacementPlan.Valid);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.False(dataBlockPlan.Valid);
            Assert.True(dataBlockPlan.Fixable);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_AllOriginalFilterItemsNoLongerExist_ReplacementInvalidAndNotFixable()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile
        };

        var originalDefaultFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item"
        };

        var replacementDefaultFilterItem = new FilterItem
        {
            Label = "Test filter item - changing!",
        };

        var originalDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                originalDefaultFilterItem
            }
        };

        var replacementDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                replacementDefaultFilterItem,
            }
        };

        var originalDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                originalDefaultFilterGroup
            }
        };

        var replacementDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                replacementDefaultFilterGroup
            }
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                originalIndicator
            }
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                replacementIndicator
            }
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[]
                {
                    originalDefaultFilterItem.Id
                },
                Indicators = new[] {originalIndicator.Id},
                LocationIds = ListOf(location.Id),
                TimePeriod = timePeriod
            },
            ReleaseVersion = releaseVersion
        };

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>
            {
                location
            });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2019, CalendarYear),
                (2020, CalendarYear)
            });

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository.Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalDefaultFilter, replacementDefaultFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(location);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(contentDbContext,
                statisticsDbContext,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id);

            VerifyAllMocks(locationRepository,
                timePeriodService);

            var replacementPlan = result.AssertRight();
            Assert.False(replacementPlan.Valid);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.False(dataBlockPlan.Valid);
            Assert.False(dataBlockPlan.Fixable);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_NewFiltersIntroduced_ReplacementInvalidAndNotFixable()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile
        };

        var originalDefaultFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item"
        };

        var replacementDefaultFilterItem = new FilterItem
        {
            Label = "Test filter item"
        };

        var replacementNewlyIntroducedFiltersFilterItem = new FilterItem
        {
            Label = "Filter item for newly introduced Filter"
        };

        var originalDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                originalDefaultFilterItem
            }
        };

        var replacementDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                replacementDefaultFilterItem
            }
        };

        var replacementNewlyIntroducedFilterGroup = new FilterGroup
        {
            Label = "Newly introduced filter group",
            FilterItems = new List<FilterItem>
            {
                replacementNewlyIntroducedFiltersFilterItem
            }
        };

        var originalDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                originalDefaultFilterGroup
            }
        };

        var replacementDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                replacementDefaultFilterGroup
            }
        };

        var replacementNewlyIntroducedFilter = new Filter
        {
            Label = "Newly introduced filter",
            Name = "newly_introduced_filter",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                replacementNewlyIntroducedFilterGroup
            }
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                originalIndicator
            }
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                replacementIndicator
            }
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[]
                {
                    originalDefaultFilterItem.Id
                },
                Indicators = new[] {originalIndicator.Id},
                LocationIds = ListOf(location.Id),
                TimePeriod = timePeriod
            },
            ReleaseVersion = releaseVersion
        };

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>
            {
                location
            });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2019, CalendarYear),
                (2020, CalendarYear)
            });

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository.Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalDefaultFilter,
                replacementDefaultFilter, replacementNewlyIntroducedFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(location);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(contentDbContext,
                statisticsDbContext,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id);

            VerifyAllMocks(locationRepository,
                timePeriodService);

            var replacementPlan = result.AssertRight();
            Assert.False(replacementPlan.Valid);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.False(dataBlockPlan.Valid);
            Assert.False(dataBlockPlan.Fixable);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_ReplacementHasDifferentLocation_LocationMatchedByCode_ReplacementValid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile
        };

        var originalFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing"
        };

        var replacementFilterItem = new FilterItem
        {
            Label = "Test filter item - not changing"
        };

        var originalFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                originalFilterItem
            }
        };

        var replacementFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                replacementFilterItem
            }
        };

        var originalFilter = new Filter
        {
            Label = "Filter - not changing",
            Name = "filter_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                originalFilterGroup
            }
        };

        var replacementFilter = new Filter
        {
            Label = "Filter - not changing",
            Name = "filter_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                replacementFilterGroup
            }
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                originalIndicator
            }
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                replacementIndicator
            }
        };

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = _derby
        };

        // Replacement location has a different id but the primary attribute code remains the same
        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            Country = _england,
            LocalAuthority = _derby
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] {originalFilterItem.Id},
                Indicators = new[] {originalIndicator.Id},
                LocationIds = ListOf(originalLocation.Id),
                TimePeriod = timePeriod
            },
            ReleaseVersion = releaseVersion
        };

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>
            {
                replacementLocation
            });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2019, CalendarYear),
                (2020, CalendarYear)
            });

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository.Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalFilter, replacementFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(originalLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(contentDbContext,
                statisticsDbContext,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id);

            VerifyAllMocks(locationRepository,
                timePeriodService);

            var replacementPlan = result.AssertRight();

            Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
            Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

            Assert.NotNull(dataBlockPlan.Locations);
            Assert.Single(dataBlockPlan.Locations);
            Assert.True(dataBlockPlan.Locations.ContainsKey(GeographicLevel.LocalAuthority.ToString()));
            Assert.Single(dataBlockPlan.Locations[GeographicLevel.LocalAuthority.ToString()].LocationAttributes);
            Assert.True(dataBlockPlan.Locations[GeographicLevel.LocalAuthority.ToString()].Valid);

            var dataBlockLocationPlan = dataBlockPlan
                .Locations[GeographicLevel.LocalAuthority.ToString()]
                .LocationAttributes
                .First();

            Assert.Equal(originalLocation.Id, dataBlockLocationPlan.Id);
            Assert.Equal(_derby.Code, dataBlockLocationPlan.Code);
            Assert.Equal(_derby.Name, dataBlockLocationPlan.Label);
            Assert.Equal(replacementLocation.Id, dataBlockLocationPlan.Target);
            Assert.True(dataBlockLocationPlan.Valid);

            Assert.True(dataBlockPlan.Valid);
            Assert.True(replacementPlan.Valid);
        }
    }

    /// <summary>
    /// Please note this test will reduce in the number of cases when the feature flag has been removed from it.
    /// </summary>
    /// <param name="dataSetVersionStatus">The data set version status of the replaced file's API data set version</param>
    /// <param name="majorVersionUpdate">Whether the user has uploaded a file that results in a major version update</param>
    /// <param name="enableReplacementOfPublicApiDataSets">Whether the feature flag for EES-5779 is switched on or off</param>
    /// <param name="expectedValidValue">The expected value for the scenario set up</param>
    [Theory]
    //When user has uploaded major version data set
    [InlineData(DataSetVersionStatus.Published, true, true, false)]
    [InlineData(DataSetVersionStatus.Mapping, true, true, false)]
    [InlineData(DataSetVersionStatus.Draft, true, true, false)]
    [InlineData(DataSetVersionStatus.Published, true, false, false)]
    [InlineData(DataSetVersionStatus.Mapping, true, false, false)]
    [InlineData(DataSetVersionStatus.Draft, true, false, false)]
    //When user has uploaded minor version
    [InlineData(DataSetVersionStatus.Published, false, true, false)]
    [InlineData(DataSetVersionStatus.Mapping, false, true, false)]
    [InlineData(DataSetVersionStatus.Draft, false, true, true)]
    [InlineData(DataSetVersionStatus.Published, false, false, false)]
    [InlineData(DataSetVersionStatus.Mapping, false, false, false)]
    [InlineData(DataSetVersionStatus.Draft, false, false, false)]
    //When API data set version status is not appropriate to be replaced
    [InlineData(DataSetVersionStatus.Processing, false, true, false)]
    [InlineData(DataSetVersionStatus.Failed, false, true, false)]
    [InlineData(DataSetVersionStatus.Deprecated, false, true, false)]
    [InlineData(DataSetVersionStatus.Withdrawn, false, true, false)]
    [InlineData(DataSetVersionStatus.Cancelled, false, true, false)]
    [InlineData(DataSetVersionStatus.Processing, false, false, false)]
    [InlineData(DataSetVersionStatus.Failed, false, false, false)]
    [InlineData(DataSetVersionStatus.Deprecated, false, false, false)]
    [InlineData(DataSetVersionStatus.Withdrawn, false, false, false)]
    [InlineData(DataSetVersionStatus.Cancelled, false, false, false)]
    public async Task GetReplacementPlan_FileIsLinkedToPublicApiDataSet_ReplacementValidated(
        DataSetVersionStatus dataSetVersionStatus, 
        bool  majorVersionUpdate,
        bool enableReplacementOfPublicApiDataSets, 
        bool expectedValidValue)
    {
        DataSet dataSet = _fixture
            .DefaultDataSet();

        DataSetVersion dataSetVersion = _fixture
            .DefaultDataSetVersion()
            .WithVersionNumber(major: 1, minor: 1, patch: 1)
            .WithStatus(dataSetVersionStatus)
            .WithDataSet(dataSet);

        Content.Model.ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        File originalFile = _fixture
            .DefaultFile()
            .WithSubjectId(originalReleaseSubject.SubjectId)
            .WithType(FileType.Data);

        File replacementFile = _fixture
            .DefaultFile()
            .WithSubjectId(replacementReleaseSubject.SubjectId)
            .WithType(FileType.Data)
            .WithReplacing(originalFile);

        originalFile.ReplacedBy = replacementFile;

        var (originalReleaseFile, replacementReleaseFile) = _fixture.DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(0, rv =>
                rv.SetFile(originalFile)
                    .SetPublicApiDataSetId(dataSet.Id)
                    .SetPublicApiDataSetVersion(dataSetVersion.SemVersion()))
            .ForIndex(1, rv =>
                rv.SetFile(replacementFile)
                    .SetPublicApiDataSetId(dataSet.Id)
                    .SetPublicApiDataSetVersion(dataSetVersion.SemVersion()))
            .GenerateTuple2();

        var dataSetVersionService = new Mock<IDataSetVersionService>(Strict);
        dataSetVersionService.Setup(mock => mock.GetDataSetVersion(
            originalReleaseFile.PublicApiDataSetId!.Value,
            originalReleaseFile.PublicApiDataSetVersion!, 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSetVersion);

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>());

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository.Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var dataSetVersionMappingService = new Mock<IDataSetVersionMappingService>(Strict);
        dataSetVersionMappingService.Setup(service => service.GetMappingStatus(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MappingStatusViewModel
            {
                FiltersComplete = majorVersionUpdate,
                LocationsComplete = majorVersionUpdate,
                HasDeletionChanges = majorVersionUpdate,
                FiltersHaveMajorChange = majorVersionUpdate,
                LocationsHaveMajorChange = majorVersionUpdate
            });
        
        var options = Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
        {
            EnableReplacementOfPublicApiDataSets = enableReplacementOfPublicApiDataSets
        });
        
        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                dataSetVersionService: dataSetVersionService.Object,
                timePeriodService: timePeriodService.Object,
                locationRepository: locationRepository.Object,
                releaseFileRepository: releaseFileRepository.Object,
                dataSetVersionMappingService: dataSetVersionMappingService.Object,
                featureFlags: options);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id);

            VerifyAllMocks(dataSetVersionService);

            var replacementPlan = result.AssertRight();
            
            Assert.NotNull(replacementPlan.ApiDataSetVersionPlan);
            Assert.Equal(dataSet.Id, replacementPlan.ApiDataSetVersionPlan.DataSetId);
            Assert.Equal(dataSet.Title, replacementPlan.ApiDataSetVersionPlan.DataSetTitle);
            Assert.Equal(dataSetVersion.Id, replacementPlan.ApiDataSetVersionPlan.Id);
            Assert.Equal(dataSetVersion.PublicVersion, replacementPlan.ApiDataSetVersionPlan.Version);
            if (enableReplacementOfPublicApiDataSets)
            {
                Assert.Equal(expectedValidValue, replacementPlan.ApiDataSetVersionPlan.Valid);
            }
            else
            {
                Assert.Null(replacementPlan.ApiDataSetVersionPlan.MappingStatus);
                Assert.False(replacementPlan.ApiDataSetVersionPlan.Valid);
            }
            Assert.Equal(dataSetVersion.Status, replacementPlan.ApiDataSetVersionPlan.Status);

            Assert.Equal(replacementPlan.ApiDataSetVersionPlan.Valid, expectedValidValue);
            Assert.Equal(replacementPlan.Valid, expectedValidValue);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_AllReplacementDataPresent_ReplacementValid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
            .WithId(releaseVersion.Id)
            .Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
            .WithReleaseVersion(statsReleaseVersion)
            .WithSubjects(_fixture.DefaultSubject().Generate(2))
            .GenerateTuple2();

        var originalFile = new File
        {
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = originalReleaseSubject.SubjectId,
        };

        var replacementFile = new File
        {
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = originalFile
        };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = replacementFile
        };

        var originalDefaultFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing"
        };

        var originalPrimarySchoolsFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Primary schools"
        };

        var originalPrimaryAndSecondarySchoolsFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Primary and secondary schools"
        };

        var replacementDefaultFilterItem = new FilterItem
        {
            Label = "Test filter item - not changing"
        };

        var replacementPrimarySchoolsFilterItem = new FilterItem
        {
            Label = "Primary schools"
        };

        var replacementPrimaryAndSecondarySchoolsFilterItem = new FilterItem
        {
            Label = "Primary and secondary schools"
        };

        var originalDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                originalDefaultFilterItem
            }
        };

        var originalIndividualSchoolTypeFilterGroup = new FilterGroup
        {
            Label = "Individual",
            FilterItems = new List<FilterItem>
            {
                originalPrimarySchoolsFilterItem
            }
        };

        var originalCombinedSchoolTypeFilterGroup = new FilterGroup
        {
            Label = "Combined",
            FilterItems = new List<FilterItem>
            {
                originalPrimaryAndSecondarySchoolsFilterItem
            }
        };

        var replacementDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem>
            {
                replacementDefaultFilterItem
            }
        };

        var replacementIndividualSchoolTypeFilterGroup = new FilterGroup
        {
            Label = "Individual",
            FilterItems = new List<FilterItem>
            {
                replacementPrimarySchoolsFilterItem
            }
        };

        var replacementCombinedSchoolTypeFilterGroup = new FilterGroup
        {
            Label = "Combined",
            FilterItems = new List<FilterItem>
            {
                replacementPrimaryAndSecondarySchoolsFilterItem
            }
        };

        var originalDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                originalDefaultFilterGroup
            }
        };

        var originalSchoolTypeFilter = new Filter
        {
            Label = "School type",
            Name = "school_type",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                originalIndividualSchoolTypeFilterGroup,
                originalCombinedSchoolTypeFilterGroup,
            }
        };

        var replacementDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                replacementDefaultFilterGroup
            }
        };

        var replacementSchoolTypeFilter = new Filter
        {
            Label = "School type",
            Name = "school_type",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                replacementIndividualSchoolTypeFilterGroup,
                replacementCombinedSchoolTypeFilterGroup
            }
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing"
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                originalIndicator
            }
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator>
            {
                replacementIndicator
            }
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[]
                {
                    originalDefaultFilterItem.Id,
                    originalPrimarySchoolsFilterItem.Id,
                    originalPrimaryAndSecondarySchoolsFilterItem.Id,
                },
                Indicators = new[] {originalIndicator.Id},
                LocationIds = ListOf(location.Id),
                TimePeriod = timePeriod
            },
            ReleaseVersion = releaseVersion
        };

        var footnoteForFilter = CreateFootnote(statsReleaseVersion,
            "Test footnote for Filter",
            filterFootnotes: new List<FilterFootnote>
            {
                new()
                {
                    Filter = originalDefaultFilter
                }
            });

        var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion,
            "Test footnote for Filter group",
            filterGroupFootnotes: new List<FilterGroupFootnote>
            {
                new()
                {
                    FilterGroup = originalDefaultFilterGroup
                }
            });

        var footnoteForFilterItem = CreateFootnote(statsReleaseVersion,
            "Test footnote for Filter item",
            filterItemFootnotes: new List<FilterItemFootnote>
            {
                new()
                {
                    FilterItem = originalDefaultFilterItem
                }
            });

        var footnoteForIndicator = CreateFootnote(statsReleaseVersion,
            "Test footnote for Filter item",
            indicatorFootnotes: new List<IndicatorFootnote>
            {
                new()
                {
                    Indicator = originalIndicator
                }
            });

        var footnoteForSubject = CreateFootnote(statsReleaseVersion,
            "Test footnote for Subject",
            subject: originalReleaseSubject.Subject);

        var locationRepository = new Mock<ILocationRepository>(Strict);
        locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<Location>
            {
                location
            });

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
            {
                (2019, CalendarYear),
                (2020, CalendarYear)
            });

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository.Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(
            releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalDefaultFilter, originalSchoolTypeFilter,
                replacementDefaultFilter, replacementSchoolTypeFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(location);
            statisticsDbContext.Footnote.AddRange(footnoteForFilter, footnoteForFilterGroup,
                footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(contentDbContext,
                statisticsDbContext,
                locationRepository: locationRepository.Object,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id);

            VerifyAllMocks(locationRepository,
                timePeriodService);

            var replacementPlan = result.AssertRight();

            Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
            Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

            Assert.Single(dataBlockPlan.IndicatorGroups);

            var dataBlockIndicatorGroupPlan = dataBlockPlan.IndicatorGroups.First();

            Assert.Equal(originalIndicator.IndicatorGroup.Id, dataBlockIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicator.IndicatorGroup.Label, dataBlockIndicatorGroupPlan.Value.Label);
            Assert.Single(dataBlockIndicatorGroupPlan.Value.Indicators);
            Assert.True(dataBlockIndicatorGroupPlan.Value.Valid);

            var dataBlockIndicatorPlan = dataBlockIndicatorGroupPlan.Value.Indicators.First();

            Assert.Equal(originalIndicator.Id, dataBlockIndicatorPlan.Id);
            Assert.Equal(originalIndicator.Label, dataBlockIndicatorPlan.Label);
            Assert.Equal(originalIndicator.Name, dataBlockIndicatorPlan.Name);
            Assert.Equal(replacementIndicator.Id, dataBlockIndicatorPlan.Target);
            Assert.True(dataBlockIndicatorPlan.Valid);

            Assert.Equal(2, dataBlockPlan.Filters.Count);

            var dataBlockDefaultFilterPlan =
                dataBlockPlan.Filters.First(f => f.Key.Equals(originalDefaultFilter.Id));

            Assert.Equal(originalDefaultFilter.Id, dataBlockDefaultFilterPlan.Value.Id);
            Assert.Equal(originalDefaultFilter.Label, dataBlockDefaultFilterPlan.Value.Label);
            Assert.Equal(originalDefaultFilter.Name, dataBlockDefaultFilterPlan.Value.Name);
            Assert.True(dataBlockDefaultFilterPlan.Value.Valid);

            Assert.Single(dataBlockDefaultFilterPlan.Value.Groups);

            var dataBlockDefaultFilterGroupPlan = dataBlockDefaultFilterPlan.Value.Groups.First();

            Assert.Equal(originalDefaultFilterGroup.Id, dataBlockDefaultFilterGroupPlan.Key);
            Assert.Equal(originalDefaultFilterGroup.Id, dataBlockDefaultFilterGroupPlan.Value.Id);
            Assert.Equal(originalDefaultFilterGroup.Label, dataBlockDefaultFilterGroupPlan.Value.Label);
            Assert.Single(dataBlockDefaultFilterGroupPlan.Value.Filters);
            Assert.True(dataBlockDefaultFilterGroupPlan.Value.Valid);

            var dataBlockDefaultFilterItemPlan = dataBlockDefaultFilterGroupPlan.Value.Filters.First();

            Assert.Equal(originalDefaultFilterItem.Id, dataBlockDefaultFilterItemPlan.Id);
            Assert.Equal(originalDefaultFilterItem.Label, dataBlockDefaultFilterItemPlan.Label);
            Assert.Equal(replacementDefaultFilterItem.Id, dataBlockDefaultFilterItemPlan.Target);
            Assert.True(dataBlockDefaultFilterItemPlan.Valid);

            var dataBlockSchoolTypeFilterPlan =
                dataBlockPlan.Filters.First(f => f.Key.Equals(originalSchoolTypeFilter.Id));

            Assert.Equal(originalSchoolTypeFilter.Id, dataBlockSchoolTypeFilterPlan.Value.Id);
            Assert.Equal(originalSchoolTypeFilter.Label, dataBlockSchoolTypeFilterPlan.Value.Label);
            Assert.Equal(originalSchoolTypeFilter.Name, dataBlockSchoolTypeFilterPlan.Value.Name);
            Assert.True(dataBlockSchoolTypeFilterPlan.Value.Valid);

            Assert.Equal(2, dataBlockSchoolTypeFilterPlan.Value.Groups.Count);

            var dataBlockIndividualSchoolTypeFilterGroupPlan =
                dataBlockSchoolTypeFilterPlan.Value.Groups.First(g =>
                    g.Key == originalIndividualSchoolTypeFilterGroup.Id);

            Assert.Equal(originalIndividualSchoolTypeFilterGroup.Id,
                dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Id);
            Assert.Equal(originalIndividualSchoolTypeFilterGroup.Label,
                dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Label);
            Assert.Single(dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Filters);
            Assert.True(dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Valid);

            var dataBlockCombinedSchoolTypeFilterGroupPlan =
                dataBlockSchoolTypeFilterPlan.Value.Groups.First(g =>
                    g.Key == originalCombinedSchoolTypeFilterGroup.Id);

            Assert.Equal(originalCombinedSchoolTypeFilterGroup.Id,
                dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Id);
            Assert.Equal(originalCombinedSchoolTypeFilterGroup.Label,
                dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Label);
            Assert.Single(dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Filters);
            Assert.True(dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Valid);

            var dataBlockPrimarySchoolsFilterItemPlan =
                dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Filters.First();

            Assert.Equal(originalPrimarySchoolsFilterItem.Id, dataBlockPrimarySchoolsFilterItemPlan.Id);
            Assert.Equal(originalPrimarySchoolsFilterItem.Label, dataBlockPrimarySchoolsFilterItemPlan.Label);
            Assert.Equal(replacementPrimarySchoolsFilterItem.Id, dataBlockPrimarySchoolsFilterItemPlan.Target);
            Assert.True(dataBlockPrimarySchoolsFilterItemPlan.Valid);

            Assert.NotNull(dataBlockPlan.Locations);
            Assert.Single(dataBlockPlan.Locations);
            Assert.True(dataBlockPlan.Locations.ContainsKey(GeographicLevel.Country.ToString()));
            Assert.Single(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].LocationAttributes);
            Assert.True(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Valid);

            var dataBlockLocationPlan = dataBlockPlan
                .Locations[GeographicLevel.Country.ToString()]
                .LocationAttributes
                .First();

            Assert.Equal(location.Id, dataBlockLocationPlan.Id);
            Assert.Equal(_england.Code, dataBlockLocationPlan.Code);
            Assert.Equal(_england.Name, dataBlockLocationPlan.Label);
            Assert.Equal(location.Id, dataBlockLocationPlan.Target);
            Assert.True(dataBlockLocationPlan.Valid);

            Assert.NotNull(dataBlockPlan.TimePeriods);
            Assert.True(dataBlockPlan.TimePeriods?.Valid);

            Assert.Equal(timePeriod.StartYear, dataBlockPlan.TimePeriods?.Start.Year);
            Assert.Equal(timePeriod.StartCode, dataBlockPlan.TimePeriods?.Start.Code);
            Assert.True(dataBlockPlan.TimePeriods?.Start.Valid);

            Assert.Equal(timePeriod.EndYear, dataBlockPlan.TimePeriods?.End.Year);
            Assert.Equal(timePeriod.EndCode, dataBlockPlan.TimePeriods?.End.Code);
            Assert.True(dataBlockPlan.TimePeriods?.End.Valid);

            Assert.True(dataBlockPlan.Valid);
            Assert.False(dataBlockPlan.Fixable);

            Assert.Equal(5, replacementPlan.Footnotes.Count());

            var footnoteForFilterPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilter.Id);

            Assert.Equal(footnoteForFilter.Content, footnoteForFilterPlan.Content);
            Assert.Single(footnoteForFilterPlan.Filters);
            Assert.Empty(footnoteForFilterPlan.FilterGroups);
            Assert.Empty(footnoteForFilterPlan.FilterItems);
            Assert.Empty(footnoteForFilterPlan.IndicatorGroups);

            var footnoteForFilterFilterPlan = footnoteForFilterPlan.Filters.First();

            Assert.Equal(originalDefaultFilter.Id, footnoteForFilterFilterPlan.Id);
            Assert.Equal(originalDefaultFilter.Label, footnoteForFilterFilterPlan.Label);
            Assert.Equal(replacementDefaultFilter.Id, footnoteForFilterFilterPlan.Target);
            Assert.True(footnoteForFilterFilterPlan.Valid);

            Assert.True(footnoteForFilterPlan.Valid);

            var footnoteForFilterGroupPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterGroup.Id);

            Assert.Equal(footnoteForFilterGroup.Content, footnoteForFilterGroupPlan.Content);
            Assert.Empty(footnoteForFilterGroupPlan.Filters);
            Assert.Single(footnoteForFilterGroupPlan.FilterGroups);
            Assert.Empty(footnoteForFilterGroupPlan.FilterItems);
            Assert.Empty(footnoteForFilterGroupPlan.IndicatorGroups);

            var footnoteForFilterGroupFilterGroupPlan = footnoteForFilterGroupPlan.FilterGroups.First();

            Assert.Equal(originalDefaultFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Id);
            Assert.Equal(originalDefaultFilterGroup.Label, footnoteForFilterGroupFilterGroupPlan.Label);
            Assert.Equal(originalDefaultFilterGroup.Filter.Id, footnoteForFilterGroupFilterGroupPlan.FilterId);
            Assert.Equal(originalDefaultFilterGroup.Filter.Label,
                footnoteForFilterGroupFilterGroupPlan.FilterLabel);
            Assert.Equal(replacementDefaultFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Target);
            Assert.True(footnoteForFilterGroupFilterGroupPlan.Valid);

            Assert.True(footnoteForFilterGroupPlan.Valid);

            var footnoteForFilterItemPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterItem.Id);

            Assert.Equal(footnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
            Assert.Empty(footnoteForFilterItemPlan.Filters);
            Assert.Empty(footnoteForFilterItemPlan.FilterGroups);
            Assert.Single(footnoteForFilterItemPlan.FilterItems);
            Assert.Empty(footnoteForFilterItemPlan.IndicatorGroups);

            var footnoteForFilterItemFilterItemPlan = footnoteForFilterItemPlan.FilterItems.First();

            Assert.Equal(originalDefaultFilterItem.Id, footnoteForFilterItemFilterItemPlan.Id);
            Assert.Equal(originalDefaultFilterItem.Label, footnoteForFilterItemFilterItemPlan.Label);
            Assert.Equal(originalDefaultFilterItem.FilterGroup.Filter.Id,
                footnoteForFilterItemFilterItemPlan.FilterId);
            Assert.Equal(originalDefaultFilterItem.FilterGroup.Filter.Label,
                footnoteForFilterItemFilterItemPlan.FilterLabel);
            Assert.Equal(originalDefaultFilterItem.FilterGroup.Id,
                footnoteForFilterItemFilterItemPlan.FilterGroupId);
            Assert.Equal(originalDefaultFilterItem.FilterGroup.Label,
                footnoteForFilterItemFilterItemPlan.FilterGroupLabel);
            Assert.Equal(replacementDefaultFilterItem.Id, footnoteForFilterItemFilterItemPlan.Target);
            Assert.True(footnoteForFilterItemFilterItemPlan.Valid);

            Assert.True(footnoteForFilterItemPlan.Valid);

            var footnoteForIndicatorPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);
            Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);

            Assert.Empty(footnoteForIndicatorPlan.Filters);
            Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
            Assert.Empty(footnoteForIndicatorPlan.FilterItems);
            Assert.Single(footnoteForIndicatorPlan.IndicatorGroups);

            var footnoteForIndicatorIndicatorGroupPlan =
                footnoteForIndicatorPlan.IndicatorGroups.First();

            Assert.Equal(originalIndicator.IndicatorGroup.Id, footnoteForIndicatorIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicator.IndicatorGroup.Label,
                footnoteForIndicatorIndicatorGroupPlan.Value.Label);
            Assert.Single(footnoteForIndicatorIndicatorGroupPlan.Value.Indicators);
            Assert.True(footnoteForIndicatorIndicatorGroupPlan.Value.Valid);

            var footnoteForIndicatorIndicatorPlan =
                footnoteForIndicatorIndicatorGroupPlan.Value.Indicators.First();

            Assert.Equal(originalIndicator.Id, footnoteForIndicatorIndicatorPlan.Id);
            Assert.Equal(originalIndicator.Label, footnoteForIndicatorIndicatorPlan.Label);
            Assert.Equal(originalIndicator.Name, footnoteForIndicatorIndicatorPlan.Name);
            Assert.Equal(replacementIndicator.Id, footnoteForIndicatorIndicatorPlan.Target);
            Assert.True(footnoteForIndicatorIndicatorPlan.Valid);

            Assert.True(footnoteForIndicatorPlan.Valid);

            var footnoteForSubjectPlan =
                replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForSubject.Id);
            Assert.Equal(footnoteForSubject.Content, footnoteForSubjectPlan.Content);
            Assert.Empty(footnoteForSubjectPlan.Filters);
            Assert.Empty(footnoteForSubjectPlan.FilterGroups);
            Assert.Empty(footnoteForSubjectPlan.FilterItems);
            Assert.Empty(footnoteForSubjectPlan.IndicatorGroups);

            Assert.True(footnoteForSubjectPlan.Valid);

            Assert.Null(replacementPlan.ApiDataSetVersionPlan);

            Assert.True(replacementPlan.Valid);
        }
    }

    private static Footnote CreateFootnote(ReleaseVersion releaseVersion,
        string content,
        List<FilterFootnote>? filterFootnotes = null,
        List<FilterGroupFootnote>? filterGroupFootnotes = null,
        List<FilterItemFootnote>? filterItemFootnotes = null,
        List<IndicatorFootnote>? indicatorFootnotes = null,
        Subject? subject = null)
    {
        return new Footnote
        {
            Content = content,
            Filters = filterFootnotes ?? new List<FilterFootnote>(),
            FilterGroups = filterGroupFootnotes ?? new List<FilterGroupFootnote>(),
            FilterItems = filterItemFootnotes ?? new List<FilterItemFootnote>(),
            Indicators = indicatorFootnotes ?? new List<IndicatorFootnote>(),
            Subjects = subject != null
                ? new List<SubjectFootnote>
                {
                    new()
                    {
                        Subject = subject
                    }
                }
                : new List<SubjectFootnote>(),
            Releases = new List<ReleaseFootnote>
            {
                new()
                {
                    ReleaseVersion = releaseVersion
                }
            }
        };
    }

    private static ReplacementPlanService BuildReplacementPlanService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        ILocationRepository? locationRepository = null,
        IDataSetVersionService? dataSetVersionService = null,
        ITimePeriodService? timePeriodService = null,
        IDataSetVersionMappingService? dataSetVersionMappingService = null,
        IReleaseFileRepository? releaseFileRepository = null,
        IOptions<FeatureFlagsOptions>? featureFlags = null
        )
    {
        featureFlags ??= Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
        {
            EnableReplacementOfPublicApiDataSets = false
        });
        return new ReplacementPlanService(
            contentDbContext,
            statisticsDbContext,
            new FilterRepository(statisticsDbContext),
            new IndicatorRepository(statisticsDbContext),
            locationRepository ?? Mock.Of<ILocationRepository>(Strict),
            new FootnoteRepository(statisticsDbContext),
            dataSetVersionService ?? Mock.Of<IDataSetVersionService>(Strict),
            timePeriodService ?? Mock.Of<ITimePeriodService>(Strict),
            AlwaysTrueUserService().Object,
            dataSetVersionMappingService ?? Mock.Of<IDataSetVersionMappingService>(Strict),
            releaseFileRepository ?? Mock.Of<IReleaseFileRepository>(Strict),
            featureFlags
        );
    }
}

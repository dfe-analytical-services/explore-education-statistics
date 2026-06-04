#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using IndicatorMapping = GovUk.Education.ExploreEducationStatistics.Content.Model.IndicatorMapping;
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
        releaseFileRepository
            .Setup(mock =>
                mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, nonExistantFileId)
            )
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
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: nonExistantFileId
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetReplacementPlan_ReleaseNotFound()
    {
        var originalFile = new File { Type = FileType.Data, SubjectId = Guid.NewGuid() };

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
            var replacementPlanService = BuildReplacementPlanService(contentDbContext, statisticsDbContext);

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: Guid.NewGuid(),
                originalFileId: originalFile.Id
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetReplacementPlan_OriginalFileIsNotUsedByAnyDependentData()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataSetMappings.Add(
                new DataSetMapping
                {
                    OriginalDataFileId = originalFile.Id,
                    ReplacementDataFileId = replacementFile.Id,
                    IndicatorMappings = new Dictionary<Guid, IndicatorMapping>(),
                    LocationMappings = new Dictionary<Guid, LocationMapping>(),
                }
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

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
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile { ReleaseVersionId = releaseVersion.Id, FileId = originalFile.Id };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            FileId = replacementFile.Id,
        };

        var originalFilterItem = new FilterItem { Id = Guid.NewGuid(), Label = "Original Test filter item" };

        var replacementFilterItem = new FilterItem { Label = "Replacement Test filter item" };

        var originalFilterGroup = new FilterGroup
        {
            Label = "Original Default group",
            FilterItems = new List<FilterItem> { originalFilterItem },
        };

        var replacementFilterGroup = new FilterGroup
        {
            Label = "Replacement Default group",
            FilterItems = new List<FilterItem> { replacementFilterItem },
        };

        var originalFilter = new Filter
        {
            Label = "Original Test filter",
            Name = "original_test_filter",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalFilterGroup },
        };

        var replacementFilter = new Filter
        {
            Label = "Replacement Test filter",
            Name = "replacement_test_filter",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementFilterGroup },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Original Indicator",
            Name = "original_indicator",
        };

        var replacementIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Replacement Indicator",
            Name = "replacement_indicator",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Original Default group",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Replacement Default group",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england,
        };
        var observationForOriginalLocation = new Observation
        {
            SubjectId = originalReleaseSubject.SubjectId,
            Location = originalLocation,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] { originalFilterItem.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = ListOf(originalLocation.Id),
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var footnoteForSubject = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Subject",
            subject: originalReleaseSubject.Subject
        );

        var footnoteForFilter = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter",
            filterFootnotes: new List<FilterFootnote> { new() { Filter = originalFilter } }
        );

        var footnoteForFilterGroup = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter group",
            filterGroupFootnotes: new List<FilterGroupFootnote> { new() { FilterGroup = originalFilterGroup } }
        );

        var footnoteForFilterItem = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            filterItemFootnotes: new List<FilterItemFootnote> { new() { FilterItem = originalFilterItem } }
        );

        var footnoteForIndicator = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            indicatorFootnotes: new List<IndicatorFootnote> { new() { Indicator = originalIndicator } }
        );

        var dataSetMapping = new DataSetMapping
        {
            OriginalDataFileId = originalReleaseFile.FileId,
            ReplacementDataFileId = replacementReleaseFile.FileId,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                { originalIndicator.Id, CreateIndicatorMapping(originalIndicator, originalIndicatorGroup) },
            },
            UnmappedReplacementIndicators =
            [
                new UnmappedIndicator
                {
                    Id = replacementIndicator.Id,
                    ColumnName = replacementIndicator.Name,
                    Label = replacementIndicator.Label,
                    GroupId = replacementIndicatorGroup.Id,
                    GroupLabel = replacementIndicatorGroup.Label,
                },
            ],
            LocationMappings = new Dictionary<Guid, LocationMapping>
            {
                { originalLocation.Id, CreateLocationMapping(originalLocation) },
            },
            UnmappedReplacementLocations = [],
        };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataSetMappings.Add(dataSetMapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalFilter, replacementFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            statisticsDbContext.Footnote.AddRange(
                footnoteForFilter,
                footnoteForFilterGroup,
                footnoteForFilterItem,
                footnoteForIndicator,
                footnoteForSubject
            );
            statisticsDbContext.Location.AddRange(originalLocation);
            statisticsDbContext.Observation.AddRange(observationForOriginalLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

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
                .LocationAttributes.First();

            Assert.Equal(originalLocation.Id, dataBlockLocationPlan.Id);
            Assert.Equal(_england.GetCodeOrFallback(), dataBlockLocationPlan.Code);
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

            Assert.Equal(5, replacementPlan.Footnotes.Count());

            var footnoteForFilterPlan = replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilter.Id);

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

            var footnoteForFilterGroupPlan = replacementPlan.Footnotes.Single(plan =>
                plan.Id == footnoteForFilterGroup.Id
            );

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

            var footnoteForFilterItemPlan = replacementPlan.Footnotes.Single(plan =>
                plan.Id == footnoteForFilterItem.Id
            );

            Assert.Equal(footnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
            Assert.Empty(footnoteForFilterItemPlan.Filters);
            Assert.Empty(footnoteForFilterItemPlan.FilterGroups);
            Assert.Single(footnoteForFilterItemPlan.FilterItems);
            Assert.Empty(footnoteForFilterItemPlan.IndicatorGroups);

            var footnoteForFilterItemFilterItemPlan = footnoteForFilterItemPlan.FilterItems.First();

            Assert.Equal(originalFilterItem.Id, footnoteForFilterItemFilterItemPlan.Id);
            Assert.Equal(originalFilterItem.Label, footnoteForFilterItemFilterItemPlan.Label);
            Assert.Equal(originalFilterItem.FilterGroup.Filter.Id, footnoteForFilterItemFilterItemPlan.FilterId);
            Assert.Equal(originalFilterItem.FilterGroup.Filter.Label, footnoteForFilterItemFilterItemPlan.FilterLabel);
            Assert.Equal(originalFilterItem.FilterGroup.Id, footnoteForFilterItemFilterItemPlan.FilterGroupId);
            Assert.Equal(originalFilterItem.FilterGroup.Label, footnoteForFilterItemFilterItemPlan.FilterGroupLabel);
            Assert.Null(footnoteForFilterItemFilterItemPlan.Target);
            Assert.False(footnoteForFilterItemFilterItemPlan.Valid);

            Assert.False(footnoteForFilterItemPlan.Valid);

            var footnoteForIndicatorPlan = replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);

            Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);
            Assert.Empty(footnoteForIndicatorPlan.Filters);
            Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
            Assert.Empty(footnoteForIndicatorPlan.FilterItems);
            Assert.Single(footnoteForIndicatorPlan.IndicatorGroups);

            var footnoteForIndicatorIndicatorGroupPlan = footnoteForIndicatorPlan.IndicatorGroups.First();

            Assert.Equal(originalIndicator.IndicatorGroup.Id, footnoteForIndicatorIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicator.IndicatorGroup.Label, footnoteForIndicatorIndicatorGroupPlan.Value.Label);
            Assert.Single(footnoteForIndicatorIndicatorGroupPlan.Value.Indicators);
            Assert.False(footnoteForIndicatorIndicatorGroupPlan.Value.Valid);

            var footnoteForIndicatorIndicatorPlan = footnoteForIndicatorIndicatorGroupPlan.Value.Indicators.First();

            Assert.Equal(originalIndicator.Id, footnoteForIndicatorIndicatorPlan.Id);
            Assert.Equal(originalIndicator.Label, footnoteForIndicatorIndicatorPlan.Label);
            Assert.Equal(originalIndicator.Name, footnoteForIndicatorIndicatorPlan.Name);
            Assert.Null(footnoteForIndicatorIndicatorPlan.Target);
            Assert.False(footnoteForIndicatorIndicatorPlan.Valid);

            Assert.False(footnoteForIndicatorPlan.Valid);

            var footnoteForSubjectPlan = replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForSubject.Id);

            Assert.Equal(footnoteForSubject.Content, footnoteForSubjectPlan.Content);
            Assert.Empty(footnoteForSubjectPlan.Filters);
            Assert.Empty(footnoteForSubjectPlan.FilterGroups);
            Assert.Empty(footnoteForSubjectPlan.FilterItems);
            Assert.Empty(footnoteForSubjectPlan.IndicatorGroups);

            Assert.True(footnoteForSubjectPlan.Valid);

            var indicatorsMappingPlan = replacementPlan.Mapping.Indicators;
            Assert.Equivalent(new[] { originalIndicator.Id }, indicatorsMappingPlan.Mappings.Keys);
            Assert.Contains(
                new ReplacementPlanIndicatorMappingViewModel
                {
                    CandidateKey = null,
                    Type = nameof(MapStatus.Unset),
                    Source = new ReplacementPlanIndicatorViewModel
                    {
                        Id = originalIndicator.Id,
                        Name = originalIndicator.Name,
                        Label = originalIndicator.Label,
                    },
                },
                indicatorsMappingPlan.Mappings.Values
            );
            Assert.Equivalent(new[] { replacementIndicator.Id }, indicatorsMappingPlan.Candidates.Keys);
            Assert.Contains(
                new ReplacementPlanIndicatorViewModel
                {
                    Id = replacementIndicator.Id,
                    Name = replacementIndicator.Name,
                    Label = replacementIndicator.Label,
                },
                indicatorsMappingPlan.Candidates.Values
            );

            var locationsMappingPlan = replacementPlan.Mapping.Locations;
            Assert.Equivalent(new[] { originalLocation.Id }, locationsMappingPlan.Mappings.Keys);
            Assert.Contains(
                new ReplacementPlanLocationMappingViewModel
                {
                    CandidateKey = null,
                    Type = nameof(MapStatus.Unset),
                    Source = new ReplacementPlanLocationViewModel
                    {
                        Id = originalLocation.Id,
                        Name = originalLocation.ToLocationAttribute().Name!,
                        Code = originalLocation.ToLocationAttribute().Code!,
                    },
                },
                locationsMappingPlan.Mappings.Values
            );
            Assert.Empty(locationsMappingPlan.Candidates);

            Assert.False(replacementPlan.Valid);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_SelectedFilterItemsNoLongerExistButSomeDo_ReplacementInvalid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var originalDefaultFilterItem = new FilterItem { Id = Guid.NewGuid(), Label = "Test filter item" };

        var originalDefaultFilterItem2 = new FilterItem { Id = Guid.NewGuid(), Label = "Test filter item 2" };

        var replacementDefaultFilterItem = new FilterItem { Label = "Test filter item" };

        var originalDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalDefaultFilterItem, originalDefaultFilterItem2 },
        };

        var replacementDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementDefaultFilterItem },
        };

        var originalDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalDefaultFilterGroup },
        };

        var replacementDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementDefaultFilterGroup },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england,
        };
        var observationForLocation = new Observation
        {
            Id = Guid.NewGuid(),
            SubjectId = originalReleaseSubject.SubjectId,
            Location = location,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] { originalDefaultFilterItem.Id, originalDefaultFilterItem2.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = ListOf(location.Id),
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataSetMappings.Add(
                new DataSetMapping
                {
                    OriginalDataFileId = originalFile.Id,
                    ReplacementDataFileId = replacementFile.Id,
                    IndicatorMappings = new Dictionary<Guid, IndicatorMapping>(),
                    LocationMappings = new Dictionary<Guid, LocationMapping>(),
                }
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalDefaultFilter, replacementDefaultFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(location);
            statisticsDbContext.Observation.AddRange(observationForLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

            var replacementPlan = result.AssertRight();
            Assert.False(replacementPlan.Valid);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.False(dataBlockPlan.Valid);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_AllOriginalFilterItemsNoLongerExist_ReplacementInvalid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var originalDefaultFilterItem = new FilterItem { Id = Guid.NewGuid(), Label = "Test filter item" };

        var replacementDefaultFilterItem = new FilterItem { Label = "Test filter item - changing!" };

        var originalDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalDefaultFilterItem },
        };

        var replacementDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementDefaultFilterItem },
        };

        var originalDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalDefaultFilterGroup },
        };

        var replacementDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementDefaultFilterGroup },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england,
        };
        var observationForLocation = new Observation
        {
            Id = Guid.NewGuid(),
            SubjectId = originalReleaseSubject.SubjectId,
            Location = location,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] { originalDefaultFilterItem.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = ListOf(location.Id),
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataSetMappings.Add(
                new DataSetMapping
                {
                    OriginalDataFileId = originalReleaseFile.FileId,
                    ReplacementDataFileId = replacementReleaseFile.FileId,
                    IndicatorMappings = new Dictionary<Guid, IndicatorMapping>(),
                    LocationMappings = new Dictionary<Guid, LocationMapping>(),
                }
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(originalDefaultFilter, replacementDefaultFilter);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(location);
            statisticsDbContext.Observation.AddRange(observationForLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

            var replacementPlan = result.AssertRight();
            Assert.False(replacementPlan.Valid);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.False(dataBlockPlan.Valid);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_NewFiltersIntroduced_ReplacementInvalid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var originalDefaultFilterItem = new FilterItem { Id = Guid.NewGuid(), Label = "Test filter item" };

        var replacementDefaultFilterItem = new FilterItem { Label = "Test filter item" };

        var replacementNewlyIntroducedFiltersFilterItem = new FilterItem
        {
            Label = "Filter item for newly introduced Filter",
        };

        var originalDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalDefaultFilterItem },
        };

        var replacementDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementDefaultFilterItem },
        };

        var replacementNewlyIntroducedFilterGroup = new FilterGroup
        {
            Label = "Newly introduced filter group",
            FilterItems = new List<FilterItem> { replacementNewlyIntroducedFiltersFilterItem },
        };

        var originalDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalDefaultFilterGroup },
        };

        var replacementDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementDefaultFilterGroup },
        };

        var replacementNewlyIntroducedFilter = new Filter
        {
            Label = "Newly introduced filter",
            Name = "newly_introduced_filter",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementNewlyIntroducedFilterGroup },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england,
        };
        var observationForLocation = new Observation
        {
            Id = Guid.NewGuid(),
            SubjectId = originalReleaseSubject.SubjectId,
            Location = location,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = new[] { originalDefaultFilterItem.Id },
                Indicators = new[] { originalIndicator.Id },
                LocationIds = ListOf(location.Id),
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataSetMappings.Add(
                new DataSetMapping
                {
                    OriginalDataFileId = originalReleaseFile.FileId,
                    ReplacementDataFileId = replacementReleaseFile.FileId,
                    IndicatorMappings = new Dictionary<Guid, IndicatorMapping>(),
                    LocationMappings = new Dictionary<Guid, LocationMapping>(),
                }
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(
                originalDefaultFilter,
                replacementDefaultFilter,
                replacementNewlyIntroducedFilter
            );
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(location);
            statisticsDbContext.Observation.AddRange(observationForLocation);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

            var replacementPlan = result.AssertRight();
            Assert.False(replacementPlan.Valid);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.False(dataBlockPlan.Valid);
        }
    }

    /// <summary>
    /// Please note this test will reduce in the number of cases when the feature flag has been removed from it.
    /// </summary>
    /// <param name="dataSetVersionStatus">The data set version status of the replaced file's API data set version</param>
    /// <param name="majorVersionUpdate">Whether the user has uploaded a file that results in a major version update</param>
    /// <param name="expectedValidValue">The expected value for the scenario set up</param>
    [Theory]
    // When user has uploaded major version data set
    [InlineData(DataSetVersionStatus.Published, true, false)]
    [InlineData(DataSetVersionStatus.Mapping, true, false)]
    [InlineData(DataSetVersionStatus.Draft, true, false)]
    // When user has uploaded minor version
    [InlineData(DataSetVersionStatus.Published, false, false)]
    [InlineData(DataSetVersionStatus.Mapping, false, false)]
    [InlineData(DataSetVersionStatus.Draft, false, true)]
    // When API data set version status is not appropriate to be replaced
    [InlineData(DataSetVersionStatus.Processing, false, false)]
    [InlineData(DataSetVersionStatus.Failed, false, false)]
    [InlineData(DataSetVersionStatus.Deprecated, false, false)]
    [InlineData(DataSetVersionStatus.Withdrawn, false, false)]
    [InlineData(DataSetVersionStatus.Cancelled, false, false)]
    public async Task GetReplacementPlan_FileIsLinkedToPublicApiDataSet_ReplacementValidated(
        DataSetVersionStatus dataSetVersionStatus,
        bool majorVersionUpdate,
        bool expectedValidValue
    )
    {
        DataSet dataSet = _fixture.DefaultDataSet();

        DataSetVersion dataSetVersion = _fixture
            .DefaultDataSetVersion()
            .WithVersionNumber(major: 1, minor: 1, patch: 1)
            .WithStatus(dataSetVersionStatus)
            .WithDataSet(dataSet);

        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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

        var (originalReleaseFile, replacementReleaseFile) = _fixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .ForIndex(
                0,
                rv =>
                    rv.SetFile(originalFile)
                        .SetPublicApiDataSetId(dataSet.Id)
                        .SetPublicApiDataSetVersion(dataSetVersion.SemVersion())
            )
            .ForIndex(
                1,
                rv =>
                    rv.SetFile(replacementFile)
                        .SetPublicApiDataSetId(dataSet.Id)
                        .SetPublicApiDataSetVersion(dataSetVersion.SemVersion())
            )
            .GenerateTuple2();

        var dataSetVersionService = new Mock<IDataSetVersionService>(Strict);
        dataSetVersionService
            .Setup(mock =>
                mock.GetDataSetVersion(
                    originalReleaseFile.PublicApiDataSetId!.Value,
                    originalReleaseFile.PublicApiDataSetVersion!,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(dataSetVersion);

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var mappingStatus = new MappingStatusViewModel
        {
            LocationsComplete = majorVersionUpdate,
            FiltersComplete = majorVersionUpdate,
            IndicatorsComplete = majorVersionUpdate,
            HasDeletionChanges = majorVersionUpdate,
            FiltersHaveMajorChange = majorVersionUpdate,
            LocationsHaveMajorChange = majorVersionUpdate,
            IndicatorsHaveMajorChange = majorVersionUpdate,
        };
        var dataSetVersionMappingService = new Mock<IDataSetVersionMappingService>(Strict);
        dataSetVersionMappingService
            .Setup(service => service.GetMappingStatus(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mappingStatus);

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataSetMappings.Add(
                new DataSetMapping
                {
                    OriginalDataFileId = originalReleaseFile.FileId,
                    ReplacementDataFileId = replacementReleaseFile.FileId,
                    IndicatorMappings = new Dictionary<Guid, IndicatorMapping>(),
                    LocationMappings = new Dictionary<Guid, LocationMapping>(),
                }
            );
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
                releaseFileRepository: releaseFileRepository.Object,
                apiDataSetVersionMappingService: dataSetVersionMappingService.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(dataSetVersionService, timePeriodService, releaseFileRepository);

            var replacementPlan = result.AssertRight();

            Assert.NotNull(replacementPlan.ApiDataSetVersionPlan);
            Assert.Equal(dataSet.Id, replacementPlan.ApiDataSetVersionPlan.DataSetId);
            Assert.Equal(dataSet.Title, replacementPlan.ApiDataSetVersionPlan.DataSetTitle);
            Assert.Equal(dataSetVersion.Id, replacementPlan.ApiDataSetVersionPlan.Id);
            Assert.Equal(dataSetVersion.PublicVersion, replacementPlan.ApiDataSetVersionPlan.Version);

            Assert.Equal(mappingStatus, replacementPlan.ApiDataSetVersionPlan.MappingStatus);

            Assert.Equal(dataSetVersion.Status, replacementPlan.ApiDataSetVersionPlan.Status);

            Assert.Equal(replacementPlan.ApiDataSetVersionPlan.Valid, expectedValidValue);
            Assert.Equal(replacementPlan.Valid, expectedValidValue);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_AllReplacementDataPresent_ReplacementValid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile { ReleaseVersionId = releaseVersion.Id, FileId = originalFile.Id };

        var replacementReleaseFile = new ReleaseFile
        {
            ReleaseVersionId = releaseVersion.Id,
            FileId = replacementFile.Id,
        };

        var originalDefaultFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Test filter item - not changing",
        };

        var originalPrimarySchoolsFilterItem = new FilterItem { Id = Guid.NewGuid(), Label = "Primary schools" };

        var originalPrimaryAndSecondarySchoolsFilterItem = new FilterItem
        {
            Id = Guid.NewGuid(),
            Label = "Primary and secondary schools",
        };

        var replacementDefaultFilterItem = new FilterItem { Label = "Test filter item - not changing" };

        var replacementPrimarySchoolsFilterItem = new FilterItem { Label = "Primary schools" };

        var replacementPrimaryAndSecondarySchoolsFilterItem = new FilterItem
        {
            Label = "Primary and secondary schools",
        };

        var originalDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { originalDefaultFilterItem },
        };

        var originalIndividualSchoolTypeFilterGroup = new FilterGroup
        {
            Label = "Individual",
            FilterItems = new List<FilterItem> { originalPrimarySchoolsFilterItem },
        };

        var originalCombinedSchoolTypeFilterGroup = new FilterGroup
        {
            Label = "Combined",
            FilterItems = new List<FilterItem> { originalPrimaryAndSecondarySchoolsFilterItem },
        };

        var replacementDefaultFilterGroup = new FilterGroup
        {
            Label = "Default group - not changing",
            FilterItems = new List<FilterItem> { replacementDefaultFilterItem },
        };

        var replacementIndividualSchoolTypeFilterGroup = new FilterGroup
        {
            Label = "Individual",
            FilterItems = new List<FilterItem> { replacementPrimarySchoolsFilterItem },
        };

        var replacementCombinedSchoolTypeFilterGroup = new FilterGroup
        {
            Label = "Combined",
            FilterItems = new List<FilterItem> { replacementPrimaryAndSecondarySchoolsFilterItem },
        };

        var originalDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = originalReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { originalDefaultFilterGroup },
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
            },
        };

        var replacementDefaultFilter = new Filter
        {
            Label = "Test filter 1 - not changing",
            Name = "test_filter_1_not_changing",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup> { replacementDefaultFilterGroup },
        };

        var replacementSchoolTypeFilter = new Filter
        {
            Label = "School type",
            Name = "school_type",
            Subject = replacementReleaseSubject.Subject,
            FilterGroups = new List<FilterGroup>
            {
                replacementIndividualSchoolTypeFilterGroup,
                replacementCombinedSchoolTypeFilterGroup,
            },
        };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var replacementIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator - not changing",
            Name = "indicator_not_changing",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var originalLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england,
        };
        var replacementLocation = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england with
            {
                Name = "England updated", // should still automap even though the name has changed
            },
        };

        var observationForOriginalLocation = new Observation
        {
            Id = Guid.NewGuid(),
            SubjectId = originalReleaseSubject.SubjectId,
            Location = originalLocation,
        };
        var observationForReplacementLocation = new Observation
        {
            Id = Guid.NewGuid(),
            SubjectId = replacementReleaseSubject.SubjectId,
            Location = replacementLocation,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
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
                Indicators = new[] { originalIndicator.Id },
                LocationIds = ListOf(originalLocation.Id),
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var footnoteForFilter = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter",
            filterFootnotes: new List<FilterFootnote> { new() { Filter = originalDefaultFilter } }
        );

        var footnoteForFilterGroup = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter group",
            filterGroupFootnotes: new List<FilterGroupFootnote> { new() { FilterGroup = originalDefaultFilterGroup } }
        );

        var footnoteForFilterItem = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            filterItemFootnotes: new List<FilterItemFootnote> { new() { FilterItem = originalDefaultFilterItem } }
        );

        var footnoteForIndicator = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            indicatorFootnotes: new List<IndicatorFootnote> { new() { Indicator = originalIndicator } }
        );

        var footnoteForSubject = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Subject",
            subject: originalReleaseSubject.Subject
        );

        var dataSetMapping = new DataSetMapping
        {
            OriginalDataFileId = originalReleaseFile.FileId,
            ReplacementDataFileId = replacementReleaseFile.FileId,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    originalIndicator.Id,
                    CreateIndicatorMapping(
                        originalIndicator,
                        originalIndicatorGroup,
                        replacementIndicator,
                        replacementIndicatorGroup,
                        MapStatus.AutoSet
                    )
                },
            },
            LocationMappings = new Dictionary<Guid, LocationMapping>
            {
                {
                    originalLocation.Id,
                    CreateLocationMapping(originalLocation, replacementLocation, MapStatus.AutoSet)
                },
            },
        };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.Files.AddRange(originalFile, replacementFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataSetMappings.AddRange(dataSetMapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.Filter.AddRange(
                originalDefaultFilter,
                originalSchoolTypeFilter,
                replacementDefaultFilter,
                replacementSchoolTypeFilter
            );
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            statisticsDbContext.Location.AddRange(originalLocation, replacementLocation);
            statisticsDbContext.Observation.AddRange(observationForOriginalLocation, observationForReplacementLocation);
            statisticsDbContext.Footnote.AddRange(
                footnoteForFilter,
                footnoteForFilterGroup,
                footnoteForFilterItem,
                footnoteForIndicator,
                footnoteForSubject
            );
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

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

            var dataBlockDefaultFilterPlan = dataBlockPlan.Filters.First(f => f.Key.Equals(originalDefaultFilter.Id));

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

            var dataBlockSchoolTypeFilterPlan = dataBlockPlan.Filters.First(f =>
                f.Key.Equals(originalSchoolTypeFilter.Id)
            );

            Assert.Equal(originalSchoolTypeFilter.Id, dataBlockSchoolTypeFilterPlan.Value.Id);
            Assert.Equal(originalSchoolTypeFilter.Label, dataBlockSchoolTypeFilterPlan.Value.Label);
            Assert.Equal(originalSchoolTypeFilter.Name, dataBlockSchoolTypeFilterPlan.Value.Name);
            Assert.True(dataBlockSchoolTypeFilterPlan.Value.Valid);

            Assert.Equal(2, dataBlockSchoolTypeFilterPlan.Value.Groups.Count);

            var dataBlockIndividualSchoolTypeFilterGroupPlan = dataBlockSchoolTypeFilterPlan.Value.Groups.First(g =>
                g.Key == originalIndividualSchoolTypeFilterGroup.Id
            );

            Assert.Equal(
                originalIndividualSchoolTypeFilterGroup.Id,
                dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Id
            );
            Assert.Equal(
                originalIndividualSchoolTypeFilterGroup.Label,
                dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Label
            );
            Assert.Single(dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Filters);
            Assert.True(dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Valid);

            var dataBlockCombinedSchoolTypeFilterGroupPlan = dataBlockSchoolTypeFilterPlan.Value.Groups.First(g =>
                g.Key == originalCombinedSchoolTypeFilterGroup.Id
            );

            Assert.Equal(originalCombinedSchoolTypeFilterGroup.Id, dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Id);
            Assert.Equal(
                originalCombinedSchoolTypeFilterGroup.Label,
                dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Label
            );
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
            Assert.True(dataBlockPlan.Locations.ContainsKey(nameof(GeographicLevel.Country)));
            Assert.Single(dataBlockPlan.Locations[nameof(GeographicLevel.Country)].LocationAttributes);
            Assert.True(dataBlockPlan.Locations[nameof(GeographicLevel.Country)].Valid);

            var dataBlockLocationPlan = dataBlockPlan
                .Locations[nameof(GeographicLevel.Country)]
                .LocationAttributes.First();

            Assert.Equal(originalLocation.Id, dataBlockLocationPlan.Id);
            Assert.Equal(_england.GetCodeOrFallback(), dataBlockLocationPlan.Code);
            Assert.Equal(_england.Name, dataBlockLocationPlan.Label);
            Assert.Equal(replacementLocation.Id, dataBlockLocationPlan.Target);
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

            Assert.Equal(5, replacementPlan.Footnotes.Count());

            var footnoteForFilterPlan = replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilter.Id);

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

            var footnoteForFilterGroupPlan = replacementPlan.Footnotes.Single(plan =>
                plan.Id == footnoteForFilterGroup.Id
            );

            Assert.Equal(footnoteForFilterGroup.Content, footnoteForFilterGroupPlan.Content);
            Assert.Empty(footnoteForFilterGroupPlan.Filters);
            Assert.Single(footnoteForFilterGroupPlan.FilterGroups);
            Assert.Empty(footnoteForFilterGroupPlan.FilterItems);
            Assert.Empty(footnoteForFilterGroupPlan.IndicatorGroups);

            var footnoteForFilterGroupFilterGroupPlan = footnoteForFilterGroupPlan.FilterGroups.First();

            Assert.Equal(originalDefaultFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Id);
            Assert.Equal(originalDefaultFilterGroup.Label, footnoteForFilterGroupFilterGroupPlan.Label);
            Assert.Equal(originalDefaultFilterGroup.Filter.Id, footnoteForFilterGroupFilterGroupPlan.FilterId);
            Assert.Equal(originalDefaultFilterGroup.Filter.Label, footnoteForFilterGroupFilterGroupPlan.FilterLabel);
            Assert.Equal(replacementDefaultFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Target);
            Assert.True(footnoteForFilterGroupFilterGroupPlan.Valid);

            Assert.True(footnoteForFilterGroupPlan.Valid);

            var footnoteForFilterItemPlan = replacementPlan.Footnotes.Single(plan =>
                plan.Id == footnoteForFilterItem.Id
            );

            Assert.Equal(footnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
            Assert.Empty(footnoteForFilterItemPlan.Filters);
            Assert.Empty(footnoteForFilterItemPlan.FilterGroups);
            Assert.Single(footnoteForFilterItemPlan.FilterItems);
            Assert.Empty(footnoteForFilterItemPlan.IndicatorGroups);

            var footnoteForFilterItemFilterItemPlan = footnoteForFilterItemPlan.FilterItems.First();

            Assert.Equal(originalDefaultFilterItem.Id, footnoteForFilterItemFilterItemPlan.Id);
            Assert.Equal(originalDefaultFilterItem.Label, footnoteForFilterItemFilterItemPlan.Label);
            Assert.Equal(originalDefaultFilterItem.FilterGroup.Filter.Id, footnoteForFilterItemFilterItemPlan.FilterId);
            Assert.Equal(
                originalDefaultFilterItem.FilterGroup.Filter.Label,
                footnoteForFilterItemFilterItemPlan.FilterLabel
            );
            Assert.Equal(originalDefaultFilterItem.FilterGroup.Id, footnoteForFilterItemFilterItemPlan.FilterGroupId);
            Assert.Equal(
                originalDefaultFilterItem.FilterGroup.Label,
                footnoteForFilterItemFilterItemPlan.FilterGroupLabel
            );
            Assert.Equal(replacementDefaultFilterItem.Id, footnoteForFilterItemFilterItemPlan.Target);
            Assert.True(footnoteForFilterItemFilterItemPlan.Valid);

            Assert.True(footnoteForFilterItemPlan.Valid);

            var footnoteForIndicatorPlan = replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);
            Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);

            Assert.Empty(footnoteForIndicatorPlan.Filters);
            Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
            Assert.Empty(footnoteForIndicatorPlan.FilterItems);
            Assert.Single(footnoteForIndicatorPlan.IndicatorGroups);

            var footnoteForIndicatorIndicatorGroupPlan = footnoteForIndicatorPlan.IndicatorGroups.First();

            Assert.Equal(originalIndicator.IndicatorGroup.Id, footnoteForIndicatorIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicator.IndicatorGroup.Label, footnoteForIndicatorIndicatorGroupPlan.Value.Label);
            Assert.Single(footnoteForIndicatorIndicatorGroupPlan.Value.Indicators);
            Assert.True(footnoteForIndicatorIndicatorGroupPlan.Value.Valid);

            var footnoteForIndicatorIndicatorPlan = footnoteForIndicatorIndicatorGroupPlan.Value.Indicators.First();

            Assert.Equal(originalIndicator.Id, footnoteForIndicatorIndicatorPlan.Id);
            Assert.Equal(originalIndicator.Label, footnoteForIndicatorIndicatorPlan.Label);
            Assert.Equal(originalIndicator.Name, footnoteForIndicatorIndicatorPlan.Name);
            Assert.Equal(replacementIndicator.Id, footnoteForIndicatorIndicatorPlan.Target);
            Assert.True(footnoteForIndicatorIndicatorPlan.Valid);

            Assert.True(footnoteForIndicatorPlan.Valid);

            var footnoteForSubjectPlan = replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForSubject.Id);
            Assert.Equal(footnoteForSubject.Content, footnoteForSubjectPlan.Content);
            Assert.Empty(footnoteForSubjectPlan.Filters);
            Assert.Empty(footnoteForSubjectPlan.FilterGroups);
            Assert.Empty(footnoteForSubjectPlan.FilterItems);
            Assert.Empty(footnoteForSubjectPlan.IndicatorGroups);

            Assert.True(footnoteForSubjectPlan.Valid);

            Assert.Null(replacementPlan.ApiDataSetVersionPlan);

            var indicatorsMappingPlan = replacementPlan.Mapping.Indicators;

            Assert.Equivalent(new[] { originalIndicator.Id }, indicatorsMappingPlan.Mappings.Keys);
            var indicatorMapping = Assert.Single(indicatorsMappingPlan.Mappings.Values);
            Assert.Equal(
                new ReplacementPlanIndicatorMappingViewModel
                {
                    CandidateKey = replacementIndicator.Id,
                    Type = nameof(MapStatus.AutoSet),
                    Source = new ReplacementPlanIndicatorViewModel
                    {
                        Id = originalIndicator.Id,
                        Label = originalIndicator.Label,
                        Name = originalIndicator.Name,
                    },
                },
                indicatorMapping
            );

            Assert.Equivalent(new[] { replacementIndicator.Id }, indicatorsMappingPlan.Candidates.Keys);
            var indicatorCandidate = Assert.Single(indicatorsMappingPlan.Candidates.Values);
            Assert.Equal(
                new ReplacementPlanIndicatorViewModel
                {
                    Id = replacementIndicator.Id,
                    Name = replacementIndicator.Name,
                    Label = replacementIndicator.Label,
                },
                indicatorCandidate
            );

            var locationsMappingPlan = replacementPlan.Mapping.Locations;

            Assert.Equivalent(new[] { originalLocation.Id }, locationsMappingPlan.Mappings.Keys);
            var locationMapping = Assert.Single(locationsMappingPlan.Mappings.Values);
            Assert.Equal(
                new ReplacementPlanLocationMappingViewModel
                {
                    CandidateKey = replacementLocation.Id,
                    Type = nameof(MapStatus.AutoSet),
                    Source = new ReplacementPlanLocationViewModel
                    {
                        Id = originalLocation.Id,
                        Name = originalLocation.ToLocationAttribute().Name!,
                        Code = originalLocation.ToLocationAttribute().Code!,
                    },
                },
                locationMapping
            );

            Assert.Equivalent(new[] { replacementLocation.Id }, locationsMappingPlan.Candidates.Keys);
            var locationCandidate = Assert.Single(locationsMappingPlan.Candidates.Values);
            Assert.Equal(
                new ReplacementPlanLocationViewModel
                {
                    Id = replacementLocation.Id,
                    Name = replacementLocation.ToLocationAttribute().Name!,
                    Code = replacementLocation.ToLocationAttribute().Code!,
                },
                locationCandidate
            );

            Assert.True(replacementPlan.Valid);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_CustomMapping_IndicatorMappedThatAppearsInDataBlockAndFootnote_ReplacementValid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var originalIndicatorA = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A",
            Name = "indicator_a",
        };

        var replacementIndicatorAUpdated = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A updated",
            Name = "indicator_a_updated",
        };

        var originalIndicatorToBeRemoved = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator to be removed",
            Name = "indicator_to_be_removed",
        };

        var replacementNewIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "New indicator",
            Name = "new_indicator",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicatorA, originalIndicatorToBeRemoved },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicatorAUpdated, replacementNewIndicator },
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = [],
                Indicators = [originalIndicatorA.Id],
                LocationIds = ListOf(location.Id),
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var footnoteForIndicator = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            indicatorFootnotes: new List<IndicatorFootnote> { new() { Indicator = originalIndicatorA } }
        );

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var dataSetMapping = new DataSetMapping
        {
            OriginalDataFileId = originalFile.Id,
            ReplacementDataFileId = replacementFile.Id,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    originalIndicatorA.Id,
                    CreateIndicatorMapping(
                        originalIndicatorA,
                        originalIndicatorGroup,
                        replacementIndicatorAUpdated,
                        replacementIndicatorGroup,
                        MapStatus.ManuallySet
                    )
                },
                {
                    originalIndicatorToBeRemoved.Id,
                    CreateIndicatorMapping(originalIndicatorToBeRemoved, originalIndicatorGroup)
                },
            },
            UnmappedReplacementIndicators =
            [
                new UnmappedIndicator
                {
                    Id = replacementNewIndicator.Id,
                    Label = replacementNewIndicator.Label,
                    ColumnName = replacementNewIndicator.Name,
                    GroupId = replacementIndicatorGroup.Id,
                    GroupLabel = replacementIndicatorGroup.Label,
                },
            ],
            LocationMappings = new Dictionary<Guid, LocationMapping>
            {
                { location.Id, CreateLocationMapping(location, location, MapStatus.AutoSet) },
            },
            UnmappedReplacementLocations = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataSetMappings.Add(dataSetMapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            statisticsDbContext.Footnote.AddRange(footnoteForIndicator);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

            var replacementPlan = result.AssertRight();

            Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
            Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

            var dataBlockIndicatorGroupPlan = Assert.Single(dataBlockPlan.IndicatorGroups);

            Assert.Equal(originalIndicatorGroup.Id, dataBlockIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicatorGroup.Label, dataBlockIndicatorGroupPlan.Value.Label);
            Assert.Single(dataBlockIndicatorGroupPlan.Value.Indicators);
            Assert.True(dataBlockIndicatorGroupPlan.Value.Valid);

            var dataBlockIndicatorPlan = dataBlockIndicatorGroupPlan.Value.Indicators.First();

            Assert.Equal(originalIndicatorA.Id, dataBlockIndicatorPlan.Id);
            Assert.Equal(originalIndicatorA.Label, dataBlockIndicatorPlan.Label);
            Assert.Equal(originalIndicatorA.Name, dataBlockIndicatorPlan.Name);
            Assert.Equal(replacementIndicatorAUpdated.Id, dataBlockIndicatorPlan.Target);
            Assert.True(dataBlockIndicatorPlan.Valid);

            var footnoteForIndicatorPlan = replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);
            Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);

            Assert.Empty(footnoteForIndicatorPlan.Filters);
            Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
            Assert.Empty(footnoteForIndicatorPlan.FilterItems);
            Assert.Single(footnoteForIndicatorPlan.IndicatorGroups);

            var footnoteForIndicatorIndicatorGroupPlan = footnoteForIndicatorPlan.IndicatorGroups.First();

            Assert.Equal(originalIndicatorA.IndicatorGroup.Id, footnoteForIndicatorIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicatorA.IndicatorGroup.Label, footnoteForIndicatorIndicatorGroupPlan.Value.Label);
            Assert.Single(footnoteForIndicatorIndicatorGroupPlan.Value.Indicators);
            Assert.True(footnoteForIndicatorIndicatorGroupPlan.Value.Valid);

            var footnoteForIndicatorIndicatorPlan = footnoteForIndicatorIndicatorGroupPlan.Value.Indicators.First();

            Assert.Equal(originalIndicatorA.Id, footnoteForIndicatorIndicatorPlan.Id);
            Assert.Equal(originalIndicatorA.Label, footnoteForIndicatorIndicatorPlan.Label);
            Assert.Equal(originalIndicatorA.Name, footnoteForIndicatorIndicatorPlan.Name);
            Assert.Equal(replacementIndicatorAUpdated.Id, footnoteForIndicatorIndicatorPlan.Target);
            Assert.True(footnoteForIndicatorIndicatorPlan.Valid);

            Assert.True(footnoteForIndicatorPlan.Valid);

            var indicatorMappingPlan = replacementPlan.Mapping.Indicators;

            Assert.Equal(2, indicatorMappingPlan.Mappings.Values.Count);
            Assert.Equivalent(
                new[] { originalIndicatorA.Id, originalIndicatorToBeRemoved.Id },
                indicatorMappingPlan.Mappings.Keys
            );
            Assert.Contains(
                new ReplacementPlanIndicatorMappingViewModel
                {
                    CandidateKey = replacementIndicatorAUpdated.Id,
                    Source = new ReplacementPlanIndicatorViewModel
                    {
                        Id = originalIndicatorA.Id,
                        Label = originalIndicatorA.Label,
                        Name = originalIndicatorA.Name,
                    },
                    Type = nameof(MapStatus.ManuallySet),
                },
                indicatorMappingPlan.Mappings.Values
            );
            Assert.Contains(
                new ReplacementPlanIndicatorMappingViewModel
                {
                    CandidateKey = null,
                    Source = new ReplacementPlanIndicatorViewModel
                    {
                        Id = originalIndicatorToBeRemoved.Id,
                        Label = originalIndicatorToBeRemoved.Label,
                        Name = originalIndicatorToBeRemoved.Name,
                    },
                    Type = nameof(MapStatus.Unset),
                },
                indicatorMappingPlan.Mappings.Values
            );

            Assert.Equal(2, indicatorMappingPlan.Candidates.Count);
            Assert.Equivalent(
                new[] { replacementIndicatorAUpdated.Id, replacementNewIndicator.Id },
                indicatorMappingPlan.Candidates.Keys
            );
            Assert.Contains(
                new ReplacementPlanIndicatorViewModel
                {
                    Id = replacementIndicatorAUpdated.Id,
                    Label = replacementIndicatorAUpdated.Label,
                    Name = replacementIndicatorAUpdated.Name,
                },
                indicatorMappingPlan.Candidates.Values
            );
            Assert.Contains(
                new ReplacementPlanIndicatorViewModel
                {
                    Id = replacementNewIndicator.Id,
                    Label = replacementNewIndicator.Label,
                    Name = replacementNewIndicator.Name,
                },
                indicatorMappingPlan.Candidates.Values
            );

            Assert.True(replacementPlan.Valid);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_CustomMapping_IndicatorUnmappedThatIsUsedInDataBlockAndFootnote_ReplacementInvalid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var originalIndicatorA = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A",
            Name = "indicator_a",
        };

        var replacementIndicatorAUpdated = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A updated",
            Name = "indicator_a_updated",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicatorA },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicatorAUpdated },
        };

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england,
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = [],
                Indicators = [originalIndicatorA.Id],
                LocationIds = ListOf(location.Id),
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var footnoteForIndicator = CreateFootnote(
            statsReleaseVersion,
            "Test footnote for Filter item",
            indicatorFootnotes: new List<IndicatorFootnote> { new() { Indicator = originalIndicatorA } }
        );

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var dataSetMapping = new DataSetMapping
        {
            OriginalDataFileId = originalFile.Id,
            ReplacementDataFileId = replacementFile.Id,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    originalIndicatorA.Id,
                    CreateIndicatorMapping(originalIndicatorA, originalIndicatorGroup) // not mapped to anything despite being included in data block/footnote, so plan should be invalid
                },
            },
            UnmappedReplacementIndicators =
            [
                new UnmappedIndicator
                {
                    Id = replacementIndicatorAUpdated.Id,
                    Label = replacementIndicatorAUpdated.Label,
                    ColumnName = replacementIndicatorAUpdated.Name,
                    GroupId = replacementIndicatorGroup.Id,
                    GroupLabel = replacementIndicatorGroup.Label,
                },
            ],
            LocationMappings = new Dictionary<Guid, LocationMapping>
            {
                { location.Id, CreateLocationMapping(location, location, MapStatus.AutoSet) },
            },
            UnmappedReplacementLocations = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataSetMappings.Add(dataSetMapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            statisticsDbContext.Footnote.AddRange(footnoteForIndicator);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

            var replacementPlan = result.AssertRight();

            Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
            Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

            Assert.Single(dataBlockPlan.IndicatorGroups);

            var dataBlockIndicatorGroupPlan = dataBlockPlan.IndicatorGroups.First();

            Assert.Equal(originalIndicatorA.IndicatorGroup.Id, dataBlockIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicatorA.IndicatorGroup.Label, dataBlockIndicatorGroupPlan.Value.Label);
            Assert.Single(dataBlockIndicatorGroupPlan.Value.Indicators);
            Assert.False(dataBlockIndicatorGroupPlan.Value.Valid);

            var dataBlockIndicatorPlan = dataBlockIndicatorGroupPlan.Value.Indicators.First();

            Assert.Equal(originalIndicatorA.Id, dataBlockIndicatorPlan.Id);
            Assert.Equal(originalIndicatorA.Label, dataBlockIndicatorPlan.Label);
            Assert.Equal(originalIndicatorA.Name, dataBlockIndicatorPlan.Name);
            Assert.Null(dataBlockIndicatorPlan.Target);
            Assert.False(dataBlockIndicatorPlan.Valid);

            var footnoteForIndicatorPlan = replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);
            Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);

            Assert.Empty(footnoteForIndicatorPlan.Filters);
            Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
            Assert.Empty(footnoteForIndicatorPlan.FilterItems);
            Assert.Single(footnoteForIndicatorPlan.IndicatorGroups);

            var footnoteForIndicatorIndicatorGroupPlan = footnoteForIndicatorPlan.IndicatorGroups.First();

            Assert.Equal(originalIndicatorA.IndicatorGroup.Id, footnoteForIndicatorIndicatorGroupPlan.Key);
            Assert.Equal(originalIndicatorA.IndicatorGroup.Label, footnoteForIndicatorIndicatorGroupPlan.Value.Label);
            Assert.Single(footnoteForIndicatorIndicatorGroupPlan.Value.Indicators);
            Assert.False(footnoteForIndicatorIndicatorGroupPlan.Value.Valid);

            var footnoteForIndicatorIndicatorPlan = footnoteForIndicatorIndicatorGroupPlan.Value.Indicators.First();

            Assert.Equal(originalIndicatorA.Id, footnoteForIndicatorIndicatorPlan.Id);
            Assert.Equal(originalIndicatorA.Label, footnoteForIndicatorIndicatorPlan.Label);
            Assert.Equal(originalIndicatorA.Name, footnoteForIndicatorIndicatorPlan.Name);
            Assert.Null(footnoteForIndicatorIndicatorPlan.Target);
            Assert.False(footnoteForIndicatorIndicatorPlan.Valid);

            Assert.False(footnoteForIndicatorPlan.Valid);

            var indicatorMappingPlan = replacementPlan.Mapping.Indicators;
            Assert.Equivalent(new[] { originalIndicatorA.Id }, indicatorMappingPlan.Mappings.Keys); // @MarkFix add this test elsewhere
            var indicatorMapping = Assert.Single(indicatorMappingPlan.Mappings.Values);
            Assert.Equal(
                new ReplacementPlanIndicatorMappingViewModel
                {
                    CandidateKey = null,
                    Type = nameof(MapStatus.Unset),
                    Source = new ReplacementPlanIndicatorViewModel
                    {
                        Id = originalIndicatorA.Id,
                        Label = originalIndicatorA.Label,
                        Name = originalIndicatorA.Name,
                    },
                },
                indicatorMapping
            );
            Assert.Equivalent(new[] { replacementIndicatorAUpdated.Id }, indicatorMappingPlan.Candidates.Keys);
            var indicatorCandidate = Assert.Single(indicatorMappingPlan.Candidates.Values);
            Assert.Equal(
                new ReplacementPlanIndicatorViewModel
                {
                    Id = replacementIndicatorAUpdated.Id,
                    Label = replacementIndicatorAUpdated.Label,
                    Name = replacementIndicatorAUpdated.Name,
                },
                indicatorCandidate
            );

            Assert.False(replacementPlan.Valid);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_CustomMapping_LocationsMappedThatAppearsInDataBlock_ReplacementValid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A",
            Name = "indicator_a",
        };

        var replacementIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "New indicator",
            Name = "new_indicator",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var locationEng = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.Country,
            Country = _england,
        };

        var originalLocationDerby = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = _derby,
        };
        var replacementLocationDerby = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = new LocalAuthority("E06000016", "", "Derby updated"), // updated code and name
        };

        var locationLeicester = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = new LocalAuthority("E03000033", "", "Leicester"),
        };

        var originalLocationBirmingham = new Location // doesn't appear in replacement
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthorityDistrict,
            LocalAuthorityDistrict = new LocalAuthorityDistrict("E02000022", "Birmingham"),
        };
        var replacementLocationNott = new Location // doesn't appear in original
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = new LocalAuthority("E01000011", "", "Nottingham"),
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = [],
                Indicators = [originalIndicator.Id],
                LocationIds = ListOf(locationEng.Id, originalLocationDerby.Id, locationLeicester.Id),
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var dataSetMapping = new DataSetMapping
        {
            OriginalDataFileId = originalFile.Id,
            ReplacementDataFileId = replacementFile.Id,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    originalIndicator.Id,
                    CreateIndicatorMapping(
                        originalIndicator,
                        originalIndicatorGroup,
                        replacementIndicator,
                        replacementIndicatorGroup,
                        MapStatus.ManuallySet
                    )
                },
            },
            UnmappedReplacementIndicators = [],
            LocationMappings = new Dictionary<Guid, LocationMapping>
            {
                { locationEng.Id, CreateLocationMapping(locationEng, locationEng, MapStatus.AutoSet) },
                {
                    originalLocationDerby.Id,
                    CreateLocationMapping(originalLocationDerby, replacementLocationDerby, MapStatus.ManuallySet)
                },
                {
                    locationLeicester.Id,
                    CreateLocationMapping(locationLeicester, locationLeicester, MapStatus.AutoSet)
                },
                { originalLocationBirmingham.Id, CreateLocationMapping(originalLocationBirmingham) },
            },
            UnmappedReplacementLocations =
            [
                new UnmappedLocation
                {
                    Id = replacementLocationNott.Id,
                    GeographicLevel = replacementLocationNott.GeographicLevel,
                    Name = replacementLocationNott.ToLocationAttribute().Name!,
                    Code = replacementLocationNott.ToLocationAttribute().GetCodeOrFallback(),
                },
            ],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataSetMappings.Add(dataSetMapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

            var replacementPlan = result.AssertRight();

            Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
            Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

            Assert.Equal(2, dataBlockPlan.Locations.Count);
            var countriesPlan = dataBlockPlan.Locations.Single(l => l.Key == nameof(GeographicLevel.Country)).Value;

            var engPlan = countriesPlan.LocationAttributes.Single();
            Assert.Multiple(
                () => Assert.Equal(locationEng.Id, engPlan.Id),
                () => Assert.Equal(locationEng.ToLocationAttribute().GetCodeOrFallback(), engPlan.Code),
                () => Assert.Equal(locationEng.ToLocationAttribute().Name, engPlan.Label),
                () => Assert.Equal(locationEng.Id, engPlan.Target),
                () => Assert.True(engPlan.Valid)
            );
            Assert.True(countriesPlan.Valid);

            var laPlan = dataBlockPlan.Locations.Single(l => l.Key == nameof(GeographicLevel.LocalAuthority)).Value;
            Assert.Equal(2, laPlan.LocationAttributes.Count());
            var derbyPlan = laPlan.LocationAttributes.Single(la =>
                la.Label == originalLocationDerby.ToLocationAttribute().Name
            );
            var leicesterPlan = laPlan.LocationAttributes.Single(la =>
                la.Label == locationLeicester.ToLocationAttribute().Name
            );
            Assert.Multiple(
                () => Assert.Equal(originalLocationDerby.Id, derbyPlan.Id),
                () => Assert.Equal(originalLocationDerby.ToLocationAttribute().GetCodeOrFallback(), derbyPlan.Code),
                () => Assert.Equal(originalLocationDerby.ToLocationAttribute().Name, derbyPlan.Label),
                () => Assert.Equal(replacementLocationDerby.Id, derbyPlan.Target),
                () => Assert.True(derbyPlan.Valid),
                () => Assert.Equal(locationLeicester.Id, leicesterPlan.Id),
                () => Assert.Equal(locationLeicester.ToLocationAttribute().GetCodeOrFallback(), leicesterPlan.Code),
                () => Assert.Equal(locationLeicester.ToLocationAttribute().Name, leicesterPlan.Label),
                () => Assert.Equal(locationLeicester.Id, leicesterPlan.Target),
                () => Assert.True(leicesterPlan.Valid)
            );
            Assert.True(laPlan.Valid);

            Assert.True(dataBlockPlan.Valid);

            var locationMappingPlan = replacementPlan.Mapping.Locations;

            Assert.Equal(4, locationMappingPlan.Mappings.Count);
            Assert.Equivalent(
                new[] { locationEng.Id, originalLocationDerby.Id, locationLeicester.Id, originalLocationBirmingham.Id },
                locationMappingPlan.Mappings.Keys
            );
            Assert.Contains(
                new ReplacementPlanLocationMappingViewModel
                {
                    CandidateKey = locationEng.Id,
                    Source = new ReplacementPlanLocationViewModel
                    {
                        Id = locationEng.Id,
                        Code = locationEng.ToLocationAttribute().Code!,
                        Name = locationEng.ToLocationAttribute().Name!,
                    },
                    Type = nameof(MapStatus.AutoSet),
                },
                locationMappingPlan.Mappings.Values
            );
            Assert.Contains(
                new ReplacementPlanLocationMappingViewModel
                {
                    CandidateKey = replacementLocationDerby.Id,
                    Source = new ReplacementPlanLocationViewModel
                    {
                        Id = originalLocationDerby.Id,
                        Code = originalLocationDerby.ToLocationAttribute().Code!,
                        Name = originalLocationDerby.ToLocationAttribute().Name!,
                    },
                    Type = nameof(MapStatus.ManuallySet),
                },
                locationMappingPlan.Mappings.Values
            );
            Assert.Contains(
                new ReplacementPlanLocationMappingViewModel
                {
                    CandidateKey = locationLeicester.Id,
                    Source = new ReplacementPlanLocationViewModel
                    {
                        Id = locationLeicester.Id,
                        Code = locationLeicester.ToLocationAttribute().Code!,
                        Name = locationLeicester.ToLocationAttribute().Name!,
                    },
                    Type = nameof(MapStatus.AutoSet),
                },
                locationMappingPlan.Mappings.Values
            );
            Assert.Contains(
                new ReplacementPlanLocationMappingViewModel
                {
                    CandidateKey = null,
                    Source = new ReplacementPlanLocationViewModel
                    {
                        Id = originalLocationBirmingham.Id,
                        Code = originalLocationBirmingham.ToLocationAttribute().Code!,
                        Name = originalLocationBirmingham.ToLocationAttribute().Name!,
                    },
                    Type = nameof(MapStatus.Unset),
                },
                locationMappingPlan.Mappings.Values
            );

            Assert.Equal(4, locationMappingPlan.Candidates.Count);
            Assert.Equivalent(
                new[] { replacementLocationNott.Id, locationEng.Id, replacementLocationDerby.Id, locationLeicester.Id },
                locationMappingPlan.Candidates.Keys
            );
            Assert.Contains(
                new ReplacementPlanLocationViewModel
                {
                    Id = replacementLocationNott.Id,
                    Name = replacementLocationNott.ToLocationAttribute().Name!,
                    Code = replacementLocationNott.ToLocationAttribute().Code!,
                },
                locationMappingPlan.Candidates.Values
            );
            Assert.Contains(
                new ReplacementPlanLocationViewModel
                {
                    Id = locationEng.Id,
                    Name = locationEng.ToLocationAttribute().Name!,
                    Code = locationEng.ToLocationAttribute().Code!,
                },
                locationMappingPlan.Candidates.Values
            );
            Assert.Contains(
                new ReplacementPlanLocationViewModel
                {
                    Id = replacementLocationDerby.Id,
                    Name = replacementLocationDerby.ToLocationAttribute().Name!,
                    Code = replacementLocationDerby.ToLocationAttribute().Code!,
                },
                locationMappingPlan.Candidates.Values
            );
            Assert.Contains(
                new ReplacementPlanLocationViewModel
                {
                    Id = locationLeicester.Id,
                    Name = locationLeicester.ToLocationAttribute().Name!,
                    Code = locationLeicester.ToLocationAttribute().Code!,
                },
                locationMappingPlan.Candidates.Values
            );

            Assert.True(replacementPlan.Valid);
        }
    }

    [Fact]
    public async Task GetReplacementPlan_CustomMapping_LocationUnmappedThatIsUsedInDataBlock_ReplacementInvalid()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease()).Generate();

        var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion().WithId(releaseVersion.Id).Generate();

        var (originalReleaseSubject, replacementReleaseSubject) = _fixture
            .DefaultReleaseSubject()
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
            Id = Guid.NewGuid(),
            Type = FileType.Data,
            SubjectId = replacementReleaseSubject.SubjectId,
            Replacing = originalFile,
        };

        originalFile.ReplacedBy = replacementFile;

        var originalReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = originalFile };

        var replacementReleaseFile = new ReleaseFile { ReleaseVersion = releaseVersion, File = replacementFile };

        var originalIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator A",
            Name = "indicator_a",
        };

        var replacementIndicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "New indicator",
            Name = "new_indicator",
        };

        var originalIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = originalReleaseSubject.Subject,
            Indicators = new List<Indicator> { originalIndicator },
        };

        var replacementIndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Default group - not changing",
            Subject = replacementReleaseSubject.Subject,
            Indicators = new List<Indicator> { replacementIndicator },
        };

        var originalLocationDerby = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = _derby,
        };
        var replacementLocationDerby = new Location
        {
            Id = Guid.NewGuid(),
            GeographicLevel = GeographicLevel.LocalAuthority,
            LocalAuthority = new LocalAuthority("E06000016", "", "Derby updated"), // updated code and name
        };

        var timePeriod = new TimePeriodQuery
        {
            StartYear = 2019,
            StartCode = CalendarYear,
            EndYear = 2020,
            EndCode = CalendarYear,
        };

        var dataBlock = new DataBlock
        {
            Name = "Test DataBlock",
            Query = new FullTableQuery
            {
                SubjectId = originalReleaseSubject.SubjectId,
                Filters = [],
                Indicators = [originalIndicator.Id],
                LocationIds = ListOf(originalLocationDerby.Id),
                TimePeriod = timePeriod,
            },
            ReleaseVersion = releaseVersion,
        };

        var timePeriodService = new Mock<ITimePeriodService>(Strict);
        timePeriodService
            .Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
            .ReturnsAsync(
                new List<(int Year, TimeIdentifier TimeIdentifier)> { (2019, CalendarYear), (2020, CalendarYear) }
            );

        var releaseFileRepository = new Mock<IReleaseFileRepository>(Strict);
        releaseFileRepository
            .Setup(mock => mock.CheckLinkedOriginalAndReplacementReleaseFilesExist(releaseVersion.Id, originalFile.Id))
            .ReturnsAsync((originalReleaseFile, replacementReleaseFile));

        var dataSetMapping = new DataSetMapping
        {
            OriginalDataFileId = originalFile.Id,
            ReplacementDataFileId = replacementFile.Id,
            IndicatorMappings = new Dictionary<Guid, IndicatorMapping>
            {
                {
                    originalIndicator.Id,
                    CreateIndicatorMapping(
                        originalIndicator,
                        originalIndicatorGroup,
                        replacementIndicator,
                        replacementIndicatorGroup,
                        MapStatus.ManuallySet
                    )
                },
            },
            UnmappedReplacementIndicators = [],
            LocationMappings = new Dictionary<Guid, LocationMapping>
            {
                { originalLocationDerby.Id, CreateLocationMapping(originalLocationDerby) },
            },
            UnmappedReplacementLocations =
            [
                new UnmappedLocation
                {
                    Id = replacementLocationDerby.Id,
                    GeographicLevel = replacementLocationDerby.GeographicLevel,
                    Name = replacementLocationDerby.ToLocationAttribute().Name!,
                    Code = replacementLocationDerby.ToLocationAttribute().GetCodeOrFallback(),
                },
            ],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
            contentDbContext.DataBlocks.AddRange(dataBlock);
            contentDbContext.DataSetMappings.Add(dataSetMapping);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
            statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject, replacementReleaseSubject);
            statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup, replacementIndicatorGroup);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var replacementPlanService = BuildReplacementPlanService(
                contentDbContext,
                statisticsDbContext,
                timePeriodService: timePeriodService.Object,
                releaseFileRepository: releaseFileRepository.Object
            );

            var result = await replacementPlanService.GetReplacementPlan(
                releaseVersionId: releaseVersion.Id,
                originalFileId: originalFile.Id
            );

            VerifyAllMocks(timePeriodService, releaseFileRepository);

            var replacementPlan = result.AssertRight();

            Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
            Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

            Assert.Single(replacementPlan.DataBlocks);
            var dataBlockPlan = replacementPlan.DataBlocks.First();
            Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
            Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

            var laPlan = Assert.Single(dataBlockPlan.Locations).Value;
            var derbyPlan = laPlan.LocationAttributes.Single();
            Assert.Multiple(
                () => Assert.Equal(originalLocationDerby.Id, derbyPlan.Id),
                () => Assert.Equal(originalLocationDerby.ToLocationAttribute().GetCodeOrFallback(), derbyPlan.Code),
                () => Assert.Equal(originalLocationDerby.ToLocationAttribute().Name, derbyPlan.Label),
                () => Assert.Null(derbyPlan.Target),
                () => Assert.False(derbyPlan.Valid)
            );
            Assert.False(laPlan.Valid);

            Assert.False(dataBlockPlan.Valid);

            var locationMapping = Assert.Single(replacementPlan.Mapping.Locations.Mappings);
            Assert.Equal(originalLocationDerby.Id, locationMapping.Key);
            var expectedLocationMapping = new ReplacementPlanLocationMappingViewModel
            {
                CandidateKey = null,
                Source = new ReplacementPlanLocationViewModel
                {
                    Id = originalLocationDerby.Id,
                    Code = originalLocationDerby.ToLocationAttribute().Code!,
                    Name = originalLocationDerby.ToLocationAttribute().Name!,
                },
                Type = nameof(MapStatus.Unset),
            };
            Assert.Equal(expectedLocationMapping, locationMapping.Value);

            var locationCandidate = Assert.Single(replacementPlan.Mapping.Locations.Candidates);
            Assert.Equal(replacementLocationDerby.Id, locationCandidate.Key);
            var expectedLocationCandidate = new ReplacementPlanLocationViewModel
            {
                Id = replacementLocationDerby.Id,
                Code = replacementLocationDerby.ToLocationAttribute().Code!,
                Name = replacementLocationDerby.ToLocationAttribute().Name!,
            };
            Assert.Equal(expectedLocationCandidate, locationCandidate.Value);

            Assert.False(replacementPlan.Valid);
        }
    }

    private static Footnote CreateFootnote(
        ReleaseVersion releaseVersion,
        string content,
        List<FilterFootnote>? filterFootnotes = null,
        List<FilterGroupFootnote>? filterGroupFootnotes = null,
        List<FilterItemFootnote>? filterItemFootnotes = null,
        List<IndicatorFootnote>? indicatorFootnotes = null,
        Subject? subject = null
    )
    {
        return new Footnote
        {
            Content = content,
            Filters = filterFootnotes ?? new List<FilterFootnote>(),
            FilterGroups = filterGroupFootnotes ?? new List<FilterGroupFootnote>(),
            FilterItems = filterItemFootnotes ?? new List<FilterItemFootnote>(),
            Indicators = indicatorFootnotes ?? new List<IndicatorFootnote>(),
            Subjects =
                subject != null
                    ? new List<SubjectFootnote> { new() { Subject = subject } }
                    : new List<SubjectFootnote>(),
            Releases = new List<ReleaseFootnote> { new() { ReleaseVersion = releaseVersion } },
        };
    }

    private static ReplacementPlanService BuildReplacementPlanService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IDataSetVersionService? dataSetVersionService = null,
        ITimePeriodService? timePeriodService = null,
        IDataSetVersionMappingService? apiDataSetVersionMappingService = null,
        IReleaseFileRepository? releaseFileRepository = null
    )
    {
        var userService = AlwaysTrueUserService().Object;
        return new ReplacementPlanService(
            contentDbContext,
            statisticsDbContext,
            new FilterRepository(statisticsDbContext),
            new FootnoteRepository(statisticsDbContext),
            dataSetVersionService ?? Mock.Of<IDataSetVersionService>(Strict),
            timePeriodService ?? Mock.Of<ITimePeriodService>(Strict),
            userService,
            apiDataSetVersionMappingService ?? Mock.Of<IDataSetVersionMappingService>(Strict),
            releaseFileRepository ?? Mock.Of<IReleaseFileRepository>(Strict)
        );
    }

    private static IndicatorMapping CreateIndicatorMapping(
        Indicator original,
        IndicatorGroup originalGroup,
        Indicator? replacement = null,
        IndicatorGroup? replacementGroup = null,
        MapStatus mapStatus = MapStatus.Unset
    )
    {
        return new IndicatorMapping
        {
            OriginalId = original.Id,
            OriginalColumnName = original.Name,
            OriginalLabel = original.Label,
            OriginalGroupId = originalGroup.Id,
            OriginalGroupLabel = originalGroup.Label,
            ReplacementId = replacement?.Id,
            ReplacementColumnName = replacement?.Name,
            ReplacementLabel = replacement?.Label,
            ReplacementGroupId = replacementGroup?.Id,
            ReplacementGroupLabel = replacementGroup?.Label,
            Status = mapStatus,
        };
    }

    private static LocationMapping CreateLocationMapping(
        Location original,
        Location? replacement = null,
        MapStatus status = MapStatus.Unset
    )
    {
        return new LocationMapping
        {
            OriginalId = original.Id,
            OriginalCode = original.ToLocationAttribute().Code!,
            OriginalName = original.ToLocationAttribute().Name!,
            OriginalGeographicLevel = original.GeographicLevel,
            ReplacementId = replacement?.Id,
            ReplacementCode = replacement?.ToLocationAttribute().Code!,
            ReplacementName = replacement?.ToLocationAttribute().Name!,
            ReplacementGeographicLevel = replacement?.GeographicLevel,
            Status = status,
        };
    }
}

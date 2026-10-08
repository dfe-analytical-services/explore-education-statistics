#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces.IReleaseService;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics;

public class FootnoteControllerTests
{
    private readonly FootnoteController _controller;

    private static readonly Guid FootnoteId = Guid.NewGuid();

    private static readonly Guid ReleaseVersionId = Guid.NewGuid();

    public FootnoteControllerTests()
    {
        var subjectIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var footnote = new Footnote
        {
            Id = FootnoteId,
            Content = "Sample footnote",
            Filters = new List<FilterFootnote>(),
            FilterGroups = new List<FilterGroupFootnote>(),
            FilterItems = new List<FilterItemFootnote>(),
            Indicators = new List<IndicatorFootnote>(),
            Subjects = new List<SubjectFootnote>(),
        };

        var dataSet = new Mock<IStorageDataSet>(MockBehavior.Strict);
        var storageDataSetResolver = new Mock<IStorageDataSetResolver>(MockBehavior.Strict);
        var footnoteService = new Mock<IFootnoteService>(MockBehavior.Strict);
        var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);

        footnoteService
            .Setup(s =>
                s.CreateFootnote(
                    ReleaseVersionId,
                    "Sample footnote",
                    It.IsAny<IReadOnlySet<Guid>>(),
                    It.IsAny<IReadOnlySet<Guid>>(),
                    It.IsAny<IReadOnlySet<Guid>>(),
                    It.IsAny<IReadOnlySet<Guid>>(),
                    It.IsAny<IReadOnlySet<Guid>>()
                )
            )
            .ReturnsAsync(footnote);

        footnoteService
            .Setup(s =>
                s.UpdateFootnote(
                    ReleaseVersionId,
                    FootnoteId,
                    "Updated sample footnote",
                    It.IsAny<IReadOnlySet<Guid>>(),
                    It.IsAny<IReadOnlySet<Guid>>(),
                    It.IsAny<IReadOnlySet<Guid>>(),
                    It.IsAny<IReadOnlySet<Guid>>(),
                    It.IsAny<IReadOnlySet<Guid>>()
                )
            )
            .ReturnsAsync(
                new Footnote
                {
                    Id = FootnoteId,
                    Content = "Updated sample footnote",
                    Filters = new List<FilterFootnote>(),
                    FilterGroups = new List<FilterGroupFootnote>(),
                    FilterItems = new List<FilterItemFootnote>(),
                    Indicators = new List<IndicatorFootnote>(),
                    Subjects = new List<SubjectFootnote>(),
                }
            );

        footnoteService.Setup(s => s.GetFootnotes(ReleaseVersionId)).ReturnsAsync(new List<Footnote> { footnote });

        footnoteService.Setup(s => s.DeleteFootnote(ReleaseVersionId, FootnoteId)).ReturnsAsync(Unit.Instance);

        releaseService
            .Setup(s => s.ListSubjects(ReleaseVersionId))
            .ReturnsAsync(
                subjectIds
                    .Select(id => new SubjectViewModel(
                        id: id,
                        name: $"Subject {id}",
                        order: 0,
                        content: "Test content",
                        timePeriods: new TimePeriodLabels(),
                        geographicLevels: new List<string>(),
                        geographicLevelsCsvOnly: new List<string>(),
                        filters: new List<string>(),
                        indicators: new List<string>(),
                        file: new FileInfo
                        {
                            Id = Guid.NewGuid(),
                            Name = "Test file",
                            FileName = "test.csv",
                            Size = "1 Mb",
                        },
                        lastUpdated: DateTime.Now
                    ))
                    .ToList()
            );

        storageDataSetResolver
            .Setup(s => s.Resolve(It.Is<File>(f => subjectIds.Contains(f.SubjectId!.Value))))
            .Returns(dataSet.Object);

        dataSet
            .Setup(s => s.ListFilters(includeItems: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<Filter>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Hint = "Filter Hint",
                        Label = "Filter label",
                        Name = "Filter name",
                        FilterGroups = new List<FilterGroup>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Filter group",
                                FilterItems = new List<FilterItem>
                                {
                                    new() { Id = Guid.NewGuid(), Label = "Filter item" },
                                },
                            },
                        },
                    },
                }
            );

        dataSet
            .Setup(s => s.ListIndicatorGroups(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<IndicatorGroup>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator group",
                        Indicators = new List<Indicator>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Indicator label",
                                Name = "Indicator name",
                                Unit = IndicatorUnit.Percent,
                                DecimalPlaces = 2,
                            },
                        },
                    },
                }
            );

        _controller = new FootnoteController(
            footnoteService.Object,
            storageDataSetResolver.Object,
            releaseService.Object,
            releaseDataFileRepository.Object
        );
    }

    [Fact]
    public async Task CreateFootnote()
    {
        var result = await _controller.CreateFootnote(
            ReleaseVersionId,
            new FootnoteCreateRequest
            {
                Content = "Sample footnote",
                Filters = SetOf<Guid>(),
                FilterGroups = SetOf<Guid>(),
                FilterItems = SetOf<Guid>(),
                Indicators = SetOf<Guid>(),
                Subjects = SetOf<Guid>(),
            }
        );

        result.AssertOkResult();
    }

    [Fact]
    public async Task GetFootnote_BuildsLinkHierarchyFromLinkParentIds()
    {
        var subjectId = Guid.NewGuid();
        var filter1Id = Guid.NewGuid();
        var filter1Group1Id = Guid.NewGuid();
        var filter1Group1Item1Id = Guid.NewGuid();
        var filter1Group2Id = Guid.NewGuid();
        var filter2Id = Guid.NewGuid();
        var indicatorGroupId = Guid.NewGuid();
        var indicatorId = Guid.NewGuid();

        var footnote = new Footnote
        {
            Id = Guid.NewGuid(),
            Content = "Footnote",
            Filters = [new FilterFootnote { FilterId = filter2Id, SubjectId = subjectId }],
            FilterGroups =
            [
                new FilterGroupFootnote
                {
                    FilterGroupId = filter1Group2Id,
                    FilterId = filter1Id,
                    SubjectId = subjectId,
                },
            ],
            FilterItems =
            [
                new FilterItemFootnote
                {
                    FilterItemId = filter1Group1Item1Id,
                    FilterGroupId = filter1Group1Id,
                    FilterId = filter1Id,
                    SubjectId = subjectId,
                },
            ],
            Indicators =
            [
                new IndicatorFootnote
                {
                    IndicatorId = indicatorId,
                    IndicatorGroupId = indicatorGroupId,
                    SubjectId = subjectId,
                },
            ],
            Subjects = [],
        };

        var footnoteService = new Mock<IFootnoteService>(MockBehavior.Strict);

        footnoteService.Setup(s => s.GetFootnote(ReleaseVersionId, footnote.Id)).ReturnsAsync(footnote);

        var controller = new FootnoteController(
            Mock.Of<IFilterRepository>(MockBehavior.Strict),
            footnoteService.Object,
            Mock.Of<IIndicatorGroupRepository>(MockBehavior.Strict),
            Mock.Of<IReleaseService>(MockBehavior.Strict),
            Mock.Of<IReleaseDataFileRepository>(MockBehavior.Strict)
        );

        var result = await controller.GetFootnote(ReleaseVersionId, footnote.Id);

        VerifyAllMocks(footnoteService);

        var viewModel = result.AssertOkResult();

        var subject = Assert.Single(viewModel.Subjects);
        Assert.Equal(subjectId, subject.Key);
        Assert.False(subject.Value.Selected);

        Assert.Equal(2, subject.Value.Filters.Count);

        var filter1ViewModel = subject.Value.Filters[filter1Id];
        Assert.False(filter1ViewModel.Selected);
        Assert.Equal(2, filter1ViewModel.FilterGroups.Count);
        Assert.False(filter1ViewModel.FilterGroups[filter1Group1Id].Selected);
        Assert.Equal(
            ListOf(filter1Group1Item1Id.ToString()),
            filter1ViewModel.FilterGroups[filter1Group1Id].FilterItems
        );
        Assert.True(filter1ViewModel.FilterGroups[filter1Group2Id].Selected);
        Assert.Empty(filter1ViewModel.FilterGroups[filter1Group2Id].FilterItems);

        var filter2ViewModel = subject.Value.Filters[filter2Id];
        Assert.True(filter2ViewModel.Selected);
        Assert.Empty(filter2ViewModel.FilterGroups);

        var indicatorGroup = Assert.Single(subject.Value.IndicatorGroups);
        Assert.Equal(indicatorGroupId, indicatorGroup.Key);
        Assert.Equal(ListOf(indicatorId.ToString()), indicatorGroup.Value.Indicators);
    }

    [Fact]
    public async Task GetFootnotes()
    {
        var result = await _controller.GetFootnotes(ReleaseVersionId);

        result.AssertOkResult();
    }

    [Fact]
    public async Task UpdateFootnote()
    {
        var result = await _controller.UpdateFootnote(
            releaseVersionId: ReleaseVersionId,
            footnoteId: FootnoteId,
            new FootnoteUpdateRequest
            {
                Content = "Updated sample footnote",
                Filters = SetOf<Guid>(),
                FilterGroups = SetOf<Guid>(),
                FilterItems = SetOf<Guid>(),
                Indicators = SetOf<Guid>(),
                Subjects = SetOf<Guid>(),
            }
        );

        result.AssertOkResult();
    }
}

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
using ReleaseFile = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseFile;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics;

public class FootnoteControllerTests
{
    private readonly FootnoteController _controller;

    private static readonly Guid FootnoteId = Guid.NewGuid();

    private static readonly Guid ReleaseVersionId = Guid.NewGuid();

    public FootnoteControllerTests()
    {
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

        var footnoteService = new Mock<IFootnoteService>(MockBehavior.Strict);

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

        _controller = new FootnoteController(
            footnoteService.Object,
            Mock.Of<IStorageDataSetResolver>(MockBehavior.Strict),
            Mock.Of<IReleaseService>(MockBehavior.Strict),
            Mock.Of<IReleaseDataFileRepository>(MockBehavior.Strict)
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
            footnoteService.Object,
            Mock.Of<IStorageDataSetResolver>(MockBehavior.Strict),
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
    public async Task GetFootnotesMeta_ReturnsFiltersAndIndicatorsFromEachSubjectsDataSet()
    {
        var subject1Id = Guid.NewGuid();
        var subject2Id = Guid.NewGuid();

        var releaseFile1 = new ReleaseFile
        {
            Name = "Subject 1",
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subject1Id,
                Type = FileType.Data,
            },
        };
        var releaseFile2 = new ReleaseFile
        {
            Name = "Subject 2",
            File = new File
            {
                Id = Guid.NewGuid(),
                SubjectId = subject2Id,
                Type = FileType.Data,
            },
        };

        // Subject 1's filter groups, filter items, indicator groups and indicators are not in label order
        var subject1FilterItemB = new FilterItem { Id = Guid.NewGuid(), Label = "Filter item B" };
        var subject1FilterItemA = new FilterItem { Id = Guid.NewGuid(), Label = "Filter item A" };
        var subject1FilterGroupB = new FilterGroup { Id = Guid.NewGuid(), Label = "Filter group B" };
        var subject1FilterGroupA = new FilterGroup
        {
            Id = Guid.NewGuid(),
            Label = "Filter group A",
            FilterItems = [subject1FilterItemB, subject1FilterItemA],
        };
        var subject1Filter = new Filter
        {
            Id = Guid.NewGuid(),
            Hint = "Subject 1 filter hint",
            Label = "Subject 1 filter",
            FilterGroups = [subject1FilterGroupB, subject1FilterGroupA],
        };

        var subject1IndicatorB = new Indicator { Id = Guid.NewGuid(), Label = "Indicator B" };
        var subject1IndicatorA = new Indicator { Id = Guid.NewGuid(), Label = "Indicator A" };
        var subject1IndicatorGroupB = new IndicatorGroup { Id = Guid.NewGuid(), Label = "Indicator group B" };
        var subject1IndicatorGroupA = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Indicator group A",
            Indicators = [subject1IndicatorB, subject1IndicatorA],
        };

        var subject2FilterItem = new FilterItem { Id = Guid.NewGuid(), Label = "Subject 2 filter item" };
        var subject2Filter = new Filter
        {
            Id = Guid.NewGuid(),
            Label = "Subject 2 filter",
            FilterGroups =
            [
                new FilterGroup
                {
                    Id = Guid.NewGuid(),
                    Label = "Subject 2 filter group",
                    FilterItems = [subject2FilterItem],
                },
            ],
        };

        var subject2Indicator = new Indicator { Id = Guid.NewGuid(), Label = "Subject 2 indicator" };
        var subject2IndicatorGroup = new IndicatorGroup
        {
            Id = Guid.NewGuid(),
            Label = "Subject 2 indicator group",
            Indicators = [subject2Indicator],
        };

        var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);
        var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(MockBehavior.Strict);
        var storageDataSetResolver = new Mock<IStorageDataSetResolver>(MockBehavior.Strict);
        var subject1DataSet = new Mock<IStorageDataSet>(MockBehavior.Strict);
        var subject2DataSet = new Mock<IStorageDataSet>(MockBehavior.Strict);

        releaseService
            .Setup(s => s.ListSubjects(ReleaseVersionId))
            .ReturnsAsync(ListOf(BuildSubjectViewModel(subject1Id), BuildSubjectViewModel(subject2Id)));

        releaseDataFileRepository.Setup(s => s.GetBySubject(ReleaseVersionId, subject1Id)).ReturnsAsync(releaseFile1);
        releaseDataFileRepository.Setup(s => s.GetBySubject(ReleaseVersionId, subject2Id)).ReturnsAsync(releaseFile2);

        storageDataSetResolver.Setup(s => s.Resolve(releaseFile1.File)).Returns(subject1DataSet.Object);
        storageDataSetResolver.Setup(s => s.Resolve(releaseFile2.File)).Returns(subject2DataSet.Object);

        subject1DataSet
            .Setup(s => s.ListFilters(includeItems: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject1Filter]);
        subject1DataSet
            .Setup(s => s.ListIndicatorGroups(It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject1IndicatorGroupB, subject1IndicatorGroupA]);

        subject2DataSet
            .Setup(s => s.ListFilters(includeItems: true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject2Filter]);
        subject2DataSet
            .Setup(s => s.ListIndicatorGroups(It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject2IndicatorGroup]);

        var controller = new FootnoteController(
            Mock.Of<IFootnoteService>(MockBehavior.Strict),
            storageDataSetResolver.Object,
            releaseService.Object,
            releaseDataFileRepository.Object
        );

        var result = await controller.GetFootnotesMeta(ReleaseVersionId);

        VerifyAllMocks(
            releaseService,
            releaseDataFileRepository,
            storageDataSetResolver,
            subject1DataSet,
            subject2DataSet
        );

        var viewModel = result.AssertOkResult();

        Assert.Equal(ListOf(subject1Id, subject2Id), viewModel.Subjects.Keys.ToList());

        var subject1 = viewModel.Subjects[subject1Id];
        Assert.Equal(subject1Id, subject1.SubjectId);
        Assert.Equal("Subject 1", subject1.SubjectName);

        var subject1FilterViewModel = Assert.Single(subject1.Filters);
        Assert.Equal(subject1Filter.Id, subject1FilterViewModel.Key);
        Assert.Equal("Subject 1 filter", subject1FilterViewModel.Value.Legend);
        Assert.Equal("Subject 1 filter hint", subject1FilterViewModel.Value.Hint);
        Assert.Equal(
            ListOf(subject1FilterGroupA.Id, subject1FilterGroupB.Id),
            subject1FilterViewModel.Value.Options.Keys.ToList()
        );
        Assert.Equal(
            ListOf(subject1FilterItemA.Id.ToString(), subject1FilterItemB.Id.ToString()),
            subject1FilterViewModel.Value.Options[subject1FilterGroupA.Id].Options.Select(o => o.Value).ToList()
        );
        Assert.Empty(subject1FilterViewModel.Value.Options[subject1FilterGroupB.Id].Options);

        Assert.Equal(ListOf(subject1IndicatorGroupA.Id, subject1IndicatorGroupB.Id), subject1.Indicators.Keys.ToList());
        Assert.Equal(
            ListOf(subject1IndicatorA.Id, subject1IndicatorB.Id),
            subject1.Indicators[subject1IndicatorGroupA.Id].Options.Select(o => o.Value).ToList()
        );
        Assert.Empty(subject1.Indicators[subject1IndicatorGroupB.Id].Options);

        var subject2 = viewModel.Subjects[subject2Id];
        Assert.Equal(subject2Id, subject2.SubjectId);
        Assert.Equal("Subject 2", subject2.SubjectName);

        var subject2FilterViewModel = Assert.Single(subject2.Filters);
        Assert.Equal(subject2Filter.Id, subject2FilterViewModel.Key);
        var subject2FilterGroupViewModel = Assert.Single(subject2FilterViewModel.Value.Options).Value;
        Assert.Equal(
            ListOf(subject2FilterItem.Id.ToString()),
            subject2FilterGroupViewModel.Options.Select(o => o.Value).ToList()
        );

        var subject2IndicatorGroupViewModel = Assert.Single(subject2.Indicators);
        Assert.Equal(subject2IndicatorGroup.Id, subject2IndicatorGroupViewModel.Key);
        Assert.Equal(
            ListOf(subject2Indicator.Id),
            subject2IndicatorGroupViewModel.Value.Options.Select(o => o.Value).ToList()
        );
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

    private static SubjectViewModel BuildSubjectViewModel(Guid subjectId)
    {
        return new SubjectViewModel(
            id: subjectId,
            name: $"Subject {subjectId}",
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
        );
    }
}

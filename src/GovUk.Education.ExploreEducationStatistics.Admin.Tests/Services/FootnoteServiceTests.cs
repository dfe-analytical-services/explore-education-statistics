#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class FootnoteServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task CreateFootnote_SubjectNotLinkedToRelease_ReturnsValidationResult()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion);

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            subjectIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task CreateFootnote_SubjectIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var subject = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject));

        var result = await CreateFootnoteWithConfiguration(releaseVersion.Id, contextId, subjectIds: SetOf(subject.Id));

        var footnote = result.AssertRight();
        var subjectFootnote = Assert.Single(footnote.Subjects);
        Assert.Equal(subject.Id, subjectFootnote.SubjectId);
    }

    [Fact]
    public async Task CreateFootnote_FilterNotLinkedToRelease_ReturnsValidationResult()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var subject = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject));

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            filterIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task CreateFootnote_FilterIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filter = _fixture.DefaultFilter().Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject));

        var result = await CreateFootnoteWithConfiguration(releaseVersion.Id, contextId, filterIds: SetOf(filter.Id));

        var footnote = result.AssertRight();
        var filterFootnote = Assert.Single(footnote.Filters);
        Assert.Equal(filter.Id, filterFootnote.FilterId);
    }

    [Fact]
    public async Task CreateFootnote_FilterGroupNotLinkedToRelease_ReturnsValidationResult()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filter = _fixture.DefaultFilter().Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject));

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            filterGroupIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task CreateFootnote_FilterGroupIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterGroup = _fixture.DefaultFilterGroup().Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject));

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            filterGroupIds: SetOf(filterGroup.Id)
        );

        var footnote = result.AssertRight();
        var filterGroupFootnote = Assert.Single(footnote.FilterGroups);
        Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);
    }

    [Fact]
    public async Task CreateFootnote_FilterItemNotLinkedToRelease_ReturnsValidationResult()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterGroup = _fixture.DefaultFilterGroup().Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject));

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            filterItemIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task CreateFootnote_FilterItemIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterItem = _fixture.DefaultFilterItem().Generate();
        var filterGroup = _fixture.DefaultFilterGroup().WithFilterItems(new List<FilterItem> { filterItem }).Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject));

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            filterItemIds: SetOf(filterItem.Id)
        );

        var footnote = result.AssertRight();
        var filterItemFootnote = Assert.Single(footnote.FilterItems);
        Assert.Equal(filterItem.Id, filterItemFootnote.FilterItemId);
    }

    [Fact]
    public async Task CreateFootnote_IndicatorNotLinkedToRelease_ReturnsValidationResult()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var subject = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject));

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            indicatorIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task CreateFootnote_IndicatorIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var indicator = _fixture.DefaultIndicator().Generate();
        var indicatorGroup = _fixture
            .DefaultIndicatorGroup()
            .WithIndicators(new List<Indicator> { indicator })
            .Generate();
        var subject = _fixture
            .DefaultSubject()
            .WithIndicatorGroups(new List<IndicatorGroup> { indicatorGroup })
            .Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject));

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            indicatorIds: SetOf(indicator.Id)
        );

        var footnote = result.AssertRight();
        var indicatorFootnote = Assert.Single(footnote.Indicators);
        Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
    }

    [Fact]
    public async Task CreateFootnote_WithFiltersAndIndicatorsAndSubjectsWhichAreAllLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterItem = _fixture.DefaultFilterItem().Generate();
        var filterGroup = _fixture.DefaultFilterGroup().WithFilterItems(new List<FilterItem> { filterItem }).Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var indicator = _fixture.DefaultIndicator().Generate();
        var indicatorGroup = _fixture
            .DefaultIndicatorGroup()
            .WithIndicators(new List<Indicator> { indicator })
            .Generate();
        var subject1 = _fixture
            .DefaultSubject()
            .WithFilters(new List<Filter> { filter })
            .WithIndicatorGroups(new List<IndicatorGroup> { indicatorGroup })
            .Generate();
        var subject2 = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject1, subject2));

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            content: "Test footnote",
            filterIds: SetOf(filter.Id),
            filterGroupIds: SetOf(filterGroup.Id),
            filterItemIds: SetOf(filterItem.Id),
            indicatorIds: SetOf(indicator.Id),
            subjectIds: SetOf(subject2.Id)
        );

        var footnote = result.AssertRight();
        var subjectFootnote = Assert.Single(footnote.Subjects);
        var filterFootnote = Assert.Single(footnote.Filters);
        var filterGroupFootnote = Assert.Single(footnote.FilterGroups);
        var filterItemFootnote = Assert.Single(footnote.FilterItems);
        var indicatorFootnote = Assert.Single(footnote.Indicators);
        Assert.Equal(subject2.Id, subjectFootnote.SubjectId);
        Assert.Equal(filter.Id, filterFootnote.FilterId);
        Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);
        Assert.Equal(filterItem.Id, filterItemFootnote.FilterItemId);
        Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
        Assert.Equal("Test footnote", footnote.Content);
    }

    [Theory]
    [InlineData(true, false, false, false, false)]
    [InlineData(false, true, false, false, false)]
    [InlineData(false, false, true, false, false)]
    [InlineData(false, false, false, true, false)]
    [InlineData(false, false, false, false, true)]
    public async Task CreateFootnote_WithFiltersAndIndicatorsAndSubjectsWhichAreAllLinkedToReleaseExceptOne_ReturnsValidationResult(
        bool filterMissing,
        bool filterGroupMissing,
        bool filterItemMissing,
        bool indicatorMissing,
        bool subjectMissing
    )
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterItem = _fixture.DefaultFilterItem().Generate();
        var filterGroup = _fixture.DefaultFilterGroup().WithFilterItems(new List<FilterItem> { filterItem }).Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var indicator = _fixture.DefaultIndicator().Generate();
        var indicatorGroup = _fixture
            .DefaultIndicatorGroup()
            .WithIndicators(new List<Indicator> { indicator })
            .Generate();
        var subject1 = _fixture
            .DefaultSubject()
            .WithFilters(new List<Filter> { filter })
            .WithIndicatorGroups(new List<IndicatorGroup> { indicatorGroup })
            .Generate();
        var subject2 = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject1, subject2));

        var result = await CreateFootnoteWithConfiguration(
            releaseVersion.Id,
            contextId,
            filterIds: SetOf(filterMissing ? Guid.NewGuid() : filter.Id),
            filterGroupIds: SetOf(filterGroupMissing ? Guid.NewGuid() : filterGroup.Id),
            filterItemIds: SetOf(filterItemMissing ? Guid.NewGuid() : filterItem.Id),
            indicatorIds: SetOf(indicatorMissing ? Guid.NewGuid() : indicator.Id),
            subjectIds: SetOf(subjectMissing ? Guid.NewGuid() : subject2.Id)
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task UpdateFootnote_SubjectNotLinkedToRelease_ReturnsValidationResult()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            subjectIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task UpdateFootnote_SubjectIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var subject = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject), footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            subjectIds: SetOf(subject.Id)
        );

        var updatedFootnote = result.AssertRight();
        var subjectFootnote = Assert.Single(updatedFootnote.Subjects);
        Assert.Equal(subject.Id, subjectFootnote.SubjectId);
    }

    [Fact]
    public async Task UpdateFootnote_FilterNotLinkedToRelease_ReturnsValidationResult()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var subject = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject), footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            filterIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task UpdateFootnote_FilterIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filter = _fixture.DefaultFilter().Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject), footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            filterIds: SetOf(filter.Id)
        );

        var updatedFootnote = result.AssertRight();
        var filterFootnote = Assert.Single(updatedFootnote.Filters);
        Assert.Equal(filter.Id, filterFootnote.FilterId);
    }

    [Fact]
    public async Task UpdateFootnote_FilterGroupNotLinkedToRelease_ReturnsValidationResult()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filter = _fixture.DefaultFilter().Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject), footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            filterGroupIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task UpdateFootnote_FilterGroupIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterGroup = _fixture.DefaultFilterGroup().Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject), footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            filterGroupIds: SetOf(filterGroup.Id)
        );

        var updatedFootnote = result.AssertRight();
        var filterGroupFootnote = Assert.Single(updatedFootnote.FilterGroups);
        Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);
    }

    [Fact]
    public async Task UpdateFootnote_FilterItemNotLinkedToRelease_ReturnsValidationResult()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterGroup = _fixture.DefaultFilterGroup().Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject), footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            filterItemIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task UpdateFootnote_FilterItemIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterItem = _fixture.DefaultFilterItem().Generate();
        var filterGroup = _fixture.DefaultFilterGroup().WithFilterItems(new List<FilterItem> { filterItem }).Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var subject = _fixture.DefaultSubject().WithFilters(new List<Filter> { filter }).Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject), footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            filterItemIds: SetOf(filterItem.Id)
        );

        var updatedFootnote = result.AssertRight();
        var filteritemFootnote = Assert.Single(updatedFootnote.FilterItems);
        Assert.Equal(filterItem.Id, filteritemFootnote.FilterItemId);
    }

    [Fact]
    public async Task UpdateFootnote_IndicatorNotLinkedToRelease_ReturnsValidationResult()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var subject = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject), footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            indicatorIds: SetOf(Guid.NewGuid())
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task UpdateFootnote_IndicatorIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var indicator = _fixture.DefaultIndicator().Generate();
        var indicatorGroup = _fixture
            .DefaultIndicatorGroup()
            .WithIndicators(new List<Indicator> { indicator })
            .Generate();
        var subject = _fixture
            .DefaultSubject()
            .WithIndicatorGroups(new List<IndicatorGroup> { indicatorGroup })
            .Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject), footnotes: ListOf(footnote));

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            indicatorIds: SetOf(indicator.Id)
        );

        var updatedFootnote = result.AssertRight();
        var indicatorFootnote = Assert.Single(updatedFootnote.Indicators);
        Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
    }

    [Fact]
    public async Task UpdateFootnote_WithFiltersAndIndicatorsAndSubjectsWhichAreAllLinkedToRelease_CreatesFootnoteWithCorrectLinks()
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterItem = _fixture.DefaultFilterItem().Generate();
        var filterGroup = _fixture.DefaultFilterGroup().WithFilterItems(new List<FilterItem> { filterItem }).Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var indicator = _fixture.DefaultIndicator().Generate();
        var indicatorGroup = _fixture
            .DefaultIndicatorGroup()
            .WithIndicators(new List<Indicator> { indicator })
            .Generate();
        var subject1 = _fixture
            .DefaultSubject()
            .WithFilters(new List<Filter> { filter })
            .WithIndicatorGroups(new List<IndicatorGroup> { indicatorGroup })
            .Generate();
        var subject2 = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(
            contextId,
            releaseVersion,
            subjects: ListOf(subject1, subject2),
            footnotes: ListOf(footnote)
        );

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            filterIds: SetOf(filter.Id),
            filterGroupIds: SetOf(filterGroup.Id),
            filterItemIds: SetOf(filterItem.Id),
            indicatorIds: SetOf(indicator.Id),
            subjectIds: SetOf(subject2.Id)
        );

        var updatedFootnote = result.AssertRight();
        var subjectFootnote = Assert.Single(updatedFootnote.Subjects);
        var filterFootnote = Assert.Single(updatedFootnote.Filters);
        var filterGroupFootnote = Assert.Single(updatedFootnote.FilterGroups);
        var filterItemFootnote = Assert.Single(updatedFootnote.FilterItems);
        var indicatorFootnote = Assert.Single(updatedFootnote.Indicators);
        Assert.Equal(subject2.Id, subjectFootnote.SubjectId);
        Assert.Equal(filter.Id, filterFootnote.FilterId);
        Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);
        Assert.Equal(filterItem.Id, filterItemFootnote.FilterItemId);
        Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
    }

    [Theory]
    [InlineData(true, false, false, false, false)]
    [InlineData(false, true, false, false, false)]
    [InlineData(false, false, true, false, false)]
    [InlineData(false, false, false, true, false)]
    [InlineData(false, false, false, false, true)]
    public async Task UpdateFootnote_WithFiltersAndIndicatorsAndSubjectsWhichAreAllLinkedToReleaseExceptOne_ReturnsValidationResult(
        bool filterMissing,
        bool filterGroupMissing,
        bool filterItemMissing,
        bool indicatorMissing,
        bool subjectMissing
    )
    {
        var footnote = _fixture.DefaultFootnote().Generate();
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());
        var filterItem = _fixture.DefaultFilterItem().Generate();
        var filterGroup = _fixture.DefaultFilterGroup().WithFilterItems(new List<FilterItem> { filterItem }).Generate();
        var filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup> { filterGroup }).Generate();
        var indicator = _fixture.DefaultIndicator().Generate();
        var indicatorGroup = _fixture
            .DefaultIndicatorGroup()
            .WithIndicators(new List<Indicator> { indicator })
            .Generate();
        var subject1 = _fixture
            .DefaultSubject()
            .WithFilters(new List<Filter> { filter })
            .WithIndicatorGroups(new List<IndicatorGroup> { indicatorGroup })
            .Generate();
        var subject2 = _fixture.DefaultSubject().Generate();

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(
            contextId,
            releaseVersion,
            subjects: ListOf(subject1, subject2),
            footnotes: ListOf(footnote)
        );

        var result = await UpdateFootnoteWithConfiguration(
            releaseVersion.Id,
            footnote.Id,
            contextId,
            filterIds: SetOf(filterMissing ? Guid.NewGuid() : filter.Id),
            filterGroupIds: SetOf(filterGroupMissing ? Guid.NewGuid() : filterGroup.Id),
            filterItemIds: SetOf(filterItemMissing ? Guid.NewGuid() : filterItem.Id),
            indicatorIds: SetOf(indicatorMissing ? Guid.NewGuid() : indicator.Id),
            subjectIds: SetOf(subjectMissing ? Guid.NewGuid() : subject2.Id)
        );

        result.AssertInternalServerError();
    }

    [Fact]
    public async Task CreateFootnote_MultipleFootnotesHaveExpectedOrder()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        Subject subject = _fixture.DefaultSubject();

        // Create a release which already has some existing footnotes
        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnotes(_fixture.DefaultFootnote().WithSubjects(ListOf(subject)).GenerateList(2))
            .GenerateList();

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnotes);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                    .Generate()
            );
            await contentDbContext.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(Strict);

        dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(releaseVersion.Id)).Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupFootnoteService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataBlockService: dataBlockService.Object
            );

            var result = (
                await service.CreateFootnote(
                    releaseVersionId: releaseVersion.Id,
                    "New Footnote",
                    filterIds: SetOf<Guid>(),
                    filterGroupIds: SetOf<Guid>(),
                    filterItemIds: SetOf<Guid>(),
                    indicatorIds: SetOf<Guid>(),
                    subjectIds: SetOf(subject.Id)
                )
            ).AssertRight();

            VerifyAllMocks(dataBlockService);

            // Check that the created footnote is assigned the next order in sequence
            Assert.Equal("New Footnote", result.Content);
            Assert.Equal(2, result.Order);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var retrievedFootnotes = await statisticsDbContext.Footnote.OrderBy(f => f.Order).ToListAsync();

            Assert.Equal(3, retrievedFootnotes.Count);

            Assert.Equal("Footnote 0 :: Content", retrievedFootnotes[0].Content);
            Assert.Equal(0, retrievedFootnotes[0].Order);
            Assert.Equal("Footnote 1 :: Content", retrievedFootnotes[1].Content);
            Assert.Equal(1, retrievedFootnotes[1].Order);
            Assert.Equal("New Footnote", retrievedFootnotes[2].Content);
            Assert.Equal(2, retrievedFootnotes[2].Order);
        }
    }

    [Fact]
    public async Task GetFootnote()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        Subject subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(1))
            .WithIndicatorGroups(
                _fixture.DefaultIndicatorGroup().WithIndicators(_fixture.DefaultIndicator().Generate(1)).Generate(1)
            );

        var releaseFootnote = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnote(
                _fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(subject))
                    .WithFilters(subject.Filters)
                    .WithFilterGroups(subject.Filters[0].FilterGroups)
                    .WithFilterItems(subject.Filters[0].FilterGroups[0].FilterItems)
                    .WithIndicators(subject.IndicatorGroups[0].Indicators)
            )
            .Generate();

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseFootnote.Add(releaseFootnote);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                    .Generate()
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

            var result = await service.GetFootnote(releaseVersion.Id, releaseFootnote.FootnoteId);
            var retrievedFootnote = result.AssertRight();

            Assert.Equal(releaseFootnote.FootnoteId, retrievedFootnote.Id);
            Assert.Equal("Footnote 0 :: Content", retrievedFootnote.Content);

            Assert.Single(retrievedFootnote.Releases);
            Assert.Equal(releaseVersion.Id, retrievedFootnote.Releases.First().ReleaseVersionId);

            Assert.Single(retrievedFootnote.Subjects);
            Assert.Equal(subject.Id, retrievedFootnote.Subjects.First().SubjectId);

            Assert.Single(retrievedFootnote.Filters);
            Assert.Equal(subject.Filters[0].Id, retrievedFootnote.Filters.First().Filter.Id);
            Assert.Equal(subject.Filters[0].Label, retrievedFootnote.Filters.First().Filter.Label);

            Assert.Single(retrievedFootnote.FilterGroups);
            Assert.Equal(subject.Filters[0].FilterGroups[0].Id, retrievedFootnote.FilterGroups.First().FilterGroup.Id);
            Assert.Equal(
                subject.Filters[0].FilterGroups[0].Label,
                retrievedFootnote.FilterGroups.First().FilterGroup.Label
            );

            Assert.Single(retrievedFootnote.FilterItems);
            Assert.Equal(
                subject.Filters[0].FilterGroups[0].FilterItems[0].Id,
                retrievedFootnote.FilterItems.First().FilterItem.Id
            );
            Assert.Equal(
                subject.Filters[0].FilterGroups[0].FilterItems[0].Label,
                retrievedFootnote.FilterItems.First().FilterItem.Label
            );

            Assert.Single(retrievedFootnote.Indicators);
            Assert.Equal(
                subject.IndicatorGroups[0].Indicators[0].Id,
                retrievedFootnote.Indicators.First().Indicator.Id
            );
            Assert.Equal(
                subject.IndicatorGroups[0].Indicators[0].Label,
                retrievedFootnote.Indicators.First().Indicator.Label
            );
        }
    }

    [Fact]
    public async Task GetFootnote_ReleaseNotFound()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var releaseFootnote = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnote(_fixture.DefaultFootnote())
            .Generate();

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.ReleaseFootnote.Add(releaseFootnote);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

            var invalidReleaseVersionId = Guid.NewGuid();
            var result = await service.GetFootnote(
                releaseVersionId: invalidReleaseVersionId,
                footnoteId: releaseFootnote.FootnoteId
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetFootnote_ReleaseAndFootnoteNotRelated()
    {
        var (releaseVersion, otherReleaseVersion) = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease())
            .GenerateTuple2();

        var releaseFootnote = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnote(_fixture.DefaultFootnote())
            .Generate();

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            statisticsDbContext.ReleaseFootnote.Add(releaseFootnote);

            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseVersions.AddRange(releaseVersion, otherReleaseVersion);

            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

            var result = await service.GetFootnote(otherReleaseVersion.Id, releaseFootnote.FootnoteId);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetFootnotes_ReturnsFootnotesForSubjectsOfReleaseVersion()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var (subject1, subject2) = _fixture.DefaultSubject().GenerateTuple2();

        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .ForIndex(0, s => s.SetFootnote(_fixture.DefaultFootnote().WithOrder(0).WithSubjects(ListOf(subject1))))
            .ForIndex(1, s => s.SetFootnote(_fixture.DefaultFootnote().WithOrder(1).WithSubjects(ListOf(subject2))))
            .GenerateList(2);

        var contextId = Guid.NewGuid().ToString();

        await SeedDatabase(contextId, releaseVersion, subjects: ListOf(subject1, subject2));

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.AttachRange(subject1, subject2);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnotes);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

            var result = (await service.GetFootnotes(releaseVersion.Id)).AssertRight();

            Assert.Equal(
                releaseFootnotes.Select(releaseFootnote => releaseFootnote.FootnoteId),
                result.Select(footnote => footnote.Id)
            );
            Assert.Equal(subject1.Id, Assert.Single(result[0].Subjects).SubjectId);
            Assert.Equal(subject2.Id, Assert.Single(result[1].Subjects).SubjectId);
        }
    }

    [Fact]
    public async Task DeleteFootnote()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        var (subject1, subject2) = _fixture
            .DefaultSubject()
            .WithFilters(_ => _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2))
            .WithIndicatorGroups(_ =>
                _fixture.DefaultIndicatorGroup().WithIndicators(_fixture.DefaultIndicator().Generate(1)).Generate(1)
            )
            .GenerateTuple2();

        var releaseFootnote = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnote(
                _fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(subject1))
                    .WithFilters(subject2.Filters)
                    .WithFilterGroups(subject2.Filters[0].FilterGroups)
                    .WithFilterItems(subject2.Filters[0].FilterGroups[0].FilterItems)
                    .WithIndicators(subject2.IndicatorGroups[0].Indicators)
            )
            .Generate();

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.AddRange(subject1, subject2);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnote);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.ReleaseFiles.AddRange(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFiles([
                        _fixture.DefaultFile(FileType.Data).WithSubjectId(subject1.Id).Generate(),
                        _fixture.DefaultFile(FileType.Data).WithSubjectId(subject2.Id).Generate(),
                    ])
                    .GenerateList()
            );
            await contentDbContext.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(Strict);

        dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(releaseVersion.Id)).Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupFootnoteService(
                statisticsDbContext,
                contentDbContext,
                dataBlockService: dataBlockService.Object
            );

            var result = await service.DeleteFootnote(
                releaseVersionId: releaseVersion.Id,
                footnoteId: releaseFootnote.FootnoteId
            );

            VerifyAllMocks(dataBlockService);

            result.AssertRight();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Empty(statisticsDbContext.Footnote);
            Assert.Empty(statisticsDbContext.ReleaseFootnote);
            Assert.Empty(statisticsDbContext.SubjectFootnote);
            Assert.Empty(statisticsDbContext.FilterFootnote);
            Assert.Empty(statisticsDbContext.FilterGroupFootnote);
            Assert.Empty(statisticsDbContext.FilterItemFootnote);
            Assert.Empty(statisticsDbContext.IndicatorFootnote);
        }
    }

    [Fact]
    public async Task DeleteFootnote_MultipleFootnotesHaveExpectedOrder()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        Subject subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2));

        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnotes(_fixture.DefaultFootnote().WithSubjects(ListOf(subject)).GenerateList(3))
            .GenerateList(3);

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnotes);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                    .Generate()
            );
            await contentDbContext.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(Strict);

        dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(releaseVersion.Id)).Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupFootnoteService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataBlockService: dataBlockService.Object
            );

            var result = await service.DeleteFootnote(
                releaseVersionId: releaseVersion.Id,
                footnoteId: releaseFootnotes[0].FootnoteId
            );

            VerifyAllMocks(dataBlockService);

            result.AssertRight();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var retrievedFootnotes = await statisticsDbContext.Footnote.OrderBy(f => f.Order).ToListAsync();

            Assert.Equal(2, retrievedFootnotes.Count);

            // Expect that the remaining footnotes have been reordered
            Assert.Equal("Footnote 1 :: Content", retrievedFootnotes[0].Content);
            Assert.Equal(0, retrievedFootnotes[0].Order);
            Assert.Equal("Footnote 2 :: Content", retrievedFootnotes[1].Content);
            Assert.Equal(1, retrievedFootnotes[1].Order);
        }
    }

    [Fact]
    public async Task UpdateFootnote_AddCriteria()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        Subject subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(3))
            .WithIndicatorGroups(
                _fixture.DefaultIndicatorGroup().WithIndicators(_fixture.DefaultIndicator().Generate(1)).Generate(1)
            );

        var releaseFootnote = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnote(_fixture.DefaultFootnote().WithOrder(1))
            .Generate();

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnote);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                    .Generate()
            );
            await contentDbContext.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(Strict);

        dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(releaseVersion.Id)).Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupFootnoteService(
                statisticsDbContext,
                contentDbContext,
                dataBlockService: dataBlockService.Object
            );

            var result = await service.UpdateFootnote(
                releaseVersionId: releaseVersion.Id,
                footnoteId: releaseFootnote.FootnoteId,
                "Updated footnote",
                filterIds: SetOf(subject.Filters[0].Id),
                filterGroupIds: SetOf(subject.Filters[1].FilterGroups[0].Id),
                filterItemIds: SetOf(subject.Filters[2].FilterGroups[0].FilterItems[0].Id),
                indicatorIds: SetOf(subject.IndicatorGroups[0].Indicators[0].Id),
                subjectIds: SetOf(subject.Id)
            );

            result.AssertRight();
        }

        VerifyAllMocks(dataBlockService);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var savedFootnote = Assert.Single(statisticsDbContext.Footnote);
            Assert.Equal(releaseFootnote.FootnoteId, savedFootnote.Id);
            Assert.Equal("Updated footnote", savedFootnote.Content);
            Assert.Equal(1, savedFootnote.Order);

            var savedReleaseFootnote = Assert.Single(statisticsDbContext.ReleaseFootnote);
            Assert.Equal(releaseVersion.Id, savedReleaseFootnote.ReleaseVersionId);
            Assert.Equal(releaseFootnote.FootnoteId, savedReleaseFootnote.FootnoteId);

            var savedSubjectFootnote = Assert.Single(statisticsDbContext.SubjectFootnote);
            Assert.Equal(subject.Id, savedSubjectFootnote.SubjectId);
            Assert.Equal(releaseFootnote.FootnoteId, savedSubjectFootnote.FootnoteId);

            var savedFilterFootnote = Assert.Single(statisticsDbContext.FilterFootnote);
            Assert.Equal(subject.Filters[0].Id, savedFilterFootnote.FilterId);
            Assert.Equal(releaseFootnote.FootnoteId, savedFilterFootnote.FootnoteId);

            var savedFilterGroupFootnote = Assert.Single(statisticsDbContext.FilterGroupFootnote);
            Assert.Equal(subject.Filters[1].FilterGroups[0].Id, savedFilterGroupFootnote.FilterGroupId);
            Assert.Equal(releaseFootnote.FootnoteId, savedFilterGroupFootnote.FootnoteId);

            var savedFilterItemFootnote = Assert.Single(statisticsDbContext.FilterItemFootnote);
            Assert.Equal(subject.Filters[2].FilterGroups[0].FilterItems[0].Id, savedFilterItemFootnote.FilterItemId);
            Assert.Equal(releaseFootnote.FootnoteId, savedFilterItemFootnote.FootnoteId);

            var savedIndicatorFootnote = Assert.Single(statisticsDbContext.IndicatorFootnote);
            Assert.Equal(subject.IndicatorGroups[0].Indicators[0].Id, savedIndicatorFootnote.IndicatorId);
            Assert.Equal(releaseFootnote.FootnoteId, savedIndicatorFootnote.FootnoteId);
        }
    }

    [Fact]
    public async Task UpdateFootnote_RemoveCriteria()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        Subject subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(3))
            .WithIndicatorGroups(
                _fixture.DefaultIndicatorGroup().WithIndicators(_fixture.DefaultIndicator().Generate(1)).Generate(1)
            );

        var releaseFootnote = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnote(_fixture.DefaultFootnote().WithOrder(1))
            .Generate();

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnote);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                    .Generate()
            );
            await contentDbContext.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(Strict);

        dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(releaseVersion.Id)).Returns(Task.CompletedTask);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = SetupFootnoteService(
                statisticsDbContext,
                contentDbContext,
                dataBlockService: dataBlockService.Object
            );

            var result = await service.UpdateFootnote(
                releaseVersionId: releaseVersion.Id,
                footnoteId: releaseFootnote.FootnoteId,
                "Updated footnote",
                filterIds: SetOf<Guid>(),
                filterGroupIds: SetOf<Guid>(),
                filterItemIds: SetOf<Guid>(),
                indicatorIds: SetOf<Guid>(),
                subjectIds: SetOf<Guid>()
            );

            result.AssertRight();
        }

        VerifyAllMocks(dataBlockService);

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var savedFootnote = Assert.Single(statisticsDbContext.Footnote);
            Assert.Equal(releaseFootnote.FootnoteId, savedFootnote.Id);
            Assert.Equal("Updated footnote", savedFootnote.Content);
            Assert.Equal(1, savedFootnote.Order);

            var savedReleaseFootnote = Assert.Single(statisticsDbContext.ReleaseFootnote);
            Assert.Equal(releaseVersion.Id, savedReleaseFootnote.ReleaseVersionId);
            Assert.Equal(releaseFootnote.FootnoteId, savedReleaseFootnote.FootnoteId);

            Assert.Empty(statisticsDbContext.SubjectFootnote);
            Assert.Empty(statisticsDbContext.FilterFootnote);
            Assert.Empty(statisticsDbContext.FilterGroupFootnote);
            Assert.Empty(statisticsDbContext.FilterItemFootnote);
            Assert.Empty(statisticsDbContext.IndicatorFootnote);
        }
    }

    [Fact]
    public async Task UpdateFootnotes()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        Subject subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2));

        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnotes(_fixture.DefaultFootnote().WithSubjects(ListOf(subject)).GenerateList(3))
            .GenerateList();

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnotes);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                    .Generate()
            );
            await contentDbContext.SaveChangesAsync();
        }

        var dataBlockService = new Mock<IDataBlockService>(Strict);

        dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(releaseVersion.Id)).Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupFootnoteService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataBlockService: dataBlockService.Object
            );

            // Create a request with identical footnotes but in a new order
            var request = new FootnotesUpdateRequest
            {
                FootnoteIds = ListOf(
                    releaseFootnotes[2].FootnoteId,
                    releaseFootnotes[0].FootnoteId,
                    releaseFootnotes[1].FootnoteId
                ),
            };

            var result = await service.UpdateFootnotes(releaseVersion.Id, request);

            VerifyAllMocks(dataBlockService);

            result.AssertRight();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var retrievedFootnotes = await statisticsDbContext.Footnote.OrderBy(f => f.Order).ToListAsync();

            Assert.Equal(3, retrievedFootnotes.Count);

            // Check the footnotes have been reordered
            Assert.Equal("Footnote 2 :: Content", retrievedFootnotes[0].Content);
            Assert.Equal(0, retrievedFootnotes[0].Order);
            Assert.Equal("Footnote 0 :: Content", retrievedFootnotes[1].Content);
            Assert.Equal(1, retrievedFootnotes[1].Order);
            Assert.Equal("Footnote 1 :: Content", retrievedFootnotes[2].Content);
            Assert.Equal(2, retrievedFootnotes[2].Order);
        }
    }

    [Fact]
    public async Task UpdateFootnotes_ReleaseNotFound()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        Subject subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2));

        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnotes(_fixture.DefaultFootnote().WithSubjects(ListOf(subject)).GenerateList(2))
            .GenerateList();

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnotes);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                    .Generate()
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupFootnoteService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            // Attempt to update the footnotes but use a different release id
            var result = await service.UpdateFootnotes(
                Guid.NewGuid(),
                new FootnotesUpdateRequest
                {
                    FootnoteIds = ListOf(releaseFootnotes[0].FootnoteId, releaseFootnotes[1].FootnoteId),
                }
            );

            result.AssertNotFound();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var retrievedFootnotes = await statisticsDbContext.Footnote.OrderBy(f => f.Order).ToListAsync();

            // Verify that the footnotes remain untouched
            Assert.Equal(2, retrievedFootnotes.Count);
            Assert.Equal("Footnote 0 :: Content", retrievedFootnotes[0].Content);
            Assert.Equal(0, retrievedFootnotes[0].Order);
            Assert.Equal("Footnote 1 :: Content", retrievedFootnotes[1].Content);
            Assert.Equal(1, retrievedFootnotes[1].Order);
        }
    }

    [Fact]
    public async Task UpdateFootnotes_FootnoteMissing()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        Subject subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2));

        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnotes(_fixture.DefaultFootnote().WithSubjects(ListOf(subject)).GenerateList(2))
            .GenerateList();

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnotes);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                    .Generate()
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupFootnoteService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            // Request has the first footnote id missing
            var request = new FootnotesUpdateRequest { FootnoteIds = ListOf(releaseFootnotes[1].FootnoteId) };

            var result = await service.UpdateFootnotes(releaseVersion.Id, request);

            result.AssertBadRequest(FootnotesDifferFromReleaseFootnotes);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var retrievedFootnotes = await statisticsDbContext.Footnote.OrderBy(f => f.Order).ToListAsync();

            // Verify that the footnotes remain untouched
            Assert.Equal(2, retrievedFootnotes.Count);
            Assert.Equal("Footnote 0 :: Content", retrievedFootnotes[0].Content);
            Assert.Equal(0, retrievedFootnotes[0].Order);
            Assert.Equal("Footnote 1 :: Content", retrievedFootnotes[1].Content);
            Assert.Equal(1, retrievedFootnotes[1].Order);
        }
    }

    [Fact]
    public async Task UpdateFootnotes_FootnoteNotForRelease()
    {
        ReleaseVersion releaseVersion = _fixture.DefaultReleaseVersion().WithRelease(_fixture.DefaultRelease());

        Subject subject = _fixture
            .DefaultSubject()
            .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2));

        var releaseFootnotes = _fixture
            .DefaultReleaseFootnote()
            .WithReleaseVersionId(releaseVersion.Id)
            .WithFootnotes(_fixture.DefaultFootnote().WithSubjects(ListOf(subject)).GenerateList(2))
            .GenerateList();

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Subject.Add(subject);
            statisticsDbContext.ReleaseFootnote.AddRange(releaseFootnotes);
            await statisticsDbContext.SaveChangesAsync();

            contentDbContext.ReleaseFiles.Add(
                _fixture
                    .DefaultReleaseFile()
                    .WithReleaseVersion(releaseVersion)
                    .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                    .Generate()
            );
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupFootnoteService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext
            );

            // Request has a footnote id not for this release
            var request = new FootnotesUpdateRequest
            {
                FootnoteIds = ListOf(releaseFootnotes[1].FootnoteId, releaseFootnotes[0].FootnoteId, Guid.NewGuid()),
            };

            var result = await service.UpdateFootnotes(releaseVersion.Id, request);

            result.AssertBadRequest(FootnotesDifferFromReleaseFootnotes);
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var retrievedFootnotes = await statisticsDbContext.Footnote.OrderBy(f => f.Order).ToListAsync();

            // Verify that the footnotes remain untouched
            Assert.Equal(2, retrievedFootnotes.Count);
            Assert.Equal("Footnote 0 :: Content", retrievedFootnotes[0].Content);
            Assert.Equal(0, retrievedFootnotes[0].Order);
            Assert.Equal("Footnote 1 :: Content", retrievedFootnotes[1].Content);
            Assert.Equal(1, retrievedFootnotes[1].Order);
        }
    }

    private async Task SeedDatabase(
        string contextId,
        ReleaseVersion releaseVersion,
        IReadOnlyList<Subject>? subjects = null,
        IReadOnlyList<Footnote>? footnotes = null
    )
    {
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);

            if (!subjects.IsNullOrEmpty())
            {
                statisticsDbContext.Subject.AddRange(subjects!);

                contentDbContext.ReleaseFiles.AddRange(
                    subjects!.Select(subject =>
                        _fixture
                            .DefaultReleaseFile()
                            .WithReleaseVersion(releaseVersion)
                            .WithFile(_fixture.DefaultFile(FileType.Data).WithSubjectId(subject.Id))
                            .Generate()
                    )
                );
            }

            if (!footnotes.IsNullOrEmpty())
            {
                statisticsDbContext.Footnote.AddRange(footnotes!);
            }

            await statisticsDbContext.SaveChangesAsync();
            await contentDbContext.SaveChangesAsync();
        }
    }

    private static async Task<Either<ActionResult, Footnote>> CreateFootnoteWithConfiguration(
        Guid releaseVersionId,
        string contextId,
        string content = "",
        IReadOnlySet<Guid>? filterIds = null,
        IReadOnlySet<Guid>? filterGroupIds = null,
        IReadOnlySet<Guid>? filterItemIds = null,
        IReadOnlySet<Guid>? indicatorIds = null,
        IReadOnlySet<Guid>? subjectIds = null
    )
    {
        filterIds ??= SetOf<Guid>();
        filterGroupIds ??= SetOf<Guid>();
        filterItemIds ??= SetOf<Guid>();
        indicatorIds ??= SetOf<Guid>();
        subjectIds ??= SetOf<Guid>();

        var dataBlockService = new Mock<IDataBlockService>();
        dataBlockService.Setup(dbs => dbs.InvalidateCachedDataBlocks(releaseVersionId)).Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var footnoteService = SetupFootnoteService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataBlockService: dataBlockService.Object
            );

            return await footnoteService.CreateFootnote(
                releaseVersionId,
                content,
                filterIds,
                filterGroupIds,
                filterItemIds,
                indicatorIds,
                subjectIds
            );
        }
    }

    private static async Task<Either<ActionResult, Footnote>> UpdateFootnoteWithConfiguration(
        Guid releaseVersionId,
        Guid footnoteId,
        string contextId,
        string content = "",
        IReadOnlySet<Guid>? filterIds = null,
        IReadOnlySet<Guid>? filterGroupIds = null,
        IReadOnlySet<Guid>? filterItemIds = null,
        IReadOnlySet<Guid>? indicatorIds = null,
        IReadOnlySet<Guid>? subjectIds = null
    )
    {
        filterIds ??= SetOf<Guid>();
        filterGroupIds ??= SetOf<Guid>();
        filterItemIds ??= SetOf<Guid>();
        indicatorIds ??= SetOf<Guid>();
        subjectIds ??= SetOf<Guid>();

        var dataBlockService = new Mock<IDataBlockService>();
        dataBlockService.Setup(dbs => dbs.InvalidateCachedDataBlocks(releaseVersionId)).Verifiable();

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var footnoteService = SetupFootnoteService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                dataBlockService: dataBlockService.Object
            );

            return await footnoteService.UpdateFootnote(
                releaseVersionId,
                footnoteId,
                content,
                filterIds,
                filterGroupIds,
                filterItemIds,
                indicatorIds,
                subjectIds
            );
        }
    }

    private static FootnoteService SetupFootnoteService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext? contentDbContext = null,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IUserService? userService = null,
        IDataBlockService? dataBlockService = null,
        IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null
    )
    {
        var contentContext = contentDbContext ?? InMemoryApplicationDbContext();

        var footnoteRepository = new FootnoteRepository(statisticsDbContext);

        return new FootnoteService(
            statisticsDbContext,
            contentContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentContext),
            userService ?? AlwaysTrueUserService().Object,
            dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
            footnoteRepository,
            statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext)
        );
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FootnoteServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task CreateFootnote_SubjectNotLinkedToRelease_ReturnsValidationResult()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release);

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                subjectIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task CreateFootnote_SubjectIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                subjectIds: SetOf(subject.Id));

            Footnote footnote = result.AssertRight();
            SubjectFootnote subjectFootnote = Assert.Single(footnote.Subjects);
            Assert.Equal(subject.Id, subjectFootnote.SubjectId);
        }

        [Fact]
        public async Task CreateFootnote_FilterNotLinkedToRelease_ReturnsValidationResult()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                filterIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task CreateFootnote_FilterIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                filterIds: SetOf(filter.Id));

            Footnote footnote = result.AssertRight();
            FilterFootnote filterFootnote = Assert.Single(footnote.Filters);
            Assert.Equal(filter.Id, filterFootnote.FilterId);
        }

        [Fact]
        public async Task CreateFootnote_FilterGroupNotLinkedToRelease_ReturnsValidationResult()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                filterGroupIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task CreateFootnote_FilterGroupIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                filterGroupIds: SetOf(filterGroup.Id));

            Footnote footnote = result.AssertRight();
            FilterGroupFootnote filterGroupFootnote = Assert.Single(footnote.FilterGroups);
            Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);
        }

        [Fact]
        public async Task CreateFootnote_FilterItemNotLinkedToRelease_ReturnsValidationResult()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                filterItemIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task CreateFootnote_FilterItemIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterItem filterItem = _fixture
                .DefaultFilterItem()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .WithFilterItems(new List<FilterItem>() { filterItem })
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                filterItemIds: SetOf(filterItem.Id));

            Footnote footnote = result.AssertRight();
            FilterItemFootnote filterItemFootnote = Assert.Single(footnote.FilterItems);
            Assert.Equal(filterItem.Id, filterItemFootnote.FilterItemId);
        }

        [Fact]
        public async Task CreateFootnote_IndicatorNotLinkedToRelease_ReturnsValidationResult()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                indicatorIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task CreateFootnote_IndicatorIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Indicator indicator = _fixture
                .DefaultIndicator()
                .Generate();
            IndicatorGroup indicatorGroup = _fixture
                .DefaultIndicatorGroup()
                .WithIndicators(new List<Indicator>() { indicator })
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithIndicatorGroups(new List<IndicatorGroup>() { indicatorGroup })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                indicatorIds: SetOf(indicator.Id));

            Footnote footnote = result.AssertRight();
            IndicatorFootnote indicatorFootnote = Assert.Single(footnote.Indicators);
            Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
        }

        [Fact]
        public async Task
            CreateFootnote_WithFiltersAndIndicatorsAndSubjectsWhichAreAllLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterItem filterItem = _fixture
                .DefaultFilterItem()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .WithFilterItems(new List<FilterItem>() { filterItem })
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Indicator indicator = _fixture
                .DefaultIndicator()
                .Generate();
            IndicatorGroup indicatorGroup = _fixture
                .DefaultIndicatorGroup()
                .WithIndicators(new List<Indicator>() { indicator })
                .Generate();
            Subject subject1 = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .WithIndicatorGroups(new List<IndicatorGroup>() { indicatorGroup })
                .Generate();
            Subject subject2 = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject1 = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject1)
                .Generate();
            ReleaseSubject releaseSubject2 = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject2)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject1, subject2),
                releaseSubjects: ListOf(releaseSubject1, releaseSubject2));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                content: "Test footnote",
                filterIds: SetOf(filter.Id),
                filterGroupIds: SetOf(filterGroup.Id),
                filterItemIds: SetOf(filterItem.Id),
                indicatorIds: SetOf(indicator.Id),
                subjectIds: SetOf(subject2.Id));

            Footnote footnote = result.AssertRight();
            SubjectFootnote subjectFootnote = Assert.Single(footnote.Subjects);
            FilterFootnote filterFootnote = Assert.Single(footnote.Filters);
            FilterGroupFootnote filterGroupFootnote = Assert.Single(footnote.FilterGroups);
            FilterItemFootnote filterItemFootnote = Assert.Single(footnote.FilterItems);
            IndicatorFootnote indicatorFootnote = Assert.Single(footnote.Indicators);
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
        public async Task
            CreateFootnote_WithFiltersAndIndicatorsAndSubjectsWhichAreAllLinkedToReleaseExceptOne_ReturnsValidationResult(
            bool filterMissing,
            bool filterGroupMissing,
            bool filterItemMissing,
            bool indicatorMissing,
            bool subjectMissing)
        {
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterItem filterItem = _fixture
                .DefaultFilterItem()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .WithFilterItems(new List<FilterItem>() { filterItem })
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Indicator indicator = _fixture
                .DefaultIndicator()
                .Generate();
            IndicatorGroup indicatorGroup = _fixture
                .DefaultIndicatorGroup()
                .WithIndicators(new List<Indicator>() { indicator })
                .Generate();
            Subject subject1 = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .WithIndicatorGroups(new List<IndicatorGroup>() { indicatorGroup })
                .Generate();
            Subject subject2 = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject1 = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject1)
                .Generate();
            ReleaseSubject releaseSubject2 = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject2)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject1, subject2),
                releaseSubjects: ListOf(releaseSubject1, releaseSubject2));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                filterIds: SetOf(filterMissing ? Guid.NewGuid() : filter.Id),
                filterGroupIds: SetOf(filterGroupMissing ? Guid.NewGuid() : filterGroup.Id),
                filterItemIds: SetOf(filterItemMissing ? Guid.NewGuid() : filterItem.Id),
                indicatorIds: SetOf(indicatorMissing ? Guid.NewGuid() : indicator.Id),
                subjectIds: SetOf(subjectMissing ? Guid.NewGuid() : subject2.Id));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task UpdateFootnote_SubjectNotLinkedToRelease_ReturnsValidationResult()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                subjectIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task UpdateFootnote_SubjectIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject),
                releaseSubjects: ListOf(releaseSubject),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                subjectIds: SetOf(subject.Id));

            Footnote updatedFootnote = result.AssertRight();
            SubjectFootnote subjectFootnote = Assert.Single(updatedFootnote.Subjects);
            Assert.Equal(subject.Id, subjectFootnote.SubjectId);
        }

        [Fact]
        public async Task UpdateFootnote_FilterNotLinkedToRelease_ReturnsValidationResult()
        {
            Footnote footnote = _fixture.
                DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject),
                releaseSubjects: ListOf(releaseSubject),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                filterIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task UpdateFootnote_FilterIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject),
                releaseSubjects: ListOf(releaseSubject),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                filterIds: SetOf(filter.Id));

            Footnote updatedFootnote = result.AssertRight();
            FilterFootnote filterFootnote = Assert.Single(updatedFootnote.Filters);
            Assert.Equal(filter.Id, filterFootnote.FilterId);
        }

        [Fact]
        public async Task UpdateFootnote_FilterGroupNotLinkedToRelease_ReturnsValidationResult()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject),
                releaseSubjects: ListOf(releaseSubject),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                filterGroupIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task UpdateFootnote_FilterGroupIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject),
                releaseSubjects: ListOf(releaseSubject),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                filterGroupIds: SetOf(filterGroup.Id));

            Footnote updatedFootnote = result.AssertRight();
            FilterGroupFootnote filterGroupFootnote = Assert.Single(updatedFootnote.FilterGroups);
            Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);
        }

        [Fact]
        public async Task UpdateFootnote_FilterItemNotLinkedToRelease_ReturnsValidationResult()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject),
                releaseSubjects: ListOf(releaseSubject),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                filterItemIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task UpdateFootnote_FilterItemIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterItem filterItem = _fixture
                .DefaultFilterItem()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .WithFilterItems(new List<FilterItem>() { filterItem })
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject),
                releaseSubjects: ListOf(releaseSubject),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                filterItemIds: SetOf(filterItem.Id));

            Footnote updatedFootnote = result.AssertRight();
            FilterItemFootnote filteritemFootnote = Assert.Single(updatedFootnote.FilterItems);
            Assert.Equal(filterItem.Id, filteritemFootnote.FilterItemId);
        }

        [Fact]
        public async Task UpdateFootnote_IndicatorNotLinkedToRelease_ReturnsValidationResult()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject),
                releaseSubjects: ListOf(releaseSubject),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                indicatorIds: SetOf(Guid.NewGuid()));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task UpdateFootnote_IndicatorIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            Indicator indicator = _fixture
                .DefaultIndicator()
                .Generate();
            IndicatorGroup indicatorGroup = _fixture
                .DefaultIndicatorGroup()
                .WithIndicators(new List<Indicator>() { indicator })
                .Generate();
            Subject subject = _fixture
                .DefaultSubject()
                .WithIndicatorGroups(new List<IndicatorGroup>() { indicatorGroup })
                .Generate();
            ReleaseSubject releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject),
                releaseSubjects: ListOf(releaseSubject),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                indicatorIds: SetOf(indicator.Id));

            Footnote updatedFootnote = result.AssertRight();
            IndicatorFootnote indicatorFootnote = Assert.Single(updatedFootnote.Indicators);
            Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
        }

        [Fact]
        public async Task
            UpdateFootnote_WithFiltersAndIndicatorsAndSubjectsWhichAreAllLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterItem filterItem = _fixture
                .DefaultFilterItem()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .WithFilterItems(new List<FilterItem>() { filterItem })
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Indicator indicator = _fixture
                .DefaultIndicator()
                .Generate();
            IndicatorGroup indicatorGroup = _fixture
                .DefaultIndicatorGroup()
                .WithIndicators(new List<Indicator>() { indicator })
                .Generate();
            Subject subject1 = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .WithIndicatorGroups(new List<IndicatorGroup>() { indicatorGroup })
                .Generate();
            Subject subject2 = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject1 = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject1)
                .Generate();
            ReleaseSubject releaseSubject2 = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject2)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject1, subject2),
                releaseSubjects: ListOf(releaseSubject1, releaseSubject2),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                filterIds: SetOf(filter.Id),
                filterGroupIds: SetOf(filterGroup.Id),
                filterItemIds: SetOf(filterItem.Id),
                indicatorIds: SetOf(indicator.Id),
                subjectIds: SetOf(subject2.Id));

            Footnote updatedFootnote = result.AssertRight();
            SubjectFootnote subjectFootnote = Assert.Single(updatedFootnote.Subjects);
            FilterFootnote filterFootnote = Assert.Single(updatedFootnote.Filters);
            FilterGroupFootnote filterGroupFootnote = Assert.Single(updatedFootnote.FilterGroups);
            FilterItemFootnote filterItemFootnote = Assert.Single(updatedFootnote.FilterItems);
            IndicatorFootnote indicatorFootnote = Assert.Single(updatedFootnote.Indicators);
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
        public async Task
            UpdateFootnote_WithFiltersAndIndicatorsAndSubjectsWhichAreAllLinkedToReleaseExceptOne_ReturnsValidationResult(
            bool filterMissing,
            bool filterGroupMissing,
            bool filterItemMissing,
            bool indicatorMissing,
            bool subjectMissing)
        {
            Footnote footnote = _fixture
                .DefaultFootnote()
                .Generate();
            Release release = _fixture
                .DefaultStatsRelease()
                .Generate();
            FilterItem filterItem = _fixture
                .DefaultFilterItem()
                .Generate();
            FilterGroup filterGroup = _fixture
                .DefaultFilterGroup()
                .WithFilterItems(new List<FilterItem>() { filterItem })
                .Generate();
            Filter filter = _fixture
                .DefaultFilter()
                .WithFilterGroups(new List<FilterGroup>() { filterGroup })
                .Generate();
            Indicator indicator = _fixture
                .DefaultIndicator()
                .Generate();
            IndicatorGroup indicatorGroup = _fixture
                .DefaultIndicatorGroup()
                .WithIndicators(new List<Indicator>() { indicator })
                .Generate();
            Subject subject1 = _fixture
                .DefaultSubject()
                .WithFilters(new List<Filter>() { filter })
                .WithIndicatorGroups(new List<IndicatorGroup>() { indicatorGroup })
                .Generate();
            Subject subject2 = _fixture
                .DefaultSubject()
                .Generate();
            ReleaseSubject releaseSubject1 = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject1)
                .Generate();
            ReleaseSubject releaseSubject2 = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(subject2)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(
                contextId,
                release,
                subjects: ListOf(subject1, subject2),
                releaseSubjects: ListOf(releaseSubject1, releaseSubject2),
                footnotes: ListOf(footnote));

            Either<ActionResult, Footnote> result = await UpdateFootnoteWithConfiguration(
                release.Id,
                footnote.Id,
                contextId,
                filterIds: SetOf(filterMissing ? Guid.NewGuid() : filter.Id),
                filterGroupIds: SetOf(filterGroupMissing ? Guid.NewGuid() : filterGroup.Id),
                filterItemIds: SetOf(filterItemMissing ? Guid.NewGuid() : filterItem.Id),
                indicatorIds: SetOf(indicatorMissing ? Guid.NewGuid() : indicator.Id),
                subjectIds: SetOf(subjectMissing ? Guid.NewGuid() : subject2.Id));

            result.AssertInternalServerError();
        }

        [Fact]
        public async Task CreateFootnote_MultipleFootnotesHaveExpectedOrder()
        {
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture.DefaultSubject())
                .Generate();

            // Create a release which already has some existing footnotes
            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(releaseSubject.Subject))
                    .GenerateList(2))
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Subject.AddAsync(releaseSubject.Subject);
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = (await service.CreateFootnote(
                    releaseId: release.Id,
                    "New Footnote",
                    filterIds: SetOf<Guid>(),
                    filterGroupIds: SetOf<Guid>(),
                    filterItemIds: SetOf<Guid>(),
                    indicatorIds: SetOf<Guid>(),
                    subjectIds: SetOf(releaseSubject.Subject.Id)
                )).AssertRight();

                VerifyAllMocks(dataBlockService);

                // Check that the created footnote is assigned the next order in sequence
                Assert.Equal("New Footnote", result.Content);
                Assert.Equal(2, result.Order);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var retrievedFootnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

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
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                        .Generate(1))
                    .WithIndicatorGroups(_fixture.DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1)))
                .Generate();

            var releaseFootnote = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnote(_fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(releaseSubject.Subject))
                    .WithFilters(releaseSubject.Subject.Filters)
                    .WithFilterGroups(releaseSubject.Subject.Filters[0].FilterGroups)
                    .WithFilterItems(releaseSubject.Subject.Filters[0].FilterGroups[0].FilterItems)
                    .WithIndicators(releaseSubject.Subject.IndicatorGroups[0].Indicators))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddAsync(releaseFootnote);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var result = await service.GetFootnote(release.Id, releaseFootnote.FootnoteId);
                var retrievedFootnote = result.AssertRight();

                Assert.Equal(releaseFootnote.FootnoteId, retrievedFootnote.Id);
                Assert.Equal("Footnote 0 :: Content", retrievedFootnote.Content);

                Assert.Single(retrievedFootnote.Releases);
                Assert.Equal(release.Id, retrievedFootnote.Releases.First().ReleaseId);

                Assert.Single(retrievedFootnote.Subjects);
                Assert.Equal(releaseSubject.Subject.Id, retrievedFootnote.Subjects.First().SubjectId);

                Assert.Single(retrievedFootnote.Filters);
                Assert.Equal(releaseSubject.Subject.Filters[0].Id, retrievedFootnote.Filters.First().Filter.Id);
                Assert.Equal(releaseSubject.Subject.Filters[0].Label, retrievedFootnote.Filters.First().Filter.Label);

                Assert.Single(retrievedFootnote.FilterGroups);
                Assert.Equal(releaseSubject.Subject.Filters[0].FilterGroups[0].Id, retrievedFootnote.FilterGroups.First().FilterGroup.Id);
                Assert.Equal(releaseSubject.Subject.Filters[0].FilterGroups[0].Label, retrievedFootnote.FilterGroups.First().FilterGroup.Label);

                Assert.Single(retrievedFootnote.FilterItems);
                Assert.Equal(releaseSubject.Subject.Filters[0].FilterGroups[0].FilterItems[0].Id, retrievedFootnote.FilterItems.First().FilterItem.Id);
                Assert.Equal(releaseSubject.Subject.Filters[0].FilterGroups[0].FilterItems[0].Label, retrievedFootnote.FilterItems.First().FilterItem.Label);

                Assert.Single(retrievedFootnote.Indicators);
                Assert.Equal(releaseSubject.Subject.IndicatorGroups[0].Indicators[0].Id, retrievedFootnote.Indicators.First().Indicator.Id);
                Assert.Equal(releaseSubject.Subject.IndicatorGroups[0].Indicators[0].Label, retrievedFootnote.Indicators.First().Indicator.Label);
            }
        }

        [Fact]
        public async Task GetFootnote_ReleaseNotFound()
        {
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseFootnote = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnote(_fixture.DefaultFootnote())
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseFootnote.AddAsync(releaseFootnote);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var invalidReleaseId = Guid.NewGuid();
                var result = await service.GetFootnote(
                    releaseId: invalidReleaseId,
                    footnoteId: releaseFootnote.FootnoteId);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetFootnote_ReleaseAndFootnoteNotRelated()
        {
            var (release, otherRelease) = _fixture.DefaultStatsRelease().GenerateList(2).ToTuple2();

            var releaseFootnote = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnote(_fixture.DefaultFootnote())
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseFootnote.AddAsync(releaseFootnote);

                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var result = await service.GetFootnote(otherRelease.Id, releaseFootnote.FootnoteId);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task DeleteFootnote()
        {
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubjects = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .WithFilters(_ => _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2))
                    .WithIndicatorGroups(_ => _fixture
                        .DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1))
                    .GenerateList(2))
                .GenerateList();

            var (subject1, subject2) = releaseSubjects.Select(rs => rs.Subject).ToTuple2();

            var releaseFootnote = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnote(_fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(subject1))
                    .WithFilters(subject2.Filters)
                    .WithFilterGroups(subject2.Filters[0].FilterGroups)
                    .WithFilterItems(subject2.Filters[0].FilterGroups[0].FilterItems)
                    .WithIndicators(subject2.IndicatorGroups[0].Indicators))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnote);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id,
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(
                    statisticsDbContext,
                    contentDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = await service.DeleteFootnote(
                    releaseId: release.Id,
                    footnoteId: releaseFootnote.FootnoteId);

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
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2)))
                .Generate();

            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(releaseSubject.Subject))
                    .GenerateList(3))
                .GenerateList(3);

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = await service.DeleteFootnote(
                    releaseId: release.Id,
                    footnoteId: releaseFootnotes[0].FootnoteId);

                VerifyAllMocks(dataBlockService);

                result.AssertRight();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var retrievedFootnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

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
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(3))
                    .WithIndicatorGroups(_fixture.DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1)))
                .Generate();

            var releaseFootnote = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnote(_fixture
                    .DefaultFootnote()
                    .WithOrder(1))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnote);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(
                    statisticsDbContext,
                    contentDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = await service.UpdateFootnote(
                    releaseId: release.Id,
                    footnoteId: releaseFootnote.FootnoteId,
                    "Updated footnote",
                    filterIds: SetOf(releaseSubject.Subject.Filters[0].Id),
                    filterGroupIds: SetOf(releaseSubject.Subject.Filters[1].FilterGroups[0].Id),
                    filterItemIds: SetOf(releaseSubject.Subject.Filters[2].FilterGroups[0].FilterItems[0].Id),
                    indicatorIds: SetOf(releaseSubject.Subject.IndicatorGroups[0].Indicators[0].Id),
                    subjectIds: SetOf(releaseSubject.Subject.Id));

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
                Assert.Equal(release.Id, savedReleaseFootnote.ReleaseId);
                Assert.Equal(releaseFootnote.FootnoteId, savedReleaseFootnote.FootnoteId);

                var savedSubjectFootnote = Assert.Single(statisticsDbContext.SubjectFootnote);
                Assert.Equal(releaseSubject.Subject.Id, savedSubjectFootnote.SubjectId);
                Assert.Equal(releaseFootnote.FootnoteId, savedSubjectFootnote.FootnoteId);

                var savedFilterFootnote = Assert.Single(statisticsDbContext.FilterFootnote);
                Assert.Equal(releaseSubject.Subject.Filters[0].Id, savedFilterFootnote.FilterId);
                Assert.Equal(releaseFootnote.FootnoteId, savedFilterFootnote.FootnoteId);

                var savedFilterGroupFootnote = Assert.Single(statisticsDbContext.FilterGroupFootnote);
                Assert.Equal(releaseSubject.Subject.Filters[1].FilterGroups[0].Id, savedFilterGroupFootnote.FilterGroupId);
                Assert.Equal(releaseFootnote.FootnoteId, savedFilterGroupFootnote.FootnoteId);

                var savedFilterItemFootnote = Assert.Single(statisticsDbContext.FilterItemFootnote);
                Assert.Equal(releaseSubject.Subject.Filters[2].FilterGroups[0].FilterItems[0].Id, savedFilterItemFootnote.FilterItemId);
                Assert.Equal(releaseFootnote.FootnoteId, savedFilterItemFootnote.FootnoteId);

                var savedIndicatorFootnote = Assert.Single(statisticsDbContext.IndicatorFootnote);
                Assert.Equal(releaseSubject.Subject.IndicatorGroups[0].Indicators[0].Id, savedIndicatorFootnote.IndicatorId);
                Assert.Equal(releaseFootnote.FootnoteId, savedIndicatorFootnote.FootnoteId);
            }
        }

        [Fact]
        public async Task UpdateFootnote_RemoveCriteria()
        {
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(3))
                    .WithIndicatorGroups(_fixture
                        .DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1)))
                .Generate();

            var releaseFootnote = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnote(_fixture
                    .DefaultFootnote()
                    .WithOrder(1))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnote);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(
                    statisticsDbContext,
                    contentDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = await service.UpdateFootnote(
                    releaseId: release.Id,
                    footnoteId: releaseFootnote.FootnoteId,
                    "Updated footnote",
                    filterIds: SetOf<Guid>(),
                    filterGroupIds: SetOf<Guid>(),
                    filterItemIds: SetOf<Guid>(),
                    indicatorIds: SetOf<Guid>(),
                    subjectIds: SetOf<Guid>());

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
                Assert.Equal(release.Id, savedReleaseFootnote.ReleaseId);
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
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2)))
                .Generate();

            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(releaseSubject.Subject))
                    .GenerateList(3))
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                // Create a request with identical footnotes but in a new order
                var request = new FootnotesUpdateRequest
                {
                    FootnoteIds = ListOf(
                        releaseFootnotes[2].FootnoteId,
                        releaseFootnotes[0].FootnoteId,
                        releaseFootnotes[1].FootnoteId
                    )
                };

                var result = await service.UpdateFootnotes(
                    release.Id,
                    request);

                VerifyAllMocks(dataBlockService);

                result.AssertRight();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var retrievedFootnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

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
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2)))
                .Generate();

            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(releaseSubject.Subject))
                    .GenerateList(2))
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                // Attempt to update the footnotes but use a different release id
                var result = await service.UpdateFootnotes(
                    Guid.NewGuid(),
                    new FootnotesUpdateRequest
                    {
                        FootnoteIds = ListOf(
                            releaseFootnotes[0].FootnoteId,
                            releaseFootnotes[1].FootnoteId
                        )
                    });

                result.AssertNotFound();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var retrievedFootnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

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
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2)))
                .Generate();

            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(releaseSubject.Subject))
                    .GenerateList(2))
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                // Request has the first footnote id missing
                var request = new FootnotesUpdateRequest
                {
                    FootnoteIds = ListOf(releaseFootnotes[1].FootnoteId)
                };

                var result = await service.UpdateFootnotes(
                    release.Id,
                    request);

                result.AssertBadRequest(FootnotesDifferFromReleaseFootnotes);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var retrievedFootnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

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
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(2)))
                .Generate();

            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(releaseSubject.Subject))
                    .GenerateList(2))
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                // Request has a footnote id not for this release
                var request = new FootnotesUpdateRequest
                {
                    FootnoteIds = ListOf(
                        releaseFootnotes[1].FootnoteId,
                        releaseFootnotes[0].FootnoteId,
                        Guid.NewGuid()
                    )
                };

                var result = await service.UpdateFootnotes(
                    release.Id,
                    request);

                result.AssertBadRequest(FootnotesDifferFromReleaseFootnotes);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var retrievedFootnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

                // Verify that the footnotes remain untouched
                Assert.Equal(2, retrievedFootnotes.Count);
                Assert.Equal("Footnote 0 :: Content", retrievedFootnotes[0].Content);
                Assert.Equal(0, retrievedFootnotes[0].Order);
                Assert.Equal("Footnote 1 :: Content", retrievedFootnotes[1].Content);
                Assert.Equal(1, retrievedFootnotes[1].Order);
            }
        }

        private static async Task SeedDatabase(
            string contextId,
            Release release,
            IReadOnlyList<Subject>? subjects = null,
            IReadOnlyList<ReleaseSubject>? releaseSubjects = null,
            IReadOnlyList<Footnote>? footnotes = null)
        {
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);

                if (!subjects.IsNullOrEmpty())
                {
                    await statisticsDbContext.Subject.AddRangeAsync(subjects!);
                }

                if (!releaseSubjects.IsNullOrEmpty())
                {
                    await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubjects!);
                }

                if (!footnotes.IsNullOrEmpty())
                {
                    await statisticsDbContext.Footnote.AddRangeAsync(footnotes!);
                }

                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }
        }

        private static async Task<Either<ActionResult, Footnote>> CreateFootnoteWithConfiguration(
            Guid releaseId,
            string contextId,
            string content = "",
            IReadOnlySet<Guid>? filterIds = null,
            IReadOnlySet<Guid>? filterGroupIds = null,
            IReadOnlySet<Guid>? filterItemIds = null,
            IReadOnlySet<Guid>? indicatorIds = null,
            IReadOnlySet<Guid>? subjectIds = null)
        {
            filterIds ??= SetOf<Guid>();
            filterGroupIds ??= SetOf<Guid>();
            filterItemIds ??= SetOf<Guid>();
            indicatorIds ??= SetOf<Guid>();
            subjectIds ??= SetOf<Guid>();

            var dataBlockService = new Mock<IDataBlockService>();
            dataBlockService
                .Setup(dbs => dbs.InvalidateCachedDataBlocks(releaseId))
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                FootnoteService footnoteService = SetupFootnoteService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                return await footnoteService.CreateFootnote(
                    releaseId,
                    content,
                    filterIds,
                    filterGroupIds,
                    filterItemIds,
                    indicatorIds,
                    subjectIds);
            }
        }

        private static async Task<Either<ActionResult, Footnote>> UpdateFootnoteWithConfiguration(
            Guid releaseId,
            Guid footnoteId,
            string contextId,
            string content = "",
            IReadOnlySet<Guid>? filterIds = null,
            IReadOnlySet<Guid>? filterGroupIds = null,
            IReadOnlySet<Guid>? filterItemIds = null,
            IReadOnlySet<Guid>? indicatorIds = null,
            IReadOnlySet<Guid>? subjectIds = null)
        {
            filterIds ??= SetOf<Guid>();
            filterGroupIds ??= SetOf<Guid>();
            filterItemIds ??= SetOf<Guid>();
            indicatorIds ??= SetOf<Guid>();
            subjectIds ??= SetOf<Guid>();

            var dataBlockService = new Mock<IDataBlockService>();
            dataBlockService
                .Setup(dbs => dbs.InvalidateCachedDataBlocks(releaseId))
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                FootnoteService footnoteService = SetupFootnoteService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                return await footnoteService.UpdateFootnote(
                    releaseId,
                    footnoteId,
                    content,
                    filterIds,
                    filterGroupIds,
                    filterItemIds,
                    indicatorIds,
                    subjectIds);
            }
        }

        private static FootnoteService SetupFootnoteService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IUserService? userService = null,
            IDataBlockService? dataBlockService = null,
            IReleaseSubjectRepository? releaseSubjectRepository = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null)
        {
            var contentContext = contentDbContext ?? new Mock<ContentDbContext>().Object;

            var footnoteRepository = new FootnoteRepository(statisticsDbContext);

            return new FootnoteService(
                statisticsDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentContext),
                userService ?? AlwaysTrueUserService().Object,
                dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
                footnoteRepository,
                releaseSubjectRepository ?? new ReleaseSubjectRepository(statisticsDbContext, footnoteRepository),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext)
            );
        }
    }
}

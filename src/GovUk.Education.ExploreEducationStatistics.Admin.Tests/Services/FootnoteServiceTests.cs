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
        public async Task CreateFootnote_SubjectNotLinkedToRelease_ReturnsFootnoteSpecificationsAreInvalidValidationResult()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release);

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf(Guid.NewGuid()));

            result.AssertBadRequest(FootnoteSpecificationsAreInvalid);
        }

        [Fact]
        public async Task CreateFootnote_SubjectIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            Subject subject = _fixture.DefaultSubject().Generate();
            ReleaseSubject releaseSubject = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf(subject.Id));

            result.AssertRight();
            Assert.Single(result.Right.Subjects);
            Assert.Equal(subject.Id, result.Right.Subjects.Single().SubjectId);
        }

        [Fact]
        public async Task CreateFootnote_FilterNotLinkedToRelease_ReturnsFootnoteSpecificationsAreInvalidValidationResult()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            Subject subject = _fixture.DefaultSubject().Generate();
            ReleaseSubject releaseSubject = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf(Guid.NewGuid()),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>());

            result.AssertBadRequest(FootnoteSpecificationsAreInvalid);
        }

        [Fact]
        public async Task CreateFootnote_FilterIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            Filter filter = _fixture.DefaultFilter().Generate();
            Subject subject = _fixture.DefaultSubject().WithFilters(new List<Filter>() { filter }).Generate();
            ReleaseSubject releaseSubject = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf(filter.Id),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>());

            result.AssertRight();
            Assert.Single(result.Right.Filters);
            Assert.Equal(filter.Id, result.Right.Filters.Single().FilterId);
        }

        [Fact]
        public async Task CreateFootnote_FilterGroupNotLinkedToRelease_ReturnsFootnoteSpecificationsAreInvalidValidationResult()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            Filter filter = _fixture.DefaultFilter().Generate();
            Subject subject = _fixture.DefaultSubject().WithFilters(new List<Filter>() { filter }).Generate();
            ReleaseSubject releaseSubject = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf<Guid>(),
                SetOf(Guid.NewGuid()),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>());

            result.AssertBadRequest(FootnoteSpecificationsAreInvalid);
        }

        [Fact]
        public async Task CreateFootnote_FilterGroupIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            FilterGroup filterGroup = _fixture.DefaultFilterGroup().Generate();
            Filter filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup>() { filterGroup }).Generate();
            Subject subject = _fixture.DefaultSubject().WithFilters(new List<Filter>() { filter }).Generate();
            ReleaseSubject releaseSubject = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf<Guid>(),
                SetOf(filterGroup.Id),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>());

            result.AssertRight();
            Assert.Single(result.Right.FilterGroups);
            Assert.Equal(filterGroup.Id, result.Right.FilterGroups.Single().FilterGroupId);
        }

        [Fact]
        public async Task CreateFootnote_FilterItemNotLinkedToRelease_ReturnsFootnoteSpecificationsAreInvalidValidationResult()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            FilterGroup filterGroup = _fixture.DefaultFilterGroup().Generate();
            Filter filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup>() { filterGroup }).Generate();
            Subject subject = _fixture.DefaultSubject().WithFilters(new List<Filter>() { filter }).Generate();
            ReleaseSubject releaseSubject = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf(Guid.NewGuid()),
                SetOf<Guid>(),
                SetOf<Guid>());

            result.AssertBadRequest(FootnoteSpecificationsAreInvalid);
        }

        [Fact]
        public async Task CreateFootnote_FilterItemIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            FilterItem filterItem = _fixture.DefaultFilterItem().Generate();
            FilterGroup filterGroup = _fixture.DefaultFilterGroup().WithFilterItems(new List<FilterItem>() { filterItem }).Generate();
            Filter filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup>() { filterGroup }).Generate();
            Subject subject = _fixture.DefaultSubject().WithFilters(new List<Filter>() { filter }).Generate();
            ReleaseSubject releaseSubject = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf(filterItem.Id),
                SetOf<Guid>(),
                SetOf<Guid>());

            result.AssertRight();
            Assert.Single(result.Right.FilterItems);
            Assert.Equal(filterItem.Id, result.Right.FilterItems.Single().FilterItemId);
        }

        [Fact]
        public async Task CreateFootnote_IndicatorNotLinkedToRelease_ReturnsFootnoteSpecificationsAreInvalidValidationResult()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            Subject subject = _fixture.DefaultSubject().Generate();
            ReleaseSubject releaseSubject = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf(Guid.NewGuid()),
                SetOf<Guid>());

            result.AssertBadRequest(FootnoteSpecificationsAreInvalid);
        }

        [Fact]
        public async Task CreateFootnote_IndicatorIsLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            Indicator indicator = _fixture.DefaultIndicator().Generate();
            IndicatorGroup indicatorGroup = _fixture.DefaultIndicatorGroup().WithIndicators(new List<Indicator>() { indicator }).Generate();
            Subject subject = _fixture.DefaultSubject().WithIndicatorGroups(new List<IndicatorGroup>() { indicatorGroup }).Generate();
            ReleaseSubject releaseSubject = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject), releaseSubjects: ListOf(releaseSubject));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf<Guid>(),
                SetOf(indicator.Id),
                SetOf<Guid>());

            result.AssertRight();
            Assert.Single(result.Right.Indicators);
            Assert.Equal(indicator.Id, result.Right.Indicators.Single().IndicatorId);
        }

        [Fact]
        public async Task CreateFootnote_WithFiltersAndIndicatorsAndSubjectsWhichAreLinkedToRelease_CreatesFootnoteWithCorrectLinks()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();
            FilterItem filterItem = _fixture.DefaultFilterItem().Generate();
            FilterGroup filterGroup = _fixture.DefaultFilterGroup().WithFilterItems(new List<FilterItem>() { filterItem }).Generate();
            Filter filter = _fixture.DefaultFilter().WithFilterGroups(new List<FilterGroup>() { filterGroup }).Generate();
            Indicator indicator = _fixture.DefaultIndicator().Generate();
            IndicatorGroup indicatorGroup = _fixture.DefaultIndicatorGroup().WithIndicators(new List<Indicator>() { indicator }).Generate();
            Subject subject1 = _fixture.DefaultSubject().WithFilters(new List<Filter>() { filter }).WithIndicatorGroups(new List<IndicatorGroup>() { indicatorGroup }).Generate();
            Subject subject2 = _fixture.DefaultSubject().Generate();
            ReleaseSubject releaseSubject1 = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject1).Generate();
            ReleaseSubject releaseSubject2 = _fixture.DefaultReleaseSubject().WithRelease(release).WithSubject(subject2).Generate();

            var contextId = Guid.NewGuid().ToString();

            await SeedDatabase(contextId, release, subjects: ListOf(subject1, subject2), releaseSubjects: ListOf(releaseSubject1, releaseSubject2));

            Either<ActionResult, Footnote> result = await CreateFootnoteWithConfiguration(
                release.Id,
                contextId,
                string.Empty,
                SetOf(filter.Id),
                SetOf(filterGroup.Id),
                SetOf(filterItem.Id),
                SetOf(indicator.Id),
                SetOf(subject2.Id));

            result.AssertRight();
            Assert.Single(result.Right.Subjects);
            Assert.Equal(subject2.Id, result.Right.Subjects.Single().SubjectId);
            Assert.Single(result.Right.Filters);
            Assert.Equal(filter.Id, result.Right.Filters.Single().FilterId);
            Assert.Single(result.Right.FilterGroups);
            Assert.Equal(filterGroup.Id, result.Right.FilterGroups.Single().FilterGroupId);
            Assert.Single(result.Right.FilterItems);
            Assert.Equal(filterItem.Id, result.Right.FilterItems.Single().FilterItemId);
            Assert.Single(result.Right.Indicators);
            Assert.Equal(indicator.Id, result.Right.Indicators.Single().IndicatorId);
        }

        [Fact]
        public async Task CreateFootnote()
        {
            Release release = _fixture.DefaultStatsRelease().Generate();

            IEnumerable<Subject> subjects = _fixture
                .DefaultSubject()
                .ForIndex(1, s => s
                    .SetFilters(_fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                        .Generate(3))
                    .SetIndicatorGroups(_fixture.DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1)))
                .Generate(2);

            List<ReleaseSubject> releaseSubjects = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(subjects)
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.Subject.AddRangeAsync(subjects);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubjects);
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

            // Footnote can be applied to the entire dataset with a subject link
            // or to specific data with links to filters/filter groups/filter items/indicators
            // Get the target references which the created footnote will be applied to
            var subject = releaseSubjects[0].Subject;
            var filter = releaseSubjects[1].Subject
                .Filters.First();
            var filterGroup = releaseSubjects[1].Subject
                .Filters[0]
                .FilterGroups.First();
            var filterItem = releaseSubjects[1].Subject
                .Filters[0]
                .FilterGroups[0]
                .FilterItems.First();
            var indicator = releaseSubjects[1].Subject.IndicatorGroups.First().Indicators.First();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = (await service.CreateFootnote(
                    releaseId: release.Id,
                    "Test footnote",
                    filterIds: SetOf(filter.Id),
                    filterGroupIds: SetOf(filterGroup.Id),
                    filterItemIds: SetOf(filterItem.Id),
                    indicatorIds: SetOf(indicator.Id),
                    subjectIds: SetOf(subject.Id)
                )).AssertRight();

                VerifyAllMocks(dataBlockService);

                Assert.Equal("Test footnote", result.Content);
                Assert.Equal(0, result.Order);

                var releaseFootnote = Assert.Single(result.Releases);
                Assert.Equal(release.Id, releaseFootnote.ReleaseId);

                var subjectFootnote = Assert.Single(result.Subjects);
                Assert.Equal(subject.Id, subjectFootnote.SubjectId);

                var filterFootnote = Assert.Single(result.Filters);
                Assert.Equal(filter.Id, filterFootnote.FilterId);

                var filterGroupFootnote = Assert.Single(result.FilterGroups);
                Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);

                var filterItemFootnote = Assert.Single(result.FilterItems);
                Assert.Equal(filterItem.Id, filterItemFootnote.FilterItemId);

                var indicatorFootnote = Assert.Single(result.Indicators);
                Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var footnotes = await statisticsDbContext.Footnote
                    .Include(f => f.Releases)
                    .Include(f => f.Subjects)
                    .Include(f => f.Filters)
                    .Include(f => f.FilterGroups)
                    .Include(f => f.FilterItems)
                    .Include(f => f.Indicators)
                    .ToListAsync();

                var savedFootnote = Assert.Single(footnotes);

                Assert.Equal("Test footnote", savedFootnote.Content);
                Assert.Equal(0, savedFootnote.Order);

                var releaseFootnote = Assert.Single(savedFootnote.Releases);
                Assert.Equal(release.Id, releaseFootnote.ReleaseId);

                var subjectFootnote = Assert.Single(savedFootnote.Subjects);
                Assert.Equal(subject.Id, subjectFootnote.SubjectId);

                var filterFootnote = Assert.Single(savedFootnote.Filters);
                Assert.Equal(filter.Id, filterFootnote.FilterId);

                var filterGroupFootnote = Assert.Single(savedFootnote.FilterGroups);
                Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);

                var filterItemFootnote = Assert.Single(savedFootnote.FilterItems);
                Assert.Equal(filterItem.Id, filterItemFootnote.FilterItemId);

                var indicatorFootnote = Assert.Single(savedFootnote.Indicators);
                Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
            }
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

                Assert.Equal("Content of Footnote 0", retrievedFootnotes[0].Content);
                Assert.Equal(0, retrievedFootnotes[0].Order);
                Assert.Equal("Content of Footnote 1", retrievedFootnotes[1].Content);
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
                Assert.Equal("Content of Footnote 0", retrievedFootnote.Content);

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
        public async Task CopyFootnotes()
        {
            var (release, amendment) = _fixture.DefaultStatsRelease().GenerateList(2).ToTuple2();

            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(1))
                    .WithIndicatorGroups(_fixture.DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1)))
                .Generate();

            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetSubjects(ListOf(releaseSubject.Subject))
                        .SetFilters(releaseSubject.Subject.Filters)
                        .SetFilterGroups(releaseSubject.Subject.Filters[0].FilterGroups)
                        .SetFilterItems(releaseSubject.Subject.Filters[0].FilterGroups[0].FilterItems)
                        .SetIndicators(releaseSubject.Subject.IndicatorGroups[0].Indicators))
                    .ForIndex(1, s => s
                        .SetSubjects(ListOf(releaseSubject.Subject)))
                    .GenerateList())
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await statisticsDbContext.Release.AddRangeAsync(release, amendment);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.ReleaseFootnote.AddRangeAsync(releaseFootnotes);

                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.Releases.AddRangeAsync(new Content.Model.Release
                {
                    Id = release.Id
                },
                    new Content.Model.Release
                    {
                        Id = amendment.Id
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(
                    statisticsDbContext,
                    contentDbContext);

                var result =
                    await service.CopyFootnotes(release.Id, amendment.Id);

                result.AssertRight();
                Assert.Equal(2, result.Right.Count);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var newFootnotesFromDb = statisticsDbContext
                    .Footnote
                    .Include(f => f.Filters)
                    .Include(f => f.FilterGroups)
                    .Include(f => f.FilterItems)
                    .Include(f => f.Releases)
                    .Include(f => f.Subjects)
                    .Include(f => f.Indicators)
                    .Where(f => f.Releases.FirstOrDefault(r => r.ReleaseId == amendment.Id) != null)
                    .OrderBy(f => f.Content)
                    .ToList();

                Assert.Equal(2, newFootnotesFromDb.Count);
                AssertFootnoteDetailsCopiedCorrectly(releaseFootnotes[0].Footnote, newFootnotesFromDb[0]);
                AssertFootnoteDetailsCopiedCorrectly(releaseFootnotes[1].Footnote, newFootnotesFromDb[1]);
            }

            void AssertFootnoteDetailsCopiedCorrectly(Footnote originalFootnote, Footnote newFootnote)
            {
                Assert.Equal(originalFootnote.Content, newFootnote.Content);
                Assert.Equal(originalFootnote.Order, newFootnote.Order);

                Assert.Equal(originalFootnote
                        .Filters
                        .SelectNullSafe(f => f.FilterId),
                    newFootnote
                        .Filters
                        .SelectNullSafe(f => f.FilterId));

                Assert.Equal(
                    originalFootnote
                        .FilterGroups
                        .SelectNullSafe(f => f.FilterGroupId),
                    newFootnote
                        .FilterGroups
                        .SelectNullSafe(f => f.FilterGroupId));

                Assert.Equal(
                    originalFootnote
                        .FilterItems
                        .SelectNullSafe(f => f.FilterItemId),
                    newFootnote
                        .FilterItems
                        .SelectNullSafe(f => f.FilterItemId));

                Assert.Equal(
                    originalFootnote
                        .Subjects
                        .SelectNullSafe(f => f.SubjectId),
                    newFootnote
                        .Subjects
                        .SelectNullSafe(f => f.SubjectId));

                Assert.Equal(
                    originalFootnote
                        .Indicators
                        .SelectNullSafe(f => f.IndicatorId),
                    newFootnote
                        .Indicators
                        .SelectNullSafe(f => f.IndicatorId));
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
                Assert.Equal("Content of Footnote 1", retrievedFootnotes[0].Content);
                Assert.Equal(0, retrievedFootnotes[0].Order);
                Assert.Equal("Content of Footnote 2", retrievedFootnotes[1].Content);
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
                Assert.Equal("Content of Footnote 2", retrievedFootnotes[0].Content);
                Assert.Equal(0, retrievedFootnotes[0].Order);
                Assert.Equal("Content of Footnote 0", retrievedFootnotes[1].Content);
                Assert.Equal(1, retrievedFootnotes[1].Order);
                Assert.Equal("Content of Footnote 1", retrievedFootnotes[2].Content);
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
                Assert.Equal("Content of Footnote 0", retrievedFootnotes[0].Content);
                Assert.Equal(0, retrievedFootnotes[0].Order);
                Assert.Equal("Content of Footnote 1", retrievedFootnotes[1].Content);
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
                Assert.Equal("Content of Footnote 0", retrievedFootnotes[0].Content);
                Assert.Equal(0, retrievedFootnotes[0].Order);
                Assert.Equal("Content of Footnote 1", retrievedFootnotes[1].Content);
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
                Assert.Equal("Content of Footnote 0", retrievedFootnotes[0].Content);
                Assert.Equal(0, retrievedFootnotes[0].Order);
                Assert.Equal("Content of Footnote 1", retrievedFootnotes[1].Content);
                Assert.Equal(1, retrievedFootnotes[1].Order);
            }
        }

        private static async Task SeedDatabase(string contextId, Release release, IReadOnlyList<Subject>? subjects = null, IReadOnlyList<ReleaseSubject> releaseSubjects = null)
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
            string content,
            IReadOnlySet<Guid> filterIds,
            IReadOnlySet<Guid> filterGroupIds,
            IReadOnlySet<Guid> filterItemIds,
            IReadOnlySet<Guid> indicatorIds,
            IReadOnlySet<Guid> subjectIds)
        {
            var dataBlockService = new Mock<IDataBlockService>();
            dataBlockService
                .Setup(dbs => dbs.InvalidateCachedDataBlocks(releaseId))
                .Verifiable();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                FootnoteService footnoteService = SetupFootnoteService(contentDbContext: contentDbContext, statisticsDbContext: statisticsDbContext, dataBlockService: dataBlockService.Object);

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

        private static FootnoteService SetupFootnoteService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IUserService? userService = null,
            IDataBlockService? dataBlockService = null,
            IFootnoteRepository? footnoteRepository = null,
            IReleaseSubjectRepository? releaseSubjectRepository = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null)
        {
            var contentContext = contentDbContext ?? new Mock<ContentDbContext>().Object;

            return new FootnoteService(
                statisticsDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentContext),
                userService ?? AlwaysTrueUserService().Object,
                dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
                footnoteRepository ?? new FootnoteRepository(statisticsDbContext),
                releaseSubjectRepository ?? new ReleaseSubjectRepository(statisticsDbContext, footnoteRepository),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext)
            );
        }
    }
}

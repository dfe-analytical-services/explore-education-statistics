#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository
{
    public class FootnoteRepositoryTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task GetFilteredFootnotes()
        {
            var release = _fixture.DefaultStatsRelease().Generate();
            
            var releaseSubjects = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .ForIndex(0, s => s
                        .SetFilters(_fixture
                            .DefaultFilter(filterGroupCount: 1, filterItemCount: 1).GenerateList(3))
                        .SetIndicatorGroups(_fixture
                            .DefaultIndicatorGroup()
                            .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                            .GenerateList(1)))
                    .GenerateList(2))
                .GenerateList();

            var subject1 = releaseSubjects[0].Subject;
            var subject2 = releaseSubjects[1].Subject;

            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applies to subject 1")
                        .SetSubjects(ListOf(subject1)))
                    .ForIndex(1, s => s
                        .SetContent("Applies to subject 1 filter 1")
                        .SetFilters(ListOf(subject1.Filters[0])))
                    .ForIndex(2, s => s
                        .SetContent("Applies to subject 1 filter 2 group 1")
                        .SetFilterGroups(ListOf(subject1.Filters[1].FilterGroups[0])))
                    .ForIndex(3, s => s
                        .SetContent("Applies to subject 1 filter 3 group 1 item 1")
                        .SetFilterItems(ListOf(subject1.Filters[2].FilterGroups[0].FilterItems[0])))
                    .ForIndex(4, s => s
                        .SetContent("Applies to subject 1 indicator 1")
                        .SetIndicators(ListOf(subject1.IndicatorGroups[0].Indicators[0])))
            
                    // Footnote applies to subject 1 filter 1
                    // and subject 1 filter 2 group 1
                    // and subject 1 filter 3 group 1 item 1
                    // and subject 1 indicator 1
                    .ForIndex(5, s => s
                        .SetContent("Applies to multiple attributes of subject 1")
                        .SetFilters(ListOf(subject1.Filters[0]))
                        .SetFilterGroups(ListOf(subject1.Filters[1].FilterGroups[0]))
                        .SetFilterItems(ListOf(subject1.Filters[2].FilterGroups[0].FilterItems[0]))
                        .SetIndicators(ListOf(subject1.IndicatorGroups[0].Indicators[0])))
                    .ForIndex(6, s => s
                        .SetContent("Applies to subject 2")
                        .SetSubjects(ListOf(subject2)))
                    .GenerateList())
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Filter.AddRangeAsync(subject1.Filters);
                await context.IndicatorGroup.AddAsync(subject1.IndicatorGroups[0]);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                //  Get the footnotes applying to subject 1 or any of its filter items or indicators
                var filter1Group1Item1Id = subject1.Filters[0].FilterGroups[0].FilterItems[0].Id;
                var filter2Group1Item1Id = subject1.Filters[1].FilterGroups[0].FilterItems[0].Id;
                var filter3Group1Item1Id = subject1.Filters[2].FilterGroups[0].FilterItems[0].Id;
                var indicatorGroup1Item1Id = subject1.IndicatorGroups[0].Indicators[0].Id;

                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf(filter1Group1Item1Id, filter2Group1Item1Id, filter3Group1Item1Id),
                    indicatorIds: ListOf(indicatorGroup1Item1Id));

                // Check that only footnotes applying to subject 1 or any of its filter items or indicators are returned
                Assert.Equal(6, results.Count);

                // Footnote applies to the requested subject
                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to subject 1", results[0].Content);

                // Footnote applies to a requested filter item via its filter
                Assert.Equal(releaseFootnotes[1].FootnoteId, results[1].Id);
                Assert.Equal("Applies to subject 1 filter 1", results[1].Content);

                // Footnote applies to a requested filter item via its filter group
                Assert.Equal(releaseFootnotes[2].FootnoteId, results[2].Id);
                Assert.Equal("Applies to subject 1 filter 2 group 1", results[2].Content);

                // Footnote applies to a requested filter item
                Assert.Equal(releaseFootnotes[3].FootnoteId, results[3].Id);
                Assert.Equal("Applies to subject 1 filter 3 group 1 item 1", results[3].Content);

                // Footnote applies to a requested indicator 
                Assert.Equal(releaseFootnotes[4].FootnoteId, results[4].Id);
                Assert.Equal("Applies to subject 1 indicator 1", results[4].Content);

                // Footnote applies to a various criteria related to the requested filter items and indicators
                Assert.Equal(releaseFootnotes[5].FootnoteId, results[5].Id);
                Assert.Equal("Applies to multiple attributes of subject 1", results[5].Content);
            }
        }

        [Fact]
        public async Task GetFilteredFootnotes_FilterBySubject()
        {
            var release = _fixture.DefaultStatsRelease().Generate();
            
            var releaseSubjects = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .WithFilters(_ => _fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                        .Generate(1))
                    .WithIndicatorGroups(_ => _fixture
                        .DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .GenerateList(1))
                    .GenerateList(2))
                .GenerateList();

            var (subject1, subject2) = GetSubjectsTuple2(releaseSubjects);
            
            // Set up a release with footnotes that apply to either subject 1 or subject 2 but not both
            var releaseFootnotes = _fixture
                .DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applies to subject 1")
                        .SetSubjects(ListOf(subject1)))
                    .ForIndex(1, s => s
                        .SetContent("Applies to subject 2")
                        .SetSubjects(ListOf(subject2)))
                    .ForIndex(2, s => s
                        .SetContent("Applies to subject 1 filter 1")
                        .SetFilters(subject1.Filters))
                    .ForIndex(3, s => s
                        .SetContent("Applies to subject 2 filter 1")
                        .SetFilters(subject2.Filters))
                    .ForIndex(4, s => s
                        .SetContent("Applies to subject 1 filter 1 group 1")
                        .SetFilterGroups(ListOf(subject1.Filters[0].FilterGroups[0])))
                    .ForIndex(5, s => s
                        .SetContent("Applies to subject 2 filter 1 group 1")
                        .SetFilterGroups(ListOf(subject2.Filters[0].FilterGroups[0])))
                    .ForIndex(6, s => s
                        .SetContent("Applies to subject 1 filter 1 group 1 item 1")
                        .SetFilterItems(ListOf(subject1.Filters[0].FilterGroups[0].FilterItems[0])))
                    .ForIndex(7, s => s
                        .SetContent("Applies to subject 2 filter 1 group 1 item 1")
                        .SetFilterItems(ListOf(subject2.Filters[0].FilterGroups[0].FilterItems[0])))
                    .ForIndex(8, s => s
                        .SetContent("Applies to subject 1 indicator 1")
                        .SetIndicators(ListOf(subject1.IndicatorGroups[0].Indicators[0])))
                    .ForIndex(9, s => s
                        .SetContent("Applies to subject 2 indicator 1")
                        .SetIndicators(ListOf(subject2.IndicatorGroups[0].Indicators[0])))
                    .GenerateList())
                .GenerateList();
            
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.IndicatorGroup.AddRangeAsync(subject1.IndicatorGroups[0], subject2.IndicatorGroups[0]);
                await context.Release.AddAsync(release);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
        
                // This test makes sure that all footnotes which apply exclusively to subjects other than the one
                // requested are excluded
        
                //  Get the footnotes applying to subject 1 or any of its filter items or indicators
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf(subject1.Filters[0].FilterGroups[0].FilterItems[0].Id),
                    indicatorIds: ListOf(subject1.IndicatorGroups[0].Indicators[0].Id));
        
                // Check that only footnotes applying to subject 1 or any of its filter items or indicators are returned
                // and that all footnotes applying to subject 2 are excluded
                Assert.Equal(5, results.Count);
        
                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to subject 1", results[0].Content);
        
                Assert.Equal(releaseFootnotes[2].FootnoteId, results[1].Id);
                Assert.Equal("Applies to subject 1 filter 1", results[1].Content);
        
                // Footnote applies to a requested filter item via its filter group
                Assert.Equal(releaseFootnotes[4].FootnoteId, results[2].Id);
                Assert.Equal("Applies to subject 1 filter 1 group 1", results[2].Content);
        
                // Footnote applies to a requested filter item via its filter group
                Assert.Equal(releaseFootnotes[6].FootnoteId, results[3].Id);
                Assert.Equal("Applies to subject 1 filter 1 group 1 item 1", results[3].Content);
        
                Assert.Equal(releaseFootnotes[8].FootnoteId, results[4].Id);
                Assert.Equal("Applies to subject 1 indicator 1", results[4].Content);
            }
        }
        
        [Fact]
        public async Task GetFilteredFootnotes_FilterByFiltersAndIndicators()
        {
            var release = _fixture.DefaultStatsRelease().Generate();
            
            var releaseSubject = _fixture.DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture.DefaultSubject()
                    .WithFilters(_fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
                        .GenerateList(1))
                    .WithIndicatorGroups(_fixture
                        .DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(2))
                        .GenerateList(1)))
                .Generate();

            var filter = releaseSubject.Subject.Filters[0];
            var indicatorGroup = releaseSubject.Subject.IndicatorGroups[0];
            
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes( _fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applies to filter 1")
                        .SetFilters(ListOf(filter)))
                    .ForIndex(1, s => s
                        .SetContent("Applies to filter 1 group 1")
                        .SetFilterGroups(ListOf(filter.FilterGroups[0])))
                    .ForIndex(2, s => s
                        .SetContent("Applies to filter item 1")
                        .SetFilterItems(ListOf(filter.FilterGroups[0].FilterItems[0])))
                    .ForIndex(3, s => s
                        .SetContent("Applies to filter item 2")
                        .SetFilterItems(ListOf(filter.FilterGroups[0].FilterItems[1])))
                    .ForIndex(4, s => s
                        .SetContent("Applies to indicator 1")
                        .SetIndicators(ListOf(indicatorGroup.Indicators[0])))
                    .ForIndex(5, s => s
                        .SetContent("Applies to indicator 2")
                        .SetIndicators(ListOf(indicatorGroup.Indicators[1])))
                    .ForIndex(6, s => s
                        .SetContent("Applies to filter 1 and indicator 1")
                        .SetFilters(ListOf(filter))
                        .SetIndicators(ListOf(indicatorGroup.Indicators[0])))
                    .ForIndex(7, s => s
                        .SetContent("Applies to filter 1 and indicator 2")
                        .SetFilters(ListOf(filter))
                        .SetIndicators(ListOf(indicatorGroup.Indicators[1])))
                    .ForIndex(8, s => s
                        .SetContent("Applies to filter 1 group 1 and indicator 1")
                        .SetFilterGroups(ListOf(filter.FilterGroups[0]))
                        .SetIndicators(ListOf(indicatorGroup.Indicators[0])))
                    .ForIndex(9, s => s
                        .SetContent("Applies to filter 1 group 1 and indicator 2")
                        .SetFilterGroups(ListOf(filter.FilterGroups[0]))
                        .SetIndicators(ListOf(indicatorGroup.Indicators[1])))
                    .ForIndex(10, s => s
                        .SetContent("Applies to filter item 1 and indicator 1")
                        .SetFilterItems(ListOf(filter.FilterGroups[0].FilterItems[0]))
                        .SetIndicators(ListOf(indicatorGroup.Indicators[0])))
                    .ForIndex(11, s => s
                        .SetContent("Applies to filter item 1 and indicator 2")
                        .SetFilterItems(ListOf(filter.FilterGroups[0].FilterItems[0]))
                        .SetIndicators(ListOf(indicatorGroup.Indicators[1])))
                    .GenerateList())
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Filter.AddAsync(filter);
                await context.IndicatorGroup.AddAsync(indicatorGroup);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubject);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
        
                //  Get the footnotes applying to filter item 1 and indicator 1
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: releaseSubject.SubjectId,
                    filterItemIds: ListOf(filter.FilterGroups[0].FilterItems[0].Id),
                    indicatorIds: ListOf(indicatorGroup.Indicators[0].Id));
        
                // Check that only footnotes applying to filter item 1, or indicator 1
                // are returned.
        
                // Any footnotes applying to filter item 2 or only to indicator 2 should be excluded
                Assert.Equal(7, results.Count);
        
                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to filter 1", results[0].Content);
        
                Assert.Equal(releaseFootnotes[1].FootnoteId, results[1].Id);
                Assert.Equal("Applies to filter 1 group 1", results[1].Content);
        
                Assert.Equal(releaseFootnotes[2].FootnoteId, results[2].Id);
                Assert.Equal("Applies to filter item 1", results[2].Content);
        
                Assert.Equal(releaseFootnotes[4].FootnoteId, results[3].Id);
                Assert.Equal("Applies to indicator 1", results[3].Content);
        
                Assert.Equal(releaseFootnotes[6].FootnoteId, results[4].Id);
                Assert.Equal("Applies to filter 1 and indicator 1", results[4].Content);
        
                Assert.Equal(releaseFootnotes[8].FootnoteId, results[5].Id);
                Assert.Equal("Applies to filter 1 group 1 and indicator 1", results[5].Content);
        
                Assert.Equal(releaseFootnotes[10].FootnoteId, results[6].Id);
                Assert.Equal("Applies to filter item 1 and indicator 1", results[6].Content);
            }
        }
        
        [Fact]
        public async Task GetFilteredFootnotes_FilterByFilterItems()
        {
            var release = _fixture.DefaultStatsRelease().Generate();
            
            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture.DefaultSubject()
                    .WithFilters(_fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 3)
                        .GenerateList(3)))
                .Generate();

            var (filter1, filter2, filter3) = releaseSubject.Subject.Filters.ToTuple3();
            
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applies to filter 1")
                        .SetFilters(ListOf(filter1)))
                    .ForIndex(1, s => s
                        .SetContent("Applies to filter 3")
                        .SetFilters(ListOf(filter3)))
                    .ForIndex(2, s => s
                        .SetContent("Applies to filter 1 and filter 3")
                        .SetFilters(ListOf(filter1, filter3)))
                    .ForIndex(3, s => s
                        .SetContent("Applies to filter 1 and filter 3 group 1")
                        .SetFilters(ListOf(filter1))
                        .SetFilterGroups(ListOf(filter3.FilterGroups[0])))
                    .ForIndex(4, s => s
                        .SetContent("Applies to filter 3 and filter 1 group 1")
                        .SetFilters(ListOf(filter3))
                        .SetFilterGroups(ListOf(filter1.FilterGroups[0])))
                    .ForIndex(5, s => s
                        .SetContent("Applies to filter 1 and filter 3 group item 1")
                        .SetFilters(ListOf(filter1))
                        .SetFilterItems(ListOf(filter3.FilterGroups[0].FilterItems[0])))
                    .ForIndex(6, s => s
                        .SetContent("Applies to filter 3 and filter 1 group item 1")
                        .SetFilters(ListOf(filter3))
                        .SetFilterItems(ListOf(filter1.FilterGroups[0].FilterItems[0])))
                    .ForIndex(7, s => s
                        .SetContent("Applies to filter 1 group 1")
                        .SetFilterGroups(ListOf(filter1.FilterGroups[0])))
                    .ForIndex(8, s => s
                        .SetContent("Applies to filter 3 group 1")
                        .SetFilterGroups(ListOf(filter3.FilterGroups[0])))
                    .ForIndex(9, s => s
                        .SetContent("Applies to filter 1 group 1 and filter 1 group 1 item 1")
                        .SetFilterGroups(ListOf(filter1.FilterGroups[0]))
                        .SetFilterItems(ListOf(filter1.FilterGroups[0].FilterItems[0])))
                    .ForIndex(10, s => s
                        .SetContent("Applies to filter 1 group 1 and filter 3 group 1 item 1")
                        .SetFilterGroups(ListOf(filter1.FilterGroups[0]))
                        .SetFilterItems(ListOf(filter3.FilterGroups[0].FilterItems[0])))
                    .ForIndex(11, s => s
                        .SetContent("Applies to filter 3 group 1 and filter 1 group 1 item 1")
                        .SetFilterGroups(ListOf(filter3.FilterGroups[0]))
                        .SetFilterItems(ListOf(filter1.FilterGroups[0].FilterItems[0])))
                    .ForIndex(12, s => s
                        .SetContent("Applies to filter 3 group 1 and filter 3 group 1 item 1")
                        .SetFilterGroups(ListOf(filter3.FilterGroups[0]))
                        .SetFilterItems(ListOf(filter3.FilterGroups[0].FilterItems[0])))
                    .ForIndex(13, s => s
                        .SetContent("Applies to filter 1 group 1 item 1")
                        .SetFilterItems(ListOf(filter1.FilterGroups[0].FilterItems[0])))
                    .ForIndex(14, s => s
                        .SetContent("Applies to filter 1 group 1 item 3")
                        .SetFilterItems(ListOf(filter1.FilterGroups[0].FilterItems[2])))
                    .ForIndex(15, s => s
                        .SetContent("Applies to filter 3 group 1 item 1")
                        .SetFilterItems(ListOf(filter3.FilterGroups[0].FilterItems[0])))
                    .ForIndex(16, s => s
                        .SetContent("Applies to filter 1 group 1 item 1 and filter 1 group 1 item 3")
                        .SetFilterItems(ListOf(filter1.FilterGroups[0].FilterItems[0], filter1.FilterGroups[0].FilterItems[2])))
                    .GenerateList())
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Filter.AddRangeAsync(filter1, filter2, filter3);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubject);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
        
                // Get the footnotes applying to filter 1 group 1 item 1 or filter 1 group 1 item 2
                // or the filter item id's of filter 2
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: releaseSubject.SubjectId,
                    filterItemIds: ListOf(filter1.FilterGroups[0].FilterItems[0].Id,
                        filter1.FilterGroups[0].FilterItems[1].Id,
                        filter2.FilterGroups[0].FilterItems[0].Id,
                        filter2.FilterGroups[0].FilterItems[1].Id),
                    indicatorIds: ListOf<Guid>()
                );
        
                // Check that only footnotes applying to filter 1 group 1 item 1 are returned.
        
                // No footnotes apply to filter 1 group 1 item 2 or to the filter items of filter 2 but
                // including their additional id's in the parameter shouldn't alter the result.
        
                // The footnotes which apply only to filter 3, filter 3 group 1 item 1, or filter 3 group 1 item 1
                // should be excluded.
                Assert.Equal(12, results.Count);
        
                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to filter 1", results[0].Content);
        
                Assert.Equal(releaseFootnotes[2].FootnoteId, results[1].Id);
                Assert.Equal("Applies to filter 1 and filter 3", results[1].Content);
        
                Assert.Equal(releaseFootnotes[3].FootnoteId, results[2].Id);
                Assert.Equal("Applies to filter 1 and filter 3 group 1", results[2].Content);
        
                Assert.Equal(releaseFootnotes[4].FootnoteId, results[3].Id);
                Assert.Equal("Applies to filter 3 and filter 1 group 1", results[3].Content);
        
                Assert.Equal(releaseFootnotes[5].FootnoteId, results[4].Id);
                Assert.Equal("Applies to filter 1 and filter 3 group item 1", results[4].Content);
        
                Assert.Equal(releaseFootnotes[6].FootnoteId, results[5].Id);
                Assert.Equal("Applies to filter 3 and filter 1 group item 1", results[5].Content);
        
                Assert.Equal(releaseFootnotes[7].FootnoteId, results[6].Id);
                Assert.Equal("Applies to filter 1 group 1", results[6].Content);
        
                Assert.Equal(releaseFootnotes[9].FootnoteId, results[7].Id);
                Assert.Equal("Applies to filter 1 group 1 and filter 1 group 1 item 1", results[7].Content);
        
                Assert.Equal(releaseFootnotes[10].FootnoteId, results[8].Id);
                Assert.Equal("Applies to filter 1 group 1 and filter 3 group 1 item 1", results[8].Content);
        
                Assert.Equal(releaseFootnotes[11].FootnoteId, results[9].Id);
                Assert.Equal("Applies to filter 3 group 1 and filter 1 group 1 item 1", results[9].Content);
        
                Assert.Equal(releaseFootnotes[13].FootnoteId, results[10].Id);
                Assert.Equal("Applies to filter 1 group 1 item 1", results[10].Content);
        
                Assert.Equal(releaseFootnotes[16].FootnoteId, results[11].Id);
                Assert.Equal("Applies to filter 1 group 1 item 1 and filter 1 group 1 item 3", results[11].Content);
            }
        }
        
        [Fact]
        public async Task GetFilteredFootnotes_FilterByIndicators()
        {
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubject = _fixture.DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture
                    .DefaultSubject()
                    .WithIndicatorGroups(_fixture
                        .DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(3))
                        .GenerateList(1)))
                .Generate();

            var indicatorGroup = releaseSubject.Subject.IndicatorGroups[0];
            
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applies to indicator 1")
                        .SetIndicators(ListOf(indicatorGroup.Indicators[0])))
                    .ForIndex(1, s => s
                        .SetContent("Applies to indicator 3")
                        .SetIndicators(ListOf(indicatorGroup.Indicators[2])))
                    .ForIndex(2, s => s
                        .SetContent("Applies to indicator 1 and indicator 3")
                        .SetIndicators(ListOf(indicatorGroup.Indicators[0], indicatorGroup.Indicators[2])))
                    .GenerateList())
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.IndicatorGroup.AddAsync(indicatorGroup);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubject);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
        
                //  Get the footnotes applying to indicator 1 or indicator 2
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: releaseSubject.SubjectId,
                    filterItemIds: ListOf<Guid>(),
                    indicatorIds: ListOf(indicatorGroup.Indicators[0].Id,
                        indicatorGroup.Indicators[1].Id)
                );
        
                // Check that only footnotes applying to indicator 1 are returned.
                // No footnotes apply to indicator 2 but including its id in the parameter shouldn't alter the result.
                // The footnote which applies only to indicator 3 should be excluded.
                Assert.Equal(2, results.Count);
        
                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to indicator 1", results[0].Content);
        
                Assert.Equal(releaseFootnotes[2].FootnoteId, results[1].Id);
                Assert.Equal("Applies to indicator 1 and indicator 3", results[1].Content);
            }
        }
        
        [Fact]
        public async Task GetFilteredFootnotes_FootnotesApplyToMultipleSubjects()
        {
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .WithFilters(_ => _fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                        .GenerateList(3))
                    .WithIndicatorGroups(_ => _fixture
                        .DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .GenerateList(1))
                    .GenerateList(2))
                .GenerateList();

            var (subject1, subject2) = GetSubjectsTuple2(releaseSubjects);
            
            // Set up a release with footnotes that apply to both subject 1 and subject 2
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applies to subject 1 and subject 2")
                        .SetSubjects(ListOf(subject1, subject2)))
                
                    // Footnote applies to subject 1 filter 1
                    // and subject 2 filter 1
                    // and subject 2 filter 2 group 1
                    // and subject 2 filter 3 group 1 item 1
                    .ForIndex(1, s => s
                        .SetContent("Applies to s1f1 s2f1 s2f2g1 s2f3g1i1")
                        .SetFilters(ListOf(subject1.Filters[0], subject2.Filters[0]))
                        .SetFilterGroups(ListOf(subject2.Filters[1].FilterGroups[0]))
                        .SetFilterItems(ListOf(subject2.Filters[2].FilterGroups[0].FilterItems[0])))
                    .ForIndex(2, s => s
                        .SetContent("Applies to subject 1 indicator 1 and subject 2 filter 1")
                        .SetFilters(ListOf(subject2.Filters[0]))
                        .SetIndicators(ListOf(subject1.IndicatorGroups[0].Indicators[0])))
                    .ForIndex(3, s => s
                        .SetContent("Applies to subject 1 filter 1 and subject 2 indicator 1")
                        .SetFilters(ListOf(subject1.Filters[0]))
                        .SetIndicators(ListOf(subject2.IndicatorGroups[0].Indicators[0])))
                    .ForIndex(4, s => s
                        .SetContent("Applies to subject 1 indicator 1 and subject 2 indicator 1")
                        .SetIndicators(ListOf(subject1.IndicatorGroups[0].Indicators[0], subject2.IndicatorGroups[0].Indicators[0])))
                    .GenerateList())
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.IndicatorGroup.AddRangeAsync(subject1.IndicatorGroups[0], subject2.IndicatorGroups[0]);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
        
                // This test makes sure that applying footnotes to more than one subject doesn't exclude them from the
                // result.
                
                // Get the footnotes applying to subject 1 or any of its filter items or indicators
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf(subject1.Filters[0].FilterGroups[0].FilterItems[0].Id),
                    indicatorIds: ListOf(subject1.IndicatorGroups[0].Indicators[0].Id));
        
                // Check that all of the footnotes are returned even though they have also been applied to subject 2
                Assert.Equal(5, results.Count);
        
                // Footnote applies to the requested subject as well as another subject
                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to subject 1 and subject 2", results[0].Content);
        
                // Footnote applies to a requested filter item via its filter as well as another subject's filter,
                // filter group, and filter item
                Assert.Equal(releaseFootnotes[1].FootnoteId, results[1].Id);
                Assert.Equal("Applies to s1f1 s2f1 s2f2g1 s2f3g1i1", results[1].Content);
        
                // Footnote applies to a requested indicator as well as another subject's filter
                Assert.Equal(releaseFootnotes[2].FootnoteId, results[2].Id);
                Assert.Equal("Applies to subject 1 indicator 1 and subject 2 filter 1", results[2].Content);
        
                // Footnote applies to a requested filter via its filter item as well as another subject's indicator
                Assert.Equal(releaseFootnotes[3].FootnoteId, results[3].Id);
                Assert.Equal("Applies to subject 1 filter 1 and subject 2 indicator 1", results[3].Content);
        
                // Footnote applies to a requested indicator as well as another subject's indicator
                Assert.Equal(releaseFootnotes[4].FootnoteId, results[4].Id);
                Assert.Equal("Applies to subject 1 indicator 1 and subject 2 indicator 1", results[4].Content);
            }
        }
        
        [Fact]
        public async Task GetFilteredFootnotes_FilterByEmptyListOfFiltersAndIndicators()
        {
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .ForIndex(0, s => s
                        .SetFilters(_fixture
                            .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                            .GenerateList(3))
                        .SetIndicatorGroups(_fixture.DefaultIndicatorGroup()
                            .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                            .GenerateList(1)))
                    .GenerateList(2))
                .GenerateList();

            var (subject1, subject2) = GetSubjectsTuple2(releaseSubjects);

            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applies to subject 1")
                        .SetSubjects(ListOf(subject1)))
                    .ForIndex(1, s => s
                        .SetContent("Applies to subject 1 filter 1")
                        .SetFilters(ListOf(subject1.Filters[0])))
                    .ForIndex(2, s => s
                        .SetContent("Applies to subject 1 filter 2 group 1")
                        .SetFilterGroups(ListOf(subject1.Filters[1].FilterGroups[0])))
                    .ForIndex(3, s => s
                        .SetContent("Applies to subject 1 filter 3 group 1 item 1")
                        .SetFilterItems(ListOf(subject1.Filters[2].FilterGroups[0].FilterItems[0])))
                    .ForIndex(4, s => s
                        .SetContent("Applies to subject 1 indicator 1")
                        .SetIndicators(ListOf(subject1.IndicatorGroups[0].Indicators[0])))
                    .ForIndex(5, s => s
                        .SetContent("Applies to subject 2")
                        .SetSubjects(ListOf(subject2)))
                    .GenerateList())
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1.Filters);
                await context.IndicatorGroup.AddAsync(subject1.IndicatorGroups[0]);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
        
                //  Get footnotes applying directly to subject 1 using empty lists of filter item and indicator id's
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf<Guid>(),
                    indicatorIds: ListOf<Guid>());
        
                // Check that only the footnotes which apply directly to subject 1 are returned
                // Other footnotes related to subject 1 should be ignored as no filter items or indicators were requested
                Assert.Single(results);
        
                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to subject 1", results[0].Content);
            }
        }
        
        [Fact]
        public async Task GetFilteredFootnotes_IgnoresFootnotesUnrelatedToRelease()
        {
            // Create one release with footnotes and another without
            var releases = _fixture.DefaultStatsRelease().GenerateList(2);
            
            var releaseSubjects = _fixture
                .DefaultReleaseSubject()
                .WithReleases(releases)
                .ForIndex(0, s => s.SetSubject(_fixture
                    .DefaultSubject()
                    .WithFilters(_fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                        .GenerateList(3))
                    .WithIndicatorGroups(_fixture
                        .DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .GenerateList(1))))
                .GenerateList();

            var subject = releaseSubjects[0].Subject;

            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(releases[0])
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applies to subject 1")
                        .SetSubjects(ListOf(subject)))
                    .ForIndex(1, s => s
                        .SetContent("Applies to subject 1 filter 1")
                        .SetFilters(ListOf(subject.Filters[0])))
                    .ForIndex(2, s => s
                        .SetContent("Applies to subject 1 filter 2 group 1")
                        .SetFilterGroups(ListOf(subject.Filters[1].FilterGroups[0])))
                    .ForIndex(3, s => s
                        .SetContent("Applies to subject 1 filter 3 group 1 item 1")
                        .SetFilterItems(ListOf(subject.Filters[2].FilterGroups[0].FilterItems[0])))
                    .ForIndex(4, s => s
                        .SetContent("Applies to subject 1 indicator 1")
                        .SetIndicators(ListOf(subject.IndicatorGroups[0].Indicators[0])))
                    .GenerateList())
                .GenerateList();
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Release.AddRangeAsync(releases);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
        
                // This test covers a case where a subject is shared by multiple releases. It makes sure that when other
                // releases have footnotes for the subject, that there's no way of retrieving those footnotes that
                // belong to other releases by specifying the subject id and filter item and indicator id's of the subject.
        
                var filter1Group1Item1Id = subject.Filters[0].FilterGroups[0].FilterItems[0].Id;
                var filter2Group1Item1Id = subject.Filters[1].FilterGroups[0].FilterItems[0].Id;
                var filter3Group1Item1Id = subject.Filters[2].FilterGroups[0].FilterItems[0].Id;
                var indicatorGroup1Item1Id = subject.IndicatorGroups[0].Indicators[0].Id;
        
                var results = await repository.GetFilteredFootnotes(
                    releaseId: releases[1].Id, // release 2 has no footnotes
                    subjectId: subject.Id,
                    filterItemIds: ListOf(filter1Group1Item1Id, filter2Group1Item1Id, filter3Group1Item1Id),
                    indicatorIds: ListOf(indicatorGroup1Item1Id));
        
                // Check that no footnotes are returned even though subject 1 has footnotes for a different release
                Assert.Empty(results);
            }
        }
        
        [Fact]
        public async Task GetFilteredFootnotes_IgnoresRequestedFilterItemsAndIndicatorsUnrelatedToSubject()
        {
            var release = _fixture.DefaultStatsRelease().Generate();

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .ForIndex(0, s => s
                        .SetFilters(_fixture
                            .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                            .GenerateList(3)))
                    .WithIndicatorGroups(_ => _fixture.DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .GenerateList(1))
                    .Generate(2))
                .GenerateList();
            
            var (subject1, subject2) = GetSubjectsTuple2(releaseSubjects);
            
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applies to subject 1")
                        .SetSubjects(ListOf(subject1)))
                    .ForIndex(1, s => s
                        .SetContent("Applies to subject 1 filter 1")
                        .SetFilters(ListOf(subject1.Filters[0])))
                    .ForIndex(2, s => s
                        .SetContent("Applies to subject 1 filter 2 group 1")
                        .SetFilterGroups(ListOf(subject1.Filters[1].FilterGroups[0])))
                    .ForIndex(3, s => s
                        .SetContent("Applies to subject 1 filter 3 group 1 item 1")
                        .SetFilterItems(ListOf(subject1.Filters[2].FilterGroups[0].FilterItems[0])))
                    .ForIndex(4, s => s
                        .SetContent("Applies to subject 1 indicator 1")
                        .SetIndicators(ListOf(subject1.IndicatorGroups[0].Indicators[0])))
                    .ForIndex(5, s => s
                        .SetContent("Applies to subject 2")
                        .SetSubjects(ListOf(subject2)))
                    .ForIndex(6, s => s
                        .SetContent("Applies to subject 2 indicator 1")
                        .SetIndicators(ListOf(subject2.IndicatorGroups[0].Indicators[0])))
                    .GenerateList())
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1.Filters);
                await context.IndicatorGroup.AddRangeAsync(subject1.IndicatorGroups[0], subject2.IndicatorGroups[0]);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
        
                // Get footnotes applying to subject 2
                // but also include filter item and indicator id's from subject 1
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject2.Id,
                    filterItemIds: ListOf(
                        subject1.Filters[0].FilterGroups[0].FilterItems[0].Id, // subject 1
                        subject1.Filters[1].FilterGroups[0].FilterItems[0].Id, // subject 1
                        subject1.Filters[2].FilterGroups[0].FilterItems[0].Id // subject 1
                    ),
                    indicatorIds: ListOf(
                        subject1.IndicatorGroups[0].Indicators[0].Id, // subject 1
                        subject2.IndicatorGroups[0].Indicators[0].Id // subject 2
                    ));
        
                // Check that only the footnotes which apply to subject 2 are returned
                // The filter item and indicator id's related to subject 1 should have been ignored
                // Footnotes applying to subject 1 should be excluded
                Assert.Equal(2, results.Count);
        
                Assert.Equal(releaseFootnotes[5].FootnoteId, results[0].Id);
                Assert.Equal("Applies to subject 2", results[0].Content);
        
                Assert.Equal(releaseFootnotes[6].FootnoteId, results[1].Id);
                Assert.Equal("Applies to subject 2 indicator 1", results[1].Content);
            }
        }

        [Fact]
        public async Task GetFootnotes_MapsAllCriteria()
        {
            var releases = _fixture.DefaultStatsRelease().GenerateList(2);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .WithRelease(releases[0])
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .WithFilters(_ => _fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                        .Generate(3))
                    .WithIndicatorGroups(_ => _fixture
                        .DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1))
                    .Generate(2))
                .GenerateList();
            
            var (subject1, subject2) = GetSubjectsTuple2(releaseSubjects);

            var releaseFootnote = _fixture.DefaultReleaseFootnote()
                .WithRelease(releases[0])
                .WithFootnote(_fixture.DefaultFootnote()
                    .WithContent("Applies to all criteria")
                    .WithSubjects(ListOf(subject1, subject2))
                    .WithFilters(ListOf(subject1.Filters[0], subject2.Filters[0]))
                    .WithFilterGroups(ListOf(subject1.Filters[1].FilterGroups[0], subject2.Filters[1].FilterGroups[0]))
                    .WithFilterItems(ListOf(subject1.Filters[2].FilterGroups[0].FilterItems[0], subject2.Filters[2].FilterGroups[0].FilterItems[0]))
                    .WithIndicators(ListOf(subject1.IndicatorGroups[0].Indicators[0], subject2.IndicatorGroups[0].Indicators[0])))
                .Generate();
            
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Release.AddRangeAsync(releases);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnote);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(releases[0].Id);

                Assert.Single(results);

                Assert.Equal("Applies to all criteria", results[0].Content);

                var footnoteReleases = results[0].Releases.ToList();

                Assert.Single(footnoteReleases);
                Assert.Equal(releases[0].Id, footnoteReleases[0].ReleaseId);

                var footnoteSubjects = results[0].Subjects.ToList();

                Assert.Equal(2, footnoteSubjects.Count);
                Assert.Equal(subject1.Id, footnoteSubjects[0].SubjectId);
                Assert.Equal(subject2.Id, footnoteSubjects[1].SubjectId);

                var footnoteFilters = results[0].Filters.ToList();

                Assert.Equal(2, footnoteSubjects.Count);
                Assert.Equal(subject1.Id, footnoteFilters[0].Filter.SubjectId);
                Assert.Equal(subject2.Id, footnoteFilters[1].Filter.SubjectId);

                var footnoteFilterGroups = results[0].FilterGroups.ToList();

                Assert.Equal(2, footnoteFilterGroups.Count);
                Assert.Equal(subject1.Id, footnoteFilterGroups[0].FilterGroup.Filter.SubjectId);
                Assert.Equal(subject2.Id, footnoteFilterGroups[1].FilterGroup.Filter.SubjectId);

                var footnoteFilterItems = results[0].FilterItems.ToList();

                Assert.Equal(2, footnoteFilterItems.Count);
                Assert.Equal(subject1.Id, footnoteFilterItems[0].FilterItem.FilterGroup.Filter.SubjectId);
                Assert.Equal(subject2.Id, footnoteFilterItems[1].FilterItem.FilterGroup.Filter.SubjectId);

                var footnoteIndicators = results[0].Indicators.ToList();

                Assert.Equal(2, footnoteIndicators.Count);
                Assert.Equal(subject1.Id, footnoteIndicators[0].Indicator.IndicatorGroup.SubjectId);
                Assert.Equal(subject2.Id, footnoteIndicators[1].Indicator.IndicatorGroup.SubjectId);
            }
        }

        [Fact]
        public async Task GetFootnotes_FiltersByRelease()
        {
            var releases = _fixture.DefaultStatsRelease().GenerateList(2);
        
            var (release1, release2) = releases.ToTuple2();
            
            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForRange(..2, s => s.SetRelease(release1))
                .ForIndex(2, s => s.SetRelease(release2))
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .WithFilters(_ => _fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                        .Generate(3))
                    .WithIndicatorGroups(_ => _fixture.DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1))
                    .Generate(3))
                .GenerateList();

            var (release1Subject1, release1Subject2, release2Subject) = GetSubjectsTuple3(releaseSubjects);
            
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForIndex(0, s => s.SetRelease(release1))
                .ForIndex(0, s => s.SetFootnote(_fixture
                    .DefaultFootnote()
                    .WithOrder(0)
                    .WithContent("Applied to release 1 subject 1")
                    .WithSubjects(ListOf(release1Subject1))))
                .ForIndex(1, s => s.SetRelease(release1))
                .ForIndex(1, s => s.SetFootnote(_fixture
                    .DefaultFootnote()
                    .WithOrder(1)
                    .WithContent("Applied to release 1 subject 2")
                    .WithSubjects(ListOf(release1Subject2))))
                .ForIndex(2, s => s.SetRelease(release2))
                .ForIndex(2, s => s.SetFootnote(_fixture
                    .DefaultFootnote()
                    .WithContent("Applied to release 2 subject")
                    .WithSubjects(ListOf(release2Subject))))
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Release.AddRangeAsync(release1, release2);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release1.Id);
        
                Assert.Equal(2, results.Count);
        
                Assert.Equal("Applied to release 1 subject 1", results[0].Content);
        
                var footnote1Releases = results[0].Releases.ToList();
        
                Assert.Single(footnote1Releases);
                Assert.Equal(release1.Id, footnote1Releases[0].ReleaseId);
        
                var footnote1Subjects = results[0].Subjects.ToList();
        
                Assert.Single(footnote1Subjects);
                Assert.Equal(release1Subject1.Id, footnote1Subjects[0].SubjectId);
        
                Assert.Equal("Applied to release 1 subject 2", results[1].Content);
        
                var footnote2Releases = results[1].Releases.ToList();
        
                Assert.Single(footnote2Releases);
                Assert.Equal(release1.Id, footnote2Releases[0].ReleaseId);
        
                var footnote2Subjects = results[1].Subjects.ToList();
                Assert.Single(footnote2Subjects);
                Assert.Equal(release1Subject2.Id, footnote2Subjects[0].SubjectId);
            }
        }
        
        [Fact]
        public async Task GetFootnotes_FiltersBySubjectId()
        {
            var release = _fixture.DefaultStatsRelease().Generate();
        
            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateList();
            
            var (subject1, subject2) = GetSubjectsTuple2(releaseSubjects);

            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Applied to subject 1")
                        .SetSubjects(ListOf(subject1)))
                    .ForIndex(1, s => s
                        .SetContent("Applied to subject 2")
                        .SetSubjects(ListOf(subject2)))
                    .GenerateList())
                .GenerateList();
            
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Release.AddRangeAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release.Id, subject2.Id);
        
                Assert.Single(results);
                Assert.Equal("Applied to subject 2", results[0].Content);
            }
        }
        
        [Fact]
        public async Task GetFootnotes_FiltersCriteriaBySubjectId()
        {
            var releases = _fixture.DefaultStatsRelease().GenerateList(2);
        
            var releaseSubjects = _fixture
                .DefaultReleaseSubject()
                .WithRelease(releases[0])
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .WithFilters(_ => _fixture
                        .DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
                        .GenerateList(3))
                    .WithIndicatorGroups(_ => _fixture
                        .DefaultIndicatorGroup()
                        .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                        .Generate(1))
                    .Generate(2))
                .GenerateList();
        
            var (subject1, subject2) = GetSubjectsTuple2(releaseSubjects);
            
            var releaseFootnote = _fixture.DefaultReleaseFootnote()
                .WithRelease(releases[0])
                .WithFootnote(_fixture
                    .DefaultFootnote()
                    .WithContent("Applies to all types of element")
                    .WithSubjects(ListOf(subject1, subject2))
                    .WithFilters(ListOf(subject1.Filters[0], subject2.Filters[0]))
                    .WithFilterGroups(ListOf(subject1.Filters[1].FilterGroups[0], subject2.Filters[1].FilterGroups[0]))
                    .WithFilterItems(ListOf(subject1.Filters[2].FilterGroups[0].FilterItems[0], subject2.Filters[2].FilterGroups[0].FilterItems[0]))
                    .WithIndicators(ListOf(subject1.IndicatorGroups[0].Indicators[0], subject2.IndicatorGroups[0].Indicators[0])))
                .Generate();
            
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1.Filters.Concat(subject2.Filters));
                await context.IndicatorGroup.AddRangeAsync(subject1.IndicatorGroups[0], subject2.IndicatorGroups[0]);
                await context.Release.AddRangeAsync(releases);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnote);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(releases[0].Id, subject1.Id);
        
                Assert.Single(results);
                Assert.Equal("Applies to all types of element", results[0].Content);
        
                var footnoteReleases = results[0].Releases.ToList();
        
                Assert.Single(footnoteReleases);
                Assert.Equal(releases[0].Id, footnoteReleases[0].ReleaseId);
        
                var footnoteSubjects = results[0].Subjects.ToList();
        
                Assert.Single(footnoteSubjects);
                Assert.Equal(subject1.Id, footnoteSubjects[0].SubjectId);
        
                var footnoteFilters = results[0].Filters.ToList();
        
                Assert.Single(footnoteFilters);
                Assert.Equal(subject1.Id, footnoteFilters[0].Filter.SubjectId);
        
                var footnoteFilterGroups = results[0].FilterGroups.ToList();
        
                Assert.Single(footnoteFilterGroups);
                Assert.Equal(subject1.Id, footnoteFilterGroups[0].FilterGroup.Filter.SubjectId);
        
                var footnoteFilterItems = results[0].FilterItems.ToList();
        
                Assert.Single(footnoteFilterItems);
                Assert.Equal(subject1.Id, footnoteFilterItems[0].FilterItem.FilterGroup.Filter.SubjectId);
        
                var footnoteIndicators = results[0].Indicators.ToList();
        
                Assert.Single(footnoteIndicators);
                Assert.Equal(subject1.Id, footnoteIndicators[0].Indicator.IndicatorGroup.SubjectId);
            }
        }
        
        [Fact]
        public async Task GetFootnotes_OrdersFootnotes()
        {
            var release = _fixture.DefaultStatsRelease().Generate();
        
            var releaseSubject = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubject(_fixture.DefaultSubject())
                .Generate();
            
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetContent("Footnote 3")
                        .Set(f => f.Order, 2)
                        .SetSubjects(ListOf(releaseSubject.Subject)))
                    .ForIndex(1, s => s
                        .SetContent("Footnote 1")
                        .Set(f => f.Order, 0)
                        .SetSubjects(ListOf(releaseSubject.Subject)))
                    .ForIndex(2, s => s
                        .SetContent("Footnote 2")
                        .Set(f => f.Order, 1)
                        .SetSubjects(ListOf(releaseSubject.Subject)))
                    .GenerateList())
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Release.AddRangeAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubject);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release.Id);
        
                Assert.Equal(3, results.Count);
        
                Assert.Equal("Footnote 1", results[0].Content);
                Assert.Equal(0, results[0].Order);
        
                Assert.Equal("Footnote 2", results[1].Content);
                Assert.Equal(1, results[1].Order);
        
                Assert.Equal("Footnote 3", results[2].Content);
                Assert.Equal(2, results[2].Order);
            }
        }
        
        [Fact]
        public async Task GetSubjectsWithNoFootnotes_FootnotePerSubject()
        {
            var release = _fixture.DefaultStatsRelease().Generate();
            
            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .WithFilters(_ => _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(3))
                    .ForIndex(4, s => s
                        .SetIndicatorGroups(_fixture
                            .DefaultIndicatorGroup()
                            .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                            .GenerateList(1)))
                    .GenerateList(6))
                .GenerateList();
            
            var subjectWithNoFootnotes = releaseSubjects[5].Subject;
        
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnotes(_fixture
                    .DefaultFootnote()
                    .ForIndex(0, s => s
                        .SetSubjects(ListOf(releaseSubjects[0].Subject)))
                    .ForIndex(1, s => s
                        .SetFilters(ListOf(releaseSubjects[1].Subject.Filters[0])))
                    .ForIndex(2, s => s
                        .SetFilterGroups(ListOf(releaseSubjects[2].Subject.Filters[1].FilterGroups[0])))
                    .ForIndex(3, s => s
                        .SetFilterItems(ListOf(releaseSubjects[3].Subject.Filters[2].FilterGroups[0].FilterItems[0])))
                    .ForIndex(4, s => s
                        .SetIndicators(ListOf(releaseSubjects[4].Subject.IndicatorGroups[0].Indicators[0])))
                    .GenerateList())
                .GenerateList();
            
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Release.AddRangeAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetSubjectsWithNoFootnotes(release.Id);
        
                Assert.Single(results);
                Assert.Equal(subjectWithNoFootnotes.Id, results[0].Id);
            }
        }
        
        [Fact]
        public async Task GetSubjectsWithNoFootnotes_FootnoteForMultipleSubjects()
        {
            var release = _fixture.DefaultStatsRelease().Generate();
        
            var releaseSubjects = _fixture
                .DefaultReleaseSubject()
                .WithRelease(release)
                .WithSubjects(_fixture
                    .DefaultSubject()
                    .WithFilters(_ => _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1).Generate(3))
                    .Generate(2))
                .GenerateList();
            
            var (subjectWithFootnotes, subjectWithNoFootnotes) = GetSubjectsTuple2(releaseSubjects);
            
            var subjectWithFootnotesIndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithSubject(subjectWithFootnotes)
                .WithIndicators(_fixture.DefaultIndicator().Generate(1))
                .Generate();
        
            var releaseFootnote = _fixture.DefaultReleaseFootnote()
                .WithRelease(release)
                .WithFootnote(_fixture
                    .DefaultFootnote()
                    .WithSubjects(ListOf(subjectWithFootnotes))
                    .WithFilters(ListOf(subjectWithFootnotes.Filters[0]))
                    .WithFilterGroups(ListOf(subjectWithFootnotes.Filters[1].FilterGroups[0]))
                    .WithFilterItems(ListOf(subjectWithFootnotes.Filters[2].FilterGroups[0].FilterItems[0]))
                    .WithIndicators(ListOf(subjectWithFootnotesIndicatorGroup.Indicators[0])))
                .Generate();
            
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Release.AddRangeAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnote);
                await context.SaveChangesAsync();
            }
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetSubjectsWithNoFootnotes(release.Id);
        
                Assert.Single(results);
        
                Assert.Equal(subjectWithNoFootnotes.Id, results[0].Id);
            }
        }

        private static Tuple<Subject, Subject> GetSubjectsTuple2(List<ReleaseSubject> releaseSubjects)
        {
            return releaseSubjects.Select(rs => rs.Subject).ToTuple2();
        }

        private static Tuple<Subject, Subject, Subject> GetSubjectsTuple3(List<ReleaseSubject> releaseSubjects)
        {
            return releaseSubjects.Select(rs => rs.Subject).ToTuple3();
        }

        private static FootnoteRepository BuildFootnoteRepository(StatisticsDbContext context)
        {
            return new FootnoteRepository(context);
        }
    }
}

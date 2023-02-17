#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
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
            var release = _fixture.DefaultRelease().Generate();
            
            var subject1Filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var (subject1, subject2) = _fixture.DefaultSubject()
                .ForIndex(0, s => s.SetFilters(subject1Filters))
                .GenerateTuple2();

            var subject1IndicatorGroup1 = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject1)
                .Generate();

            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to subject 1")
                    .SetSubjects(_ => ListOf(subject1)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 1")
                    .SetFilters(_ => ListOf(subject1Filters[0])))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 2 group 1")
                    .SetFilterGroups(_ => ListOf(subject1Filters[1].FilterGroups[0])))
                .ForIndex(3, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 3 group 1 item 1")
                    .SetFilterItems(_ => ListOf(subject1Filters[2].FilterGroups[0].FilterItems[0])))
                .ForIndex(4, s => s
                    .Set(f => f.Content, "Applies to subject 1 indicator 1")
                    .SetIndicators(_ => ListOf(subject1IndicatorGroup1.Indicators[0])))
                
                // Footnote applies to subject 1 filter 1
                // and subject 1 filter 2 group 1
                // and subject 1 filter 3 group 1 item 1
                // and subject 1 indicator 1
                .ForIndex(5, s => s
                    .Set(f => f.Content, "Applies to multiple attributes of subject 1")
                    .SetFilters(_ => ListOf(subject1Filters[0]))
                    .SetFilterGroups(_ => ListOf(subject1Filters[1].FilterGroups[0]))
                    .SetFilterItems(_ => ListOf(subject1Filters[2].FilterGroups[0].FilterItems[0]))
                    .SetIndicators(_ => ListOf(subject1IndicatorGroup1.Indicators[0])))
                .ForIndex(6, s => s
                    .Set(f => f.Content, "Applies to subject 2")
                    .SetSubjects(_ => ListOf(subject2)))
                .GenerateList();
            
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForIndex(0, s => s.Set(rs => rs.Subject, subject1))
                .ForIndex(1, s => s.Set(rs => rs.Subject, subject2))
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filters);
                await context.IndicatorGroup.AddAsync(subject1IndicatorGroup1);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                //  Get the footnotes applying to subject 1 or any of its filter items or indicators
                var filter1Group1Item1Id = subject1Filters[0].FilterGroups[0].FilterItems[0].Id;
                var filter2Group1Item1Id = subject1Filters[1].FilterGroups[0].FilterItems[0].Id;
                var filter3Group1Item1Id = subject1Filters[2].FilterGroups[0].FilterItems[0].Id;
                var indicatorGroup1Item1Id = subject1IndicatorGroup1.Indicators[0].Id;

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
            var release = _fixture.DefaultRelease().Generate();
            
            // todo a default one with new filtergroups and all?
            var subject1Filter = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .Generate();
            
            var subject2Filter = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .Generate();
            
            var (subject1, subject2) = _fixture.DefaultSubject()
                .ForIndex(0, s => s.SetFilters(ListOf(subject1Filter)))
                .ForIndex(1, s => s.SetFilters(ListOf(subject2Filter)))
                .GenerateTuple2();

            var subject1IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject1)
                .Generate();

            var subject2IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject2)
                .Generate();
            
            // Set up a release with footnotes that apply to either subject 1 or subject 2 but not both
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to subject 1")
                    .SetSubjects(_ => ListOf(subject1)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applies to subject 2")
                    .SetSubjects(_ => ListOf(subject2)))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 1")
                    .SetFilters(_ => ListOf(subject1Filter)))
                .ForIndex(3, s => s
                    .Set(f => f.Content, "Applies to subject 2 filter 1")
                    .SetFilters(_ => ListOf(subject2Filter)))
                .ForIndex(4, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 1 group 1")
                    .SetFilterGroups(_ => ListOf(subject1Filter.FilterGroups[0])))
                .ForIndex(5, s => s
                    .Set(f => f.Content, "Applies to subject 2 filter 1 group 1")
                    .SetFilterGroups(_ => ListOf(subject2Filter.FilterGroups[0])))
                .ForIndex(6, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 1 group 1 item 1")
                    .SetFilterItems(_ => ListOf(subject1Filter.FilterGroups[0].FilterItems[0])))
                .ForIndex(7, s => s
                    .Set(f => f.Content, "Applies to subject 2 filter 1 group 1 item 1")
                    .SetFilterItems(_ => ListOf(subject2Filter.FilterGroups[0].FilterItems[0])))
                .ForIndex(8, s => s
                    .Set(f => f.Content, "Applies to subject 1 indicator 1")
                    .SetIndicators(_ => ListOf(subject1IndicatorGroup.Indicators[0])))
                .ForIndex(9, s => s
                    .Set(f => f.Content, "Applies to subject 2 indicator 1")
                    .SetIndicators(_ => ListOf(subject2IndicatorGroup.Indicators[0])))
                .GenerateList();
            
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForIndex(0, s => s.Set(rs => rs.Subject, subject1))
                .ForIndex(1, s => s.Set(rs => rs.Subject, subject2))
                .GenerateList();
            
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filter, subject2Filter);
                await context.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
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
                    filterItemIds: ListOf(subject1Filter.FilterGroups[0].FilterItems[0].Id),
                    indicatorIds: ListOf(subject1IndicatorGroup.Indicators[0].Id));
        
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
            var release = _fixture.DefaultRelease().Generate();
            
            // todo a default one with new filtergroups and all?
            var filter = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(2))
                    .GenerateList(1))
                .Generate();
            
            var subject = _fixture.DefaultSubject()
                .WithFilters(ListOf(filter))
                .Generate();

            var indicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(2))
                .WithSubject(subject)
                .Generate();
            
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to filter 1")
                    .SetFilters(_ => ListOf(filter)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applies to filter 1 group 1")
                    .SetFilterGroups(_ => ListOf(filter.FilterGroups[0])))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Applies to filter item 1")
                    .SetFilterItems(_ => ListOf(filter.FilterGroups[0].FilterItems[0])))
                .ForIndex(3, s => s
                    .Set(f => f.Content, "Applies to filter item 2")
                    .SetFilterItems(_ => ListOf(filter.FilterGroups[0].FilterItems[1])))
                .ForIndex(4, s => s
                    .Set(f => f.Content, "Applies to indicator 1")
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[0])))
                .ForIndex(5, s => s
                    .Set(f => f.Content, "Applies to indicator 2")
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[1])))
                .ForIndex(6, s => s
                    .Set(f => f.Content, "Applies to filter 1 and indicator 1")
                    .SetFilters(_ => ListOf(filter))
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[0])))
                .ForIndex(7, s => s
                    .Set(f => f.Content, "Applies to filter 1 and indicator 2")
                    .SetFilters(_ => ListOf(filter))
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[1])))
                .ForIndex(8, s => s
                    .Set(f => f.Content, "Applies to filter 1 group 1 and indicator 1")
                    .SetFilterGroups(_ => ListOf(filter.FilterGroups[0]))
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[0])))
                .ForIndex(9, s => s
                    .Set(f => f.Content, "Applies to filter 1 group 1 and indicator 2")
                    .SetFilterGroups(_ => ListOf(filter.FilterGroups[0]))
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[1])))
                .ForIndex(10, s => s
                    .Set(f => f.Content, "Applies to filter item 1 and indicator 1")
                    .SetFilterItems(_ => ListOf(filter.FilterGroups[0].FilterItems[0]))
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[0])))
                .ForIndex(11, s => s
                    .Set(f => f.Content, "Applies to filter item 1 and indicator 2")
                    .SetFilterItems(_ => ListOf(filter.FilterGroups[0].FilterItems[0]))
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[1])))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubject = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForInstance(s => s.Set(rs => rs.Subject, subject))
                .Generate();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddAsync(subject);
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
                    subjectId: subject.Id,
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
            var release = _fixture.DefaultRelease().Generate();
            
            var (filter1, filter2, filter3) = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(3))
                    .GenerateList(1))
                .GenerateTuple3();
            
            var subject = _fixture.DefaultSubject()
                .WithFilters(ListOf(filter1, filter2, filter3))
                .Generate();
            
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to filter 1")
                    .SetFilters(_ => ListOf(filter1)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applies to filter 3")
                    .SetFilters(_ => ListOf(filter3)))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Applies to filter 1 and filter 3")
                    .SetFilters(_ => ListOf(filter1, filter3)))
                .ForIndex(3, s => s
                    .Set(f => f.Content, "Applies to filter 1 and filter 3 group 1")
                    .SetFilters(_ => ListOf(filter1))
                    .SetFilterGroups(_ => ListOf(filter3.FilterGroups[0])))
                .ForIndex(4, s => s
                    .Set(f => f.Content, "Applies to filter 3 and filter 1 group 1")
                    .SetFilters(_ => ListOf(filter3))
                    .SetFilterGroups(_ => ListOf(filter1.FilterGroups[0])))
                .ForIndex(5, s => s
                    .Set(f => f.Content, "Applies to filter 1 and filter 3 group item 1")
                    .SetFilters(_ => ListOf(filter1))
                    .SetFilterItems(_ => ListOf(filter3.FilterGroups[0].FilterItems[0])))
                .ForIndex(6, s => s
                    .Set(f => f.Content, "Applies to filter 3 and filter 1 group item 1")
                    .SetFilters(_ => ListOf(filter3))
                    .SetFilterItems(_ => ListOf(filter1.FilterGroups[0].FilterItems[0])))
                .ForIndex(7, s => s
                    .Set(f => f.Content, "Applies to filter 1 group 1")
                    .SetFilterGroups(_ => ListOf(filter1.FilterGroups[0])))
                .ForIndex(8, s => s
                    .Set(f => f.Content, "Applies to filter 3 group 1")
                    .SetFilterGroups(_ => ListOf(filter3.FilterGroups[0])))
                .ForIndex(9, s => s
                    .Set(f => f.Content, "Applies to filter 1 group 1 and filter 1 group 1 item 1")
                    .SetFilterGroups(_ => ListOf(filter1.FilterGroups[0]))
                    .SetFilterItems(_ => ListOf(filter1.FilterGroups[0].FilterItems[0])))
                .ForIndex(10, s => s
                    .Set(f => f.Content, "Applies to filter 1 group 1 and filter 3 group 1 item 1")
                    .SetFilterGroups(_ => ListOf(filter1.FilterGroups[0]))
                    .SetFilterItems(_ => ListOf(filter3.FilterGroups[0].FilterItems[0])))
                .ForIndex(11, s => s
                    .Set(f => f.Content, "Applies to filter 3 group 1 and filter 1 group 1 item 1")
                    .SetFilterGroups(_ => ListOf(filter3.FilterGroups[0]))
                    .SetFilterItems(_ => ListOf(filter1.FilterGroups[0].FilterItems[0])))
                .ForIndex(12, s => s
                    .Set(f => f.Content, "Applies to filter 3 group 1 and filter 3 group 1 item 1")
                    .SetFilterGroups(_ => ListOf(filter3.FilterGroups[0]))
                    .SetFilterItems(_ => ListOf(filter3.FilterGroups[0].FilterItems[0])))
                .ForIndex(13, s => s
                    .Set(f => f.Content, "Applies to filter 1 group 1 item 1")
                    .SetFilterItems(_ => ListOf(filter1.FilterGroups[0].FilterItems[0])))
                .ForIndex(14, s => s
                    .Set(f => f.Content, "Applies to filter 1 group 1 item 3")
                    .SetFilterItems(_ => ListOf(filter1.FilterGroups[0].FilterItems[2])))
                .ForIndex(15, s => s
                    .Set(f => f.Content, "Applies to filter 3 group 1 item 1")
                    .SetFilterItems(_ => ListOf(filter3.FilterGroups[0].FilterItems[0])))
                .ForIndex(16, s => s
                    .Set(f => f.Content, "Applies to filter 1 group 1 item 1 and filter 1 group 1 item 3")
                    .SetFilterItems(_ => ListOf(filter1.FilterGroups[0].FilterItems[0], filter1.FilterGroups[0].FilterItems[2])))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubject = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForInstance(s => s.Set(rs => rs.Subject, subject))
                .Generate();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddAsync(subject);
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
                    subjectId: subject.Id,
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
            var release = _fixture.DefaultRelease().Generate();
            
            var indicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(3))
                .Generate();
            
            var subject = _fixture.DefaultSubject()
                .WithIndicatorGroups(ListOf(indicatorGroup))
                .Generate();
            
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to indicator 1")
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[0])))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applies to indicator 3")
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[2])))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Applies to indicator 1 and indicator 3")
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[0], indicatorGroup.Indicators[2])))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubject = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForInstance(s => s.Set(rs => rs.Subject, subject))
                .Generate();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddAsync(subject);
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
                    subjectId: subject.Id,
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
            var release = _fixture.DefaultRelease().Generate();
            
            // todo a default one with new filtergroups and all?
            var filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(4);

            var subject1Filters = ListOf(filters[0]);
            var subject2Filters = filters.GetRange(1, 3);
            
            var (subject1, subject2) = _fixture.DefaultSubject()
                .ForIndex(0, s => s.SetFilters(subject1Filters))
                .ForIndex(1, s => s.SetFilters(subject2Filters))
                .GenerateTuple2();

            var subject1IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject1)
                .Generate();
            
            var subject2IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject2)
                .Generate();
            
            // Set up a release with footnotes that apply to both subject 1 and subject 2
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to subject 1 and subject 2")
                    .SetSubjects(_ => ListOf(subject1, subject2)))
                
                // Footnote applies to subject 1 filter 1
                // and subject 2 filter 1
                // and subject 2 filter 2 group 1
                // and subject 2 filter 3 group 1 item 1
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applies to s1f1 s2f1 s2f2g1 s2f3g1i1")
                    .SetFilters(_ => ListOf(subject1Filters[0], subject2Filters[0]))
                    .SetFilterGroups(_ => ListOf(subject2Filters[1].FilterGroups[0]))
                    .SetFilterItems(_ => ListOf(subject2Filters[2].FilterGroups[0].FilterItems[0])))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Applies to subject 1 indicator 1 and subject 2 filter 1")
                    .SetFilters(_ => ListOf(subject2Filters[0]))
                    .SetIndicators(_ => ListOf(subject1IndicatorGroup.Indicators[0])))
                .ForIndex(3, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 1 and subject 2 indicator 1")
                    .SetFilters(_ => ListOf(subject1Filters[0]))
                    .SetIndicators(_ => ListOf(subject2IndicatorGroup.Indicators[0])))
                .ForIndex(4, s => s
                    .Set(f => f.Content, "Applies to subject 1 indicator 1 and subject 2 indicator 1")
                    .SetIndicators(_ => ListOf(subject1IndicatorGroup.Indicators[0], subject2IndicatorGroup.Indicators[0])))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForIndex(0, s => s.Set(rs => rs.Subject, subject1))
                .ForIndex(1, s => s.Set(rs => rs.Subject, subject2))
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(filters);
                await context.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
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
                    filterItemIds: ListOf(subject1Filters[0].FilterGroups[0].FilterItems[0].Id),
                    indicatorIds: ListOf(subject1IndicatorGroup.Indicators[0].Id));
        
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
            var release = _fixture.DefaultRelease().Generate();
            
            // todo a default one with new filtergroups and all?
            var subject1Filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var (subject1, subject2) = _fixture.DefaultSubject()
                .ForIndex(0, s => s.SetFilters(subject1Filters))
                .GenerateTuple2();

            var subject1IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject1)
                .Generate();
            
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to subject 1")
                    .SetSubjects(_ => ListOf(subject1)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 1")
                    .SetFilters(_ => ListOf(subject1Filters[0])))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 2 group 1")
                    .SetFilterGroups(_ => ListOf(subject1Filters[1].FilterGroups[0])))
                .ForIndex(3, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 3 group 1 item 1")
                    .SetFilterItems(_ => ListOf(subject1Filters[2].FilterGroups[0].FilterItems[0])))
                .ForIndex(4, s => s
                    .Set(f => f.Content, "Applies to subject 1 indicator 1")
                    .SetIndicators(_ => ListOf(subject1IndicatorGroup.Indicators[0])))
                .ForIndex(5, s => s
                    .Set(f => f.Content, "Applies to subject 2")
                    .SetSubjects(_ => ListOf(subject2)))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForIndex(0, s => s.Set(rs => rs.Subject, subject1))
                .ForIndex(1, s => s.Set(rs => rs.Subject, subject2))
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filters);
                await context.IndicatorGroup.AddAsync(subject1IndicatorGroup);
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
            var (release1, release2) = _fixture.DefaultRelease().GenerateTuple2();
            
            // todo a default one with new filtergroups and all?
            var filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .Generate();

            var indicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject)
                .Generate();
            
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to subject 1")
                    .SetSubjects(_ => ListOf(subject)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 1")
                    .SetFilters(_ => ListOf(filters[0])))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 2 group 1")
                    .SetFilterGroups(_ => ListOf(filters[1].FilterGroups[0])))
                .ForIndex(3, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 3 group 1 item 1")
                    .SetFilterItems(_ => ListOf(filters[2].FilterGroups[0].FilterItems[0])))
                .ForIndex(4, s => s
                    .Set(f => f.Content, "Applies to subject 1 indicator 1")
                    .SetIndicators(_ => ListOf(indicatorGroup.Indicators[0])))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release1))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);
            
            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForIndex(0, s => s.Set(rs => rs.Release, release1))
                .ForIndex(0, s => s.Set(rs => rs.Subject, subject))
                .ForIndex(1, s => s.Set(rs => rs.Release, release2))
                .ForIndex(1, s => s.Set(rs => rs.Subject, subject))
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddAsync(subject);
                await context.Filter.AddRangeAsync(filters);
                await context.IndicatorGroup.AddAsync(indicatorGroup);
                await context.Release.AddRangeAsync(release1, release2);
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
        
                var filter1Group1Item1Id = filters[0].FilterGroups[0].FilterItems[0].Id;
                var filter2Group1Item1Id = filters[1].FilterGroups[0].FilterItems[0].Id;
                var filter3Group1Item1Id = filters[2].FilterGroups[0].FilterItems[0].Id;
                var indicatorGroup1Item1Id = indicatorGroup.Indicators[0].Id;
        
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release2.Id, // release 2 has no footnotes
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
            var release = _fixture.DefaultRelease().Generate();
            
            // todo a default one with new filtergroups and all?
            var subject1Filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var (subject1, subject2) = _fixture.DefaultSubject()
                .ForIndex(0, s => s.SetFilters(subject1Filters))
                .GenerateTuple2();

            var subject1IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject1)
                .Generate();

            var subject2IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject2)
                .Generate();
            
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to subject 1")
                    .SetSubjects(_ => ListOf(subject1)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 1")
                    .SetFilters(_ => ListOf(subject1Filters[0])))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 2 group 1")
                    .SetFilterGroups(_ => ListOf(subject1Filters[1].FilterGroups[0])))
                .ForIndex(3, s => s
                    .Set(f => f.Content, "Applies to subject 1 filter 3 group 1 item 1")
                    .SetFilterItems(_ => ListOf(subject1Filters[2].FilterGroups[0].FilterItems[0])))
                .ForIndex(4, s => s
                    .Set(f => f.Content, "Applies to subject 1 indicator 1")
                    .SetIndicators(_ => ListOf(subject1IndicatorGroup.Indicators[0])))
                .ForIndex(5, s => s
                    .Set(f => f.Content, "Applies to subject 2")
                    .SetSubjects(_ => ListOf(subject2)))
                .ForIndex(6, s => s
                    .Set(f => f.Content, "Applies to subject 2 indicator 1")
                    .SetIndicators(_ => ListOf(subject2IndicatorGroup.Indicators[0])))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForIndex(0, s => s.Set(rs => rs.Subject, subject1))
                .ForIndex(1, s => s.Set(rs => rs.Subject, subject2))
                .GenerateList();
        
            var contextId = Guid.NewGuid().ToString();
        
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filters);
                await context.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
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
                        subject1Filters[0].FilterGroups[0].FilterItems[0].Id, // subject 1
                        subject1Filters[1].FilterGroups[0].FilterItems[0].Id, // subject 1
                        subject1Filters[2].FilterGroups[0].FilterItems[0].Id // subject 1
                    ),
                    indicatorIds: ListOf(
                        subject1IndicatorGroup.Indicators[0].Id, // subject 1
                        subject2IndicatorGroup.Indicators[0].Id // subject 2
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
            var (release1, release2) = _fixture.DefaultRelease().GenerateTuple2();

            // todo a default one with new filtergroups and all?
            var subject1Filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var subject2Filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var (subject1, subject2) = _fixture.DefaultSubject()
                .ForIndex(0, s => s.SetFilters(subject1Filters))
                .ForIndex(1, s => s.SetFilters(subject2Filters))
                .GenerateTuple2();

            var subject1IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject1)
                .Generate();

            var subject2IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject2)
                .Generate();
            
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to all criteria")
                    .SetSubjects(_ => ListOf(subject1, subject2))
                    .SetFilters(_ => ListOf(subject1Filters[0], subject2Filters[0]))
                    .SetFilterGroups(_ => ListOf(subject1Filters[1].FilterGroups[0], subject2Filters[1].FilterGroups[0]))
                    .SetFilterItems(_ => ListOf(subject1Filters[2].FilterGroups[0].FilterItems[0], subject2Filters[2].FilterGroups[0].FilterItems[0]))
                    .SetIndicators(_ => ListOf(subject1IndicatorGroup.Indicators[0], subject2IndicatorGroup.Indicators[0])))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release1))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release1))
                .ForIndex(0, s => s.Set(rs => rs.Subject, subject1))
                .ForIndex(1, s => s.Set(rs => rs.Subject, subject2))
                .GenerateList();
            
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filters.Concat(subject2Filters));
                await context.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
                await context.Release.AddRangeAsync(release1, release2);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release1.Id);

                Assert.Single(results);

                Assert.Equal("Applies to all criteria", results[0].Content);

                var footnoteReleases = results[0].Releases.ToList();

                Assert.Single(footnoteReleases);
                Assert.Equal(release1.Id, footnoteReleases[0].ReleaseId);

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
            var (release1, release2) = _fixture.DefaultRelease().GenerateTuple2();

            // todo a default one with new filtergroups and all?
            var subject1Filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var subject2Filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var (release1Subject1, release1Subject2, release2Subject) = _fixture.DefaultSubject()
                .ForIndex(0, s => s.SetFilters(subject1Filters))
                .ForIndex(1, s => s.SetFilters(subject2Filters))
                .GenerateTuple3();

            var subject1IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(release1Subject1)
                .Generate();

            var subject2IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(release1Subject2)
                .Generate();
            
            var release1Footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applied to release 1 subject 1")
                    .SetSubjects(_ => ListOf(release1Subject1)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applied to release 1 subject 2")
                    .SetSubjects(_ => ListOf(release1Subject2)))
                .GenerateList();
            
            var release2Footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applied to release 2 subject")
                    .SetSubjects(_ => ListOf(release2Subject)))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForIndex(0, s => s
                    .Set(rf => rf.Release, release1)
                    .Set(rf => rf.Footnote, release1Footnotes[0]))
                .ForIndex(1, s => s
                    .Set(rf => rf.Release, release1)
                    .Set(rf => rf.Footnote, release1Footnotes[1]))
                .ForIndex(2, s => s
                    .Set(rf => rf.Release, release2)
                    .Set(rf => rf.Footnote, release2Footnotes[0]))
                .GenerateList();

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release1))
                .ForIndex(0, s => s.Set(rs => rs.Subject, release1Subject1))
                .ForIndex(1, s => s.Set(rs => rs.Subject, release1Subject2))
                .ForIndex(2, s => s.Set(rs => rs.Subject, release2Subject))
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(release1Subject1, release1Subject2);
                await context.Filter.AddRangeAsync(subject1Filters.Concat(subject2Filters));
                await context.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
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
            var release = _fixture.DefaultRelease().Generate();
            
            var (subject1, subject2) = _fixture.DefaultSubject()
                .GenerateTuple2();

            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applied to subject 1")
                    .SetSubjects(_ => ListOf(subject1)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Applied to subject 2")
                    .SetSubjects(_ => ListOf(subject2)))
                .GenerateList();
            
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForIndex(0, s => s.Set(rs => rs.Subject, subject1))
                .ForIndex(1, s => s.Set(rs => rs.Subject, subject2))
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
            var (release1, release2) = _fixture.DefaultRelease().GenerateTuple2();

            // todo a default one with new filtergroups and all?
            var subject1Filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var subject2Filters = _fixture.DefaultFilter()
                .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                    .WithFilterItems(_ => _fixture.DefaultFilterItem().GenerateList(1))
                    .GenerateList(1))
                .GenerateList(3);
            
            var (subject1, subject2) = _fixture.DefaultSubject()
                .ForIndex(0, s => s.SetFilters(subject1Filters))
                .ForIndex(1, s => s.SetFilters(subject2Filters))
                .GenerateTuple2();

            var subject1IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject1)
                .Generate();

            var subject2IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .WithSubject(subject2)
                .Generate();
            
            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Applies to all types of element")
                    .SetSubjects(_ => ListOf(subject1, subject2))
                    .SetFilters(_ => ListOf(subject1Filters[0], subject2Filters[0]))
                    .SetFilterGroups(_ => ListOf(subject1Filters[1].FilterGroups[0], subject2Filters[1].FilterGroups[0]))
                    .SetFilterItems(_ => ListOf(subject1Filters[2].FilterGroups[0].FilterItems[0], subject2Filters[2].FilterGroups[0].FilterItems[0]))
                    .SetIndicators(_ => ListOf(subject1IndicatorGroup.Indicators[0], subject2IndicatorGroup.Indicators[0])))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release1))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release1))
                .ForIndex(0, s => s.Set(rs => rs.Subject, subject1))
                .ForIndex(1, s => s.Set(rs => rs.Subject, subject2))
                .GenerateList();
            
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filters.Concat(subject2Filters));
                await context.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
                await context.Release.AddRangeAsync(release1, release2);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.ReleaseFootnote.AddRangeAsync(releaseFootnotes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release1.Id, subject1.Id);

                Assert.Single(results);
                Assert.Equal("Applies to all types of element", results[0].Content);

                var footnoteReleases = results[0].Releases.ToList();

                Assert.Single(footnoteReleases);
                Assert.Equal(release1.Id, footnoteReleases[0].ReleaseId);

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
            var release = _fixture.DefaultRelease().Generate();
            var subject = _fixture.DefaultSubject().Generate();

            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .Set(f => f.Content, "Footnote 3")
                    .Set(f => f.Order, 2)
                    .SetSubjects(_ => ListOf(subject)))
                .ForIndex(1, s => s
                    .Set(f => f.Content, "Footnote 1")
                    .Set(f => f.Order, 0)
                    .SetSubjects(_ => ListOf(subject)))
                .ForIndex(2, s => s
                    .Set(f => f.Content, "Footnote 2")
                    .Set(f => f.Order, 1)
                    .SetSubjects(_ => ListOf(subject)))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForInstance(s => s.Set(rs => rs.Subject, subject))
                .GenerateList(1);

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
            var release = _fixture.DefaultRelease().Generate();
            
            var subjects = _fixture.DefaultSubject()
                .WithFilters(_ => _fixture.DefaultFilter()
                    .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                        .WithFilterItems(_ => _fixture.DefaultFilterItem()
                            .GenerateList(1))
                        .GenerateList(1))
                    .GenerateList(3))
                .GenerateList(6);
            
            var subjectWithNoFootnotes = subjects[5];

            var subject5IndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithSubject(subjects[4])
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .Generate();

            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .SetSubjects(_ => ListOf(subjects[0])))
                .ForIndex(1, s => s
                    .SetFilters(_ => ListOf(subjects[1].Filters[0])))
                .ForIndex(2, s => s
                    .SetFilterGroups(_ => ListOf(subjects[2].Filters[1].FilterGroups[0])))
                .ForIndex(3, s => s
                    .SetFilterItems(_ => ListOf(subjects[3].Filters[2].FilterGroups[0].FilterItems[0])))
                .ForIndex(4, s => s
                    .SetIndicators(_ => ListOf(subject5IndicatorGroup.Indicators[0])))
                .GenerateList();
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForInstance(s => s.Set(rs => rs.Subject, (_, _, context) => subjects[context.Index]))
                .GenerateList(subjects.Count);
            
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subjects);
                await context.IndicatorGroup.AddRangeAsync(subject5IndicatorGroup);
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
            var release = _fixture.DefaultRelease().Generate();
            
            var subjects = _fixture.DefaultSubject()
                .WithFilters(_ => _fixture.DefaultFilter()
                    .WithFilterGroups(_ => _fixture.DefaultFilterGroup()
                        .WithFilterItems(_ => _fixture.DefaultFilterItem()
                            .GenerateList(1))
                        .GenerateList(1))
                    .GenerateList(3))
                .GenerateList(2);
            
            var subjectWithFootnotes = subjects[0];
            var subjectWithNoFootnotes = subjects[1];
            
            var subjectWithFootnotesIndicatorGroup = _fixture.DefaultIndicatorGroup()
                .WithSubject(subjectWithFootnotes)
                .WithIndicators(_fixture.DefaultIndicator().GenerateList(1))
                .Generate();

            var footnotes = _fixture.DefaultFootnote()
                .ForIndex(0, s => s
                    .SetSubjects(_ => ListOf(subjectWithFootnotes))
                    .SetFilters(_ => ListOf(subjectWithFootnotes.Filters[0]))
                    .SetFilterGroups(_ => ListOf(subjectWithFootnotes.Filters[1].FilterGroups[0]))
                    .SetFilterItems(_ => ListOf(subjectWithFootnotes.Filters[2].FilterGroups[0].FilterItems[0]))
                    .SetIndicators(_ => ListOf(subjectWithFootnotesIndicatorGroup.Indicators[0])))
                .GenerateList(1);
                
            var releaseFootnotes = _fixture.DefaultReleaseFootnote()
                .ForInstance(s => s.Set(rf => rf.Release, release))
                .ForInstance(s => s.Set(rf => rf.Footnote, (_, _, context) => footnotes[context.Index]))
                .GenerateList(footnotes.Count);

            var releaseSubjects = _fixture.DefaultReleaseSubject()
                .ForInstance(s => s.Set(rs => rs.Release, release))
                .ForInstance(s => s.Set(rs => rs.Subject, (_, _, context) => subjects[context.Index]))
                .GenerateList(subjects.Count);
            
            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subjects);
                await context.IndicatorGroup.AddRangeAsync(subjectWithFootnotesIndicatorGroup);
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

        private static FootnoteRepository BuildFootnoteRepository(StatisticsDbContext context)
        {
            return new FootnoteRepository(context);
        }
    }
}

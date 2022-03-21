#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using Thinktecture.EntityFrameworkCore.TempTables;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ObservationService;
using static Moq.MockBehavior;

// ReSharper disable AccessToDisposedClosure
namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class ObservationServiceTests
    {
        [Fact]
        public async Task GetMatchedObservations()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            await using var context = InMemoryStatisticsDbContext();
            
            const string sql = "Some SQL here";

            var sqlParameters = ListOf(new SqlParameter("param1", "value"));

            var observationQueryContext = new ObservationQueryContext
            {
                SubjectId = Guid.NewGuid(),
                Filters = ListOf(Guid.NewGuid()),
                LocationIds = ListOf(Guid.NewGuid()),
                TimePeriod = new TimePeriodQuery()
            };
            
            var queryGenerator = new Mock<IMatchingObservationsQueryGenerator>(Strict);

            var tempTable1 = new Mock<ITempTableQuery<IdTempTable>>(Strict);
            var tempTable2 = new Mock<ITempTableQuery<IdTempTable>>(Strict);
            ListOf(tempTable1, tempTable2)
                .ForEach(tempTable => tempTable
                    .Setup(t => t.DisposeAsync())
                    .Returns(new ValueTask()));
            
            var tempTableList = ListOf(tempTable1.Object, tempTable2.Object)
                .Cast<IAsyncDisposable>()
                .ToList();
            
            queryGenerator
                .Setup(s => s
                    .GetMatchingObservationsQuery(
                        context, 
                        observationQueryContext.SubjectId, 
                        ItIs.ListSequenceEqualTo(observationQueryContext.Filters),
                        ItIs.ListSequenceEqualTo(observationQueryContext.LocationIds),
                        observationQueryContext.TimePeriod,
                        cancellationToken))
                .ReturnsAsync((sql, sqlParameters, tempTableList));

            var sqlExecutor = new Mock<IRawSqlExecutor>(Strict);

            sqlExecutor
                .Setup(s => s.ExecuteSqlRaw(context, sql, sqlParameters, cancellationToken))
                .Returns(Task.CompletedTask);

            var service = BuildService(
                context, 
                queryGenerator.Object,
                sqlExecutor.Object);
            
            await service.GetMatchedObservations(observationQueryContext, cancellationToken);
            VerifyAllMocks(queryGenerator, sqlExecutor, tempTable1, tempTable2);
        }
        
        [Fact]
        public async Task QueryGenerator_MinimalQuery()
        {
            var subjectId = Guid.NewGuid();
            await using var context = InMemoryStatisticsDbContext();
            
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var tempTableCreator = new Mock<MatchingObservationsQueryGenerator.ITemporaryTableCreator>();
            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedObservation>(context, cancellationToken))
                .Returns(Task.CompletedTask);
            
            var queryGenerator = new MatchingObservationsQueryGenerator
            {
                TempTableCreator = tempTableCreator.Object
            };

            var (sql, sqlParameters, tempTableReferences) = 
                await queryGenerator
                    .GetMatchingObservationsQuery(
                        context, 
                        subjectId,
                        null,
                        null,
                        null,
                        cancellationToken);

            VerifyAllMocks(tempTableCreator);
            
            const string expectedSql = @"
                  INSERT INTO #MatchedObservation 
                  SELECT o.id FROM Observation o
                  WHERE o.SubjectId = @subjectId
                  ORDER BY o.Id;";
            
            Assert.Equal(FormatSql(expectedSql), FormatSql(sql));
            sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
            Assert.Empty(tempTableReferences);
        }
        
        [Fact]
        public async Task QueryGenerator_TimePeriodQuery()
        {
            var subjectId = Guid.NewGuid();
            await using var context = InMemoryStatisticsDbContext();
            
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var tempTableCreator = new Mock<MatchingObservationsQueryGenerator.ITemporaryTableCreator>();
            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedObservation>(context, cancellationToken))
                .Returns(Task.CompletedTask);
            
            var queryGenerator = new MatchingObservationsQueryGenerator
            {
                TempTableCreator = tempTableCreator.Object
            };

            var (sql, sqlParameters, tempTableReferences) = 
                await queryGenerator
                    .GetMatchingObservationsQuery(
                        context, 
                        subjectId,
                        null,
                        null,
                        new TimePeriodQuery
                        {
                            StartCode = TimeIdentifier.AcademicYear,
                            StartYear = 2015,
                            EndCode = TimeIdentifier.AcademicYear,
                            EndYear = 2017
                        },
                        cancellationToken);

            VerifyAllMocks(tempTableCreator);
            
            const string expectedSql = @"
                  INSERT INTO #MatchedObservation 
                  SELECT o.id FROM Observation o
                  WHERE o.SubjectId = @subjectId
                  AND (
                    (o.TimeIdentifier = 'AY' AND o.Year = 2015) OR 
                    (o.TimeIdentifier = 'AY' AND o.Year = 2016) OR 
                    (o.TimeIdentifier = 'AY' AND o.Year = 2017)
                  ) 
                  ORDER BY o.Id;";
            
            Assert.Equal(FormatSql(expectedSql), FormatSql(sql));
            sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
            Assert.Empty(tempTableReferences);
        }
        
        [Fact]
        public async Task QueryGenerator_LocationIds()
        {
            var locationIds = ListOf(Guid.NewGuid(), Guid.NewGuid());
            var locationIdsTempTableEntries = locationIds.Select(id => new IdTempTable(id));

            var subjectId = Guid.NewGuid();
            await using var context = InMemoryStatisticsDbContext();
            
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var tempTableCreator = new Mock<MatchingObservationsQueryGenerator.ITemporaryTableCreator>();
            
            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedObservation>(context, cancellationToken))
                .Returns(Task.CompletedTask);
            
            var locationIdsTempTableReference = new Mock<ITempTableQuery<IdTempTable>>();
            locationIdsTempTableReference
                .SetupGet(t => t.Name)
                .Returns("#LocationTempTable");
            
            tempTableCreator
                .Setup(s => s
                    .CreateTemporaryTableAndPopulate(
                        context, 
                        locationIdsTempTableEntries, 
                        cancellationToken))
                .ReturnsAsync(locationIdsTempTableReference.Object);

            var queryGenerator = new MatchingObservationsQueryGenerator
            {
                TempTableCreator = tempTableCreator.Object
            };

            var (sql, sqlParameters, tempTableReferences) = 
                await queryGenerator
                    .GetMatchingObservationsQuery(
                        context, 
                        subjectId,
                        null,
                        locationIds,
                        null,
                        cancellationToken);

            VerifyAllMocks(tempTableCreator, locationIdsTempTableReference);
            
            const string expectedSql = @"
                  INSERT INTO #MatchedObservation 
                  SELECT o.id FROM Observation o
                  WHERE o.SubjectId = @subjectId
                  AND (o.LocationId IN (SELECT Id FROM #LocationTempTable)) 
                  ORDER BY o.Id;";
            
            Assert.Equal(FormatSql(expectedSql), FormatSql(sql));
            sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
            Assert.Equal(locationIdsTempTableReference.Object, Assert.Single(tempTableReferences));
        }
        
        [Fact]
        public async Task QueryGenerator_FilterItems()
        {
            var subjectId = Guid.NewGuid();

            var filter1 = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                FilterGroups = CreateFilterGroups(5, 5)
            };
            
            var filter2 = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                FilterGroups = CreateFilterGroups(10)
            };
            
            var filter3 = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                FilterGroups = CreateFilterGroups(2, 2, 2, 2, 2)
            };
            
            var unselectedFilter4 = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                FilterGroups = CreateFilterGroups(2)
            };
            
            var unrelatedFilter = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                FilterGroups = CreateFilterGroups(5)
            };
            
            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddRangeAsync(
                    filter1, 
                    filter2, 
                    filter3, 
                    unselectedFilter4, 
                    unrelatedFilter);
                
                await context.SaveChangesAsync();
            }
            
            var tempTableCreator = new Mock<MatchingObservationsQueryGenerator.ITemporaryTableCreator>(Strict);
            
            var queryGenerator = new MatchingObservationsQueryGenerator
            {
                TempTableCreator = tempTableCreator.Object
            };

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                // Set up some selected Filter Item Ids from 3 of the 4 Filters on this Subject.
                //
                // Filter1 has the least Filter Items selected and will therefore appear first 
                // in the list of EXISTS clauses, in order for it to filter as many table rows down
                // as possible before the next EXISTS clause is evaluated.
                //
                // Filter3 then has the second lowest number of Filter Items selected so it will
                // appear as the second EXISTS clause, and finally Filter3 will be the final EXISTS
                // clause.
                var selectedFilter1FilterItemIds = ListOf(
                    filter1.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                    filter1.FilterGroups.ToList()[0].FilterItems.ToList()[2].Id,
                    filter1.FilterGroups.ToList()[1].FilterItems.ToList()[1].Id);
                
                var selectedFilter2FilterItemIds = ListOf(
                    filter2.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                    filter2.FilterGroups.ToList()[0].FilterItems.ToList()[2].Id,
                    filter2.FilterGroups.ToList()[0].FilterItems.ToList()[4].Id,
                    filter2.FilterGroups.ToList()[0].FilterItems.ToList()[3].Id,
                    filter2.FilterGroups.ToList()[0].FilterItems.ToList()[7].Id,
                    filter2.FilterGroups.ToList()[0].FilterItems.ToList()[6].Id,
                    filter2.FilterGroups.ToList()[0].FilterItems.ToList()[8].Id);
                
                var selectedFilter3FilterItemIds = ListOf(
                    filter3.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                    filter3.FilterGroups.ToList()[0].FilterItems.ToList()[1].Id,
                    filter3.FilterGroups.ToList()[1].FilterItems.ToList()[0].Id,
                    filter3.FilterGroups.ToList()[2].FilterItems.ToList()[1].Id,
                    filter3.FilterGroups.ToList()[3].FilterItems.ToList()[0].Id,
                    filter3.FilterGroups.ToList()[4].FilterItems.ToList()[1].Id);

                // An accidental inclusion of a Filter Item Id that doesn't belong to a
                // Filter on this Subject will be ignored.
                var invalidUnrelatedFilterFilterItemIds = ListOf(
                    unrelatedFilter.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id);

                var selectFilterItemIds = selectedFilter1FilterItemIds
                    .Concat(selectedFilter2FilterItemIds)
                    .Concat(selectedFilter3FilterItemIds)
                    .Concat(invalidUnrelatedFilterFilterItemIds)
                    .ToList();

                tempTableCreator
                    .Setup(s => 
                        s.CreateTemporaryTable<MatchedObservation>(context, cancellationToken))
                    .Returns(Task.CompletedTask);

                var selectedFilterItemsByFilter = ListOf(
                    selectedFilter1FilterItemIds,
                    selectedFilter2FilterItemIds, 
                    selectedFilter3FilterItemIds);

                var selectedFilterItemTempTableEntriesByFilter = selectedFilterItemsByFilter
                    .Select(filterItemIds => filterItemIds
                        .Select(id => new IdTempTable(id))
                        .ToList())
                    .ToList();

                var filterItemIdTempTableReferences = 
                    selectedFilterItemTempTableEntriesByFilter
                        .Select(tempTableEntries =>
                        {
                            var filterNumber = selectedFilterItemTempTableEntriesByFilter.IndexOf(tempTableEntries) + 1;

                            var orderedIds = tempTableEntries
                                .OrderBy(t => t.Id)
                                .ToList();
                            
                            var tempTableReference = new Mock<ITempTableQuery<IdTempTable>>();
                            tempTableReference
                                .SetupGet(t => t.Name)
                                .Returns($"#Filter{filterNumber}TempTable");

                            tempTableCreator
                                .Setup(s =>
                                    s.CreateTemporaryTableAndPopulate(
                                        context,
                                        ItIs.ListSequenceEqualTo(orderedIds),
                                        cancellationToken))
                                .ReturnsAsync(tempTableReference.Object);

                            return tempTableReference;
                        })
                        .ToList();
                
                var (sql, sqlParameters, tempTableReferences) = 
                    await queryGenerator
                        .GetMatchingObservationsQuery(
                            context, 
                            subjectId,
                            selectFilterItemIds,
                            null,
                            null,
                            cancellationToken);
                
                VerifyAllMocks(tempTableCreator);
                VerifyAllMocks(filterItemIdTempTableReferences.Cast<Mock>().ToArray());

                // Ensure that the EXISTS clauses appear in the expected order in the generated query,
                // with the Filter with the least number of Filter Items chosen being the first. 
                const string expectedSql = @"
                      INSERT INTO #MatchedObservation 
                      SELECT o.id FROM Observation o
                      WHERE o.SubjectId = @subjectId 
                      AND (
                          EXISTS (SELECT 1 FROM ObservationFilterItem ofi 
                                  WHERE ofi.ObservationId = o.id 
                                  AND ofi.FilterItemId IN (SELECT Id FROM #Filter1TempTable)) AND
                          EXISTS (SELECT 1 FROM ObservationFilterItem ofi 
                                  WHERE ofi.ObservationId = o.id 
                                  AND ofi.FilterItemId IN (SELECT Id FROM #Filter3TempTable)) AND
                          EXISTS (SELECT 1 FROM ObservationFilterItem ofi 
                                  WHERE ofi.ObservationId = o.id 
                                  AND ofi.FilterItemId IN (SELECT Id FROM #Filter2TempTable))
                      )
                      ORDER BY o.Id;";
                
                Assert.Equal(FormatSql(expectedSql), FormatSql(sql));
                
                sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));

                var expectedTempTableReferences = 
                    filterItemIdTempTableReferences.Select(mock => mock.Object);

                // Assert that the temporary table for the LocationIds, and the 3 temporary tables for the
                // 3 Filters' sets of selected FilterItemIds are returned so that the calling code can
                // dispose of them as it needs to (the order they are returned in is not important).
                Assert.Equal(
                    expectedTempTableReferences.OrderBy(t => t.GetHashCode()), 
                    tempTableReferences.OrderBy(t => t.GetHashCode()));
            }
        }
        
        [Fact]
        public async Task QueryGenerator_FullQuery()
        {
            var subjectId = Guid.NewGuid();

            var filter = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                FilterGroups = CreateFilterGroups(5, 5)
            };
            
            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(filter);
                await context.SaveChangesAsync();
            }
            
            var tempTableCreator = new Mock<MatchingObservationsQueryGenerator.ITemporaryTableCreator>(Strict);
            
            var queryGenerator = new MatchingObservationsQueryGenerator
            {
                TempTableCreator = tempTableCreator.Object
            };

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var selectedFilterItemIds = ListOf(
                    filter.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                    filter.FilterGroups.ToList()[0].FilterItems.ToList()[2].Id,
                    filter.FilterGroups.ToList()[1].FilterItems.ToList()[1].Id);
                
                tempTableCreator
                    .Setup(s => 
                        s.CreateTemporaryTable<MatchedObservation>(context, cancellationToken))
                    .Returns(Task.CompletedTask);

                var selectedLocationIds = ListOf(Guid.NewGuid(), Guid.NewGuid());
                
                var selectedLocationIdTempTableEntries = selectedLocationIds
                        .Select(id => new IdTempTable(id))
                        .ToList();

                var locationIdsTempTableReference = new Mock<ITempTableQuery<IdTempTable>>();
                locationIdsTempTableReference
                    .SetupGet(t => t.Name)
                    .Returns("#LocationTempTable");
                
                tempTableCreator
                    .Setup(s =>
                        s.CreateTemporaryTableAndPopulate(
                            context,
                            ItIs.EnumerableSequenceEqualTo(selectedLocationIdTempTableEntries),
                            cancellationToken))
                    .ReturnsAsync(locationIdsTempTableReference.Object);

                var filterItemIdsTempTableReference = new Mock<ITempTableQuery<IdTempTable>>();
                filterItemIdsTempTableReference
                    .SetupGet(t => t.Name)
                    .Returns("#FilterTempTable");

                tempTableCreator
                    .Setup(s =>
                        s.CreateTemporaryTableAndPopulate(
                            context,
                            ItIs.ListSequenceEqualTo(
                                selectedFilterItemIds
                                    .OrderBy(id => id)
                                    .Select(id => new IdTempTable(id))
                                    .ToList()),
                            cancellationToken))
                    .ReturnsAsync(filterItemIdsTempTableReference.Object);     
                
                var tempTableMocks = ListOf(locationIdsTempTableReference);
                tempTableMocks.Add(filterItemIdsTempTableReference);            

                var (sql, sqlParameters, tempTableReferences) = 
                    await queryGenerator
                        .GetMatchingObservationsQuery(
                            context, 
                            subjectId,
                            selectedFilterItemIds,
                            selectedLocationIds,
                            new TimePeriodQuery
                            {
                                StartCode = TimeIdentifier.AcademicYear,
                                StartYear = 2015,
                                EndCode = TimeIdentifier.AcademicYear,
                                EndYear = 2017
                            },
                            cancellationToken);
                
                VerifyAllMocks(tempTableCreator);
                VerifyAllMocks(tempTableMocks.Cast<Mock>().ToArray());

                const string expectedSql = @"
                      INSERT INTO #MatchedObservation 
                      SELECT o.id FROM Observation o
                      WHERE o.SubjectId = @subjectId 
                      AND (
                        (o.TimeIdentifier = 'AY' AND o.Year = 2015) OR 
                        (o.TimeIdentifier = 'AY' AND o.Year = 2016) OR 
                        (o.TimeIdentifier = 'AY' AND o.Year = 2017)
                      ) 
                      AND (o.LocationId IN (SELECT Id FROM #LocationTempTable)) 
                      AND (
                          EXISTS (SELECT 1 FROM ObservationFilterItem ofi 
                                  WHERE ofi.ObservationId = o.id 
                                  AND ofi.FilterItemId IN (SELECT Id FROM #FilterTempTable))
                      )
                      ORDER BY o.Id;";
                
                Assert.Equal(FormatSql(expectedSql), FormatSql(sql));
                
                sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));

                var expectedTempTableReferences = 
                    tempTableMocks.Select(mock => mock.Object);

                // Assert that the temporary table for the LocationIds, and the 3 temporary tables for the
                // 3 Filters' sets of selected FilterItemIds are returned so that the calling code can
                // dispose of them as it needs to (the order they are returned in is not important).
                Assert.Equal(
                    expectedTempTableReferences.OrderBy(t => t.GetHashCode()), 
                    tempTableReferences.OrderBy(t => t.GetHashCode()));
            }
        }

        private static string FormatSql(string sql)
        {
            var removeNewLines = new Regex("\n", RegexOptions.Compiled);
            var removeWhitespaceAfterOpeningBracket = new Regex("\\( *", RegexOptions.Compiled);
            var removeWhitespaceBeforeClosingBracket = new Regex(" *\\)", RegexOptions.Compiled);
            var removeExtraWhitespace = new Regex("[ ]{2,}", RegexOptions.Compiled);

            var newLinesStripped = removeNewLines.Replace(sql, "");
            var openingBracketWhitespaceStripped = removeWhitespaceAfterOpeningBracket
                .Replace(newLinesStripped, "(");
            var closingBracketWhitespaceStripped = removeWhitespaceBeforeClosingBracket
                .Replace(openingBracketWhitespaceStripped, ")");
            return removeExtraWhitespace
                .Replace(closingBracketWhitespaceStripped, " ")
                .Trim();
        }

        private static ObservationService BuildService(
            StatisticsDbContext context,
            IMatchingObservationsQueryGenerator? queryGenerator = null,
            IRawSqlExecutor? sqlExecutor = null)
        {
            var service = new ObservationService(context, Mock.Of<ILogger<ObservationService>>())
            {
                QueryGenerator = queryGenerator ?? Mock.Of<IMatchingObservationsQueryGenerator>(Strict),
                SqlExecutor = sqlExecutor ?? Mock.Of<IRawSqlExecutor>(Strict)
            };
            
            return service;
        }

        private static List<FilterGroup> CreateFilterGroups(params int[] numberOfFilterItemsPerFilterGroup)
        {
            return numberOfFilterItemsPerFilterGroup
                .Select(filterItemCount => new FilterGroup
                {
                    Id = Guid.NewGuid(),
                    FilterItems = Enumerable
                        .Range(0, filterItemCount)
                        .Select(_ => new FilterItem
                        {
                            Id = Guid.NewGuid()
                        })
                        .ToList()
                })
                .ToList();
        }
    }
}

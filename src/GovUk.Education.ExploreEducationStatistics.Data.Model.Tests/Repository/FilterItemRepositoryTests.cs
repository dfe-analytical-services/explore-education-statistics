#nullable enable
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using Thinktecture.EntityFrameworkCore.TempTables;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.FilterItemRepository;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository;

public class FilterItemRepositoryTests
{
    private static DataFixture Fixture = new();
        
    [Fact]
    public async Task GetMatchedFilterItems_QueryGeneratedAndExecuted()
    {
        var subjectId = Guid.NewGuid();
        
        await using var context = InMemoryStatisticsDbContext();
        
        const string sql = "Some SQL here";

        var queryGenerator = new Mock<IMatchedFilterItemsQueryGenerator>(Strict);

        var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
        matchingObservationsTable
            .SetupGet(t => t.Name)
            .Returns("#MatchedObservation");
        
        var matchedFilterItemsTable = new Mock<ITempTableReference>(Strict);
        matchedFilterItemsTable
            .SetupGet(t => t.Name)
            .Returns("#MatchedFilterItem");

        queryGenerator
            .Setup(s => s
                .GetMatchedFilterItemsQuery(
                    context,
                    matchingObservationsTable.Object,
                    default))
            .ReturnsAsync(sql);

        var sqlExecutor = new Mock<IRawSqlExecutor>(Strict);

        sqlExecutor
            .Setup(s => s.ExecuteSqlRaw(context, sql, new List<SqlParameter>(), default))
            .Returns(Task.CompletedTask);
        
        var service = BuildFilterItemRepository(
            context: context,
            sqlExecutor: sqlExecutor.Object,
            queryGenerator: queryGenerator.Object);

        await service.GetFilterItemsFromMatchedObservationIds(
            subjectId,
            matchingObservationsTable.Object,
            default);
        
        VerifyAllMocks(queryGenerator, sqlExecutor);
    }
    
    [Fact]
    public async Task GetMatchedFilterItems_MatchedFilterItemsUsedToReturnFilterItems()
    {
        // Create 4 filters for this Subject.
        var filters = Fixture
            .DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
            .WithSubject(Fixture.DefaultSubject())
            .GenerateList(4);

        var subjectId = filters[0].SubjectId;
        
        // We have matched a FilterItem from Filter 1, Filter Group 2.
        var matchedFilter1Group2Item2 = filters[0].FilterGroups[1].FilterItems[1];
        
        // We have matched 2 FilterItems from Filter 2, Filter Group 1.
        var matchedFilter2Group1Item1 = filters[1].FilterGroups[0].FilterItems[0];
        var matchedFilter2Group1Item2 = filters[1].FilterGroups[0].FilterItems[1];
        
        // We have matched 2 FilterItems from Filter 3, from different Filter Groups.
        var matchedFilter3Group1Item1 = filters[2].FilterGroups[0].FilterItems[0];
        var matchedFilter3Group2Item1 = filters[2].FilterGroups[1].FilterItems[0];
        
        await using var context = InMemoryStatisticsDbContext();
        
        context.Filter.AddRange(filters);
        await context.SaveChangesAsync();

        FilterItem[] expectedMatchedFilterItems =
        [
            matchedFilter1Group2Item2,
            matchedFilter2Group1Item1,
            matchedFilter2Group1Item2,
            matchedFilter3Group1Item1,
            matchedFilter3Group2Item1
        ];
        
        // Fake populating the temporary table with results from executing the generated
        // query from the MatchedFilterItemsQueryGenerator. 
        context.MatchedFilterItems.AddRange(
            expectedMatchedFilterItems.Select(fi => new MatchedFilterItem(fi.Id)));
        await context.SaveChangesAsync();
        
        var matchingObservationsTable = new Mock<ITempTableReference>(Loose);
        var sqlExecutor = new Mock<IRawSqlExecutor>(Loose);
        var queryGenerator = new Mock<IMatchedFilterItemsQueryGenerator>(Loose);

        var service = BuildFilterItemRepository(
            context: context,
            sqlExecutor: sqlExecutor.Object,
            queryGenerator: queryGenerator.Object);

        var matchedFilterItems = await service.GetFilterItemsFromMatchedObservationIds(
            subjectId,
            matchingObservationsTable.Object,
            default);
        
        Assert.Equal(
            expectedMatchedFilterItems.OrderBy(f => f.Id),
            matchedFilterItems.OrderBy(f => f.Id));
    }
    
    [Fact]
    public async Task MatchedFilterItemsQueryGenerator()
    {
        await using var context = InMemoryStatisticsDbContext();
        
        var tempTableCreator = new Mock<ITemporaryTableCreator>();

        var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
        matchingObservationsTable
            .SetupGet(t => t.Name)
            .Returns("#MatchedObservation");

        var matchedFilterItemsTable = new Mock<ITempTableReference>(Strict);
        matchedFilterItemsTable
            .SetupGet(t => t.Name)
            .Returns("#MatchedFilterItem");

        tempTableCreator
            .Setup(s =>
                s.CreateTemporaryTable<MatchedFilterItem, StatisticsDbContext>(context, default))
            .ReturnsAsync(matchedFilterItemsTable.Object);
        
        var queryGenerator = new MatchedFilterItemsQueryGenerator
        {
            TempTableCreator = tempTableCreator.Object
        };

        var sql = 
            await queryGenerator
                .GetMatchedFilterItemsQuery(
                    context,
                    matchingObservationsTable.Object,
                    default);

        VerifyAllMocks(tempTableCreator);
        
        var expectedSql = $@"
            INSERT INTO {matchedFilterItemsTable.Object.Name} WITH (TABLOCK)
            SELECT DISTINCT o.FilterItemId
            FROM {matchingObservationsTable.Object.Name} AS mo
            JOIN ObservationFilterItem AS o
              ON o.ObservationId = mo.Id
            OPTION(RECOMPILE, MAXDOP 4);";

        AssertInsertIntoMatchedFilterItemsCorrect(sql, expectedSql);
    }
    
    [Fact]
    public async Task CountFilterItemsByFilter()
    {
        var filterItemCharacteristicSchoolYear1 = new FilterItem();
        var filterItemCharacteristicFsmEligible = new FilterItem();
        var filterItemCharacteristicFsmNotEligible = new FilterItem();
        var filterItemSchoolTypePrimary = new FilterItem();
        var filterItemSchoolTypeSecondary = new FilterItem();

        var filterCharacteristic = new Filter
        {
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        filterItemCharacteristicSchoolYear1
                    }
                },
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        filterItemCharacteristicFsmEligible,
                        filterItemCharacteristicFsmNotEligible
                    }
                }
            }
        };

        var filterSchoolType = new Filter
        {
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        filterItemSchoolTypePrimary,
                        filterItemSchoolTypeSecondary
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Filter.AddRangeAsync(filterCharacteristic, filterSchoolType);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var repository = BuildFilterItemRepository(context);
            var filterItemIds = new List<Guid>
            {
                filterItemCharacteristicSchoolYear1.Id,
                filterItemCharacteristicFsmEligible.Id,
                filterItemCharacteristicFsmNotEligible.Id,
                filterItemSchoolTypePrimary.Id,
                filterItemSchoolTypeSecondary.Id
            };
            var result = await repository.CountFilterItemsByFilter(filterItemIds);

            // Result should contain the counts of filter items in both filters
            Assert.Equal(2, result.Count);

            // 3 of the filter items belong to the Characteristic filter 
            Assert.Equal(3, result[filterCharacteristic.Id]);

            // 2 of the filter items belong to the School Type filter
            Assert.Equal(2, result[filterSchoolType.Id]);
        }
    }

    [Fact]
    public async Task CountFilterItemsByFilter_EmptyFilterItemsIsEmpty()
    {
        var filter = new Filter
        {
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        new()
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Filter.AddRangeAsync(filter);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var repository = BuildFilterItemRepository(context);
            var result = await repository.CountFilterItemsByFilter(new List<Guid>());
            Assert.Empty(result);
        }
    }

    [Fact]
    public async Task CountFilterItemsByFilter_FilterItemsNotFoundThrowsException()
    {
        var filterItem = new FilterItem();
        var filter = new Filter
        {
            FilterGroups = new List<FilterGroup>
            {
                new()
                {
                    FilterItems = new List<FilterItem>
                    {
                        filterItem
                    }
                }
            }
        };

        var statisticsDbContextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            await statisticsDbContext.Filter.AddRangeAsync(filter);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var context = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            var repository = BuildFilterItemRepository(context);

            var filterItemNotFound1 = Guid.NewGuid();
            var filterItemNotFound2 = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await repository.CountFilterItemsByFilter(
                    ListOf(
                        filterItem.Id,
                        filterItemNotFound1,
                        filterItemNotFound2
                    ));
            });

            Assert.Equal($"Could not find filter items: {filterItemNotFound1}, {filterItemNotFound2}", exception.Message);
        }
    }

    private static FilterItemRepository BuildFilterItemRepository(
        StatisticsDbContext context,
        IRawSqlExecutor? sqlExecutor = null,
        IMatchedFilterItemsQueryGenerator? queryGenerator = null)
    {
        return new(context, Mock.Of<ILogger<FilterItemRepository>>())
        {
            SqlExecutor = sqlExecutor ?? Mock.Of<IRawSqlExecutor>(Strict),
            QueryGenerator = queryGenerator ?? Mock.Of<IMatchedFilterItemsQueryGenerator>(Strict)
        };
    }
    
    private static void AssertInsertIntoMatchedFilterItemsCorrect(string sql, string expectedSql)
    {
        // Check the expected query is present to insert matching Observation Ids into
        // the temp table.
        var actualSql = FormatSql(sql);
        Assert.StartsWith(FormatSql(expectedSql), actualSql);

        // Check the expected index is applied to the temp table after the insert.
        var restOfSql = actualSql.Split(FormatSql(expectedSql))[1].TrimStart();
        var indexSqlPattern = new Regex(
            @"CREATE UNIQUE CLUSTERED INDEX \[IX_#MatchedFilterItem_Id_.{36}\].* " +
            @"ON #MatchedFilterItem\(Id\) WITH \(MAXDOP = 4\);");
        Assert.Matches(indexSqlPattern, restOfSql);
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
}

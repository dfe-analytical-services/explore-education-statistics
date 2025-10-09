#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.SqlTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ObservationService;
using static Moq.MockBehavior;

// ReSharper disable AccessToDisposedClosure
namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

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

        var fullTableQuery = new FullTableQuery
        {
            SubjectId = Guid.NewGuid(),
            Filters = ListOf(Guid.NewGuid()),
            LocationIds = ListOf(Guid.NewGuid()),
            TimePeriod = new TimePeriodQuery(),
        };

        var queryGenerator = new Mock<IMatchingObservationsQueryGenerator>(Strict);

        var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
        matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

        queryGenerator
            .Setup(s =>
                s.GetMatchingObservationsQuery(
                    context,
                    fullTableQuery.SubjectId,
                    ItIs.ListSequenceEqualTo(fullTableQuery.GetFilterItemIds()),
                    ItIs.ListSequenceEqualTo(fullTableQuery.LocationIds),
                    fullTableQuery.TimePeriod,
                    cancellationToken
                )
            )
            .ReturnsAsync((sql, sqlParameters, matchingObservationsTable.Object));

        var sqlExecutor = new Mock<IRawSqlExecutor>(Strict);

        sqlExecutor
            .Setup(s => s.ExecuteSqlRaw(context, sql, sqlParameters, cancellationToken))
            .Returns(Task.CompletedTask);

        var service = BuildService(context, queryGenerator.Object, sqlExecutor.Object);

        await service.GetMatchedObservations(fullTableQuery, cancellationToken);
        VerifyAllMocks(queryGenerator, sqlExecutor);
    }

    [Fact]
    public async Task QueryGenerator_MinimalQuery()
    {
        var subjectId = Guid.NewGuid();
        await using var context = InMemoryStatisticsDbContext();

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var tempTableCreator = new Mock<ITemporaryTableCreator>();

        var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
        matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

        tempTableCreator
            .Setup(s => s.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(context, cancellationToken))
            .ReturnsAsync(matchingObservationsTable.Object);

        var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

        sqlHelper
            .Setup(s => s.CreateRandomIndexName("#MatchedObservation", "Id"))
            .Returns("IX_#MatchedObservation_Id_1234");

        var queryGenerator = new MatchingObservationsQueryGenerator(
            tempTableCreator: tempTableCreator.Object,
            sqlHelper: sqlHelper.Object
        );

        var (sql, sqlParameters, _) = await queryGenerator.GetMatchingObservationsQuery(
            context,
            subjectId,
            [],
            [],
            null,
            cancellationToken
        );

        VerifyAllMocks(tempTableCreator, sqlHelper);

        const string expectedSql =
            @"
            INSERT INTO #MatchedObservation WITH (TABLOCK)
            SELECT o.id FROM Observation o
            WHERE o.SubjectId = @subjectId
            OPTION(RECOMPILE, MAXDOP 4);
                
            CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedObservation_Id_1234]
            ON #MatchedObservation(Id) WITH (MAXDOP = 4);

            UPDATE STATISTICS #MatchedObservation WITH FULLSCAN;
            ";

        Assert.Equal(NormaliseSqlFormatting(expectedSql), NormaliseSqlFormatting(sql));

        sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
    }

    [Fact]
    public async Task QueryGenerator_TimePeriodQuery()
    {
        var subjectId = Guid.NewGuid();
        await using var context = InMemoryStatisticsDbContext();

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var tempTableCreator = new Mock<ITemporaryTableCreator>();

        var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
        matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

        tempTableCreator
            .Setup(s => s.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(context, cancellationToken))
            .ReturnsAsync(matchingObservationsTable.Object);

        var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

        sqlHelper
            .Setup(s => s.CreateRandomIndexName("#MatchedObservation", "Id"))
            .Returns("IX_#MatchedObservation_Id_1234");

        var queryGenerator = new MatchingObservationsQueryGenerator(
            tempTableCreator: tempTableCreator.Object,
            sqlHelper: sqlHelper.Object
        );

        var (sql, sqlParameters, _) = await queryGenerator.GetMatchingObservationsQuery(
            context,
            subjectId,
            [],
            [],
            new TimePeriodQuery
            {
                StartCode = TimeIdentifier.AcademicYear,
                StartYear = 2015,
                EndCode = TimeIdentifier.AcademicYear,
                EndYear = 2017,
            },
            cancellationToken
        );

        VerifyAllMocks(tempTableCreator, sqlHelper);

        const string expectedSql =
            @"
                INSERT INTO #MatchedObservation WITH (TABLOCK)
                SELECT o.id FROM Observation o
                WHERE o.SubjectId = @subjectId
                AND (
                  (o.TimeIdentifier = 'AY' AND o.Year = 2015) OR
                  (o.TimeIdentifier = 'AY' AND o.Year = 2016) OR
                  (o.TimeIdentifier = 'AY' AND o.Year = 2017)
                )
                OPTION(RECOMPILE, MAXDOP 4);
                
                CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedObservation_Id_1234]
                ON #MatchedObservation(Id) WITH (MAXDOP = 4);

                UPDATE STATISTICS #MatchedObservation WITH FULLSCAN;
            ";

        Assert.Equal(NormaliseSqlFormatting(expectedSql), NormaliseSqlFormatting(sql));

        sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
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

        var tempTableCreator = new Mock<ITemporaryTableCreator>();

        var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
        matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

        tempTableCreator
            .Setup(s => s.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(context, cancellationToken))
            .ReturnsAsync(matchingObservationsTable.Object);

        var locationIdsTempTableReference = new Mock<ITempTableQuery<IdTempTable>>();
        locationIdsTempTableReference.SetupGet(t => t.Name).Returns("#LocationTempTable");

        tempTableCreator
            .Setup(s =>
                s.CreateAnonymousTemporaryTableAndPopulate(context, locationIdsTempTableEntries, cancellationToken)
            )
            .ReturnsAsync(locationIdsTempTableReference.Object);

        var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

        sqlHelper
            .Setup(s => s.CreateRandomIndexName("#MatchedObservation", "Id"))
            .Returns("IX_#MatchedObservation_Id_1234");

        var queryGenerator = new MatchingObservationsQueryGenerator(
            tempTableCreator: tempTableCreator.Object,
            sqlHelper: sqlHelper.Object
        );

        var (sql, sqlParameters, _) = await queryGenerator.GetMatchingObservationsQuery(
            context,
            subjectId,
            [],
            locationIds,
            null,
            cancellationToken
        );

        VerifyAllMocks(tempTableCreator, locationIdsTempTableReference);

        const string expectedSql =
            @"
                INSERT INTO #MatchedObservation WITH (TABLOCK)
                SELECT o.id FROM Observation o
                WHERE o.SubjectId = @subjectId
                AND (o.LocationId IN (SELECT Id FROM #LocationTempTable))
                OPTION(RECOMPILE, MAXDOP 4);
                
                CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedObservation_Id_1234]
                ON #MatchedObservation(Id) WITH (MAXDOP = 4);

                UPDATE STATISTICS #MatchedObservation WITH FULLSCAN;
                ";

        Assert.Equal(NormaliseSqlFormatting(expectedSql), NormaliseSqlFormatting(sql));

        sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task QueryGenerator_FilterItems_LessThanOrHalfItemsChosenForFilter_NotExistsCheck(
        int numberOfSelectedItems
    )
    {
        var subjectId = Guid.NewGuid();

        // Create a Filter with 6 Filter Items in total.
        var filter = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(numberOfFilterItemsPerFilterGroup: 6),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            await context.AddRangeAsync(filter);
            await context.SaveChangesAsync();
        }

        var tempTableCreator = new Mock<ITemporaryTableCreator>(Strict);

        var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

        sqlHelper
            .Setup(s => s.CreateRandomIndexName("#MatchedObservation", "Id"))
            .Returns("IX_#MatchedObservation_Id_1234");

        var queryGenerator = new MatchingObservationsQueryGenerator(
            tempTableCreator: tempTableCreator.Object,
            sqlHelper: sqlHelper.Object
        );

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // We are selecting half or less of the Filter Items out of the
            // available 6 for the Filter, meaning that it is cheaper to exclude
            // Observations by checking for the *non-existence* of these Filter
            // Items against Observation rows.
            var selectedFilterItemIds = filter
                .FilterGroups[0]
                .FilterItems.Take(numberOfSelectedItems)
                .Select(fi => fi.Id)
                .ToList();

            var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
            matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(context, cancellationToken))
                .ReturnsAsync(matchingObservationsTable.Object);

            var selectedFilterItemTempTableEntriesForFilter = selectedFilterItemIds
                .Select(id => new IdTempTable(id))
                .ToList();

            var filterItemIdTempTableReference = GenerateSetupsForFilterItemTempTable(
                filterNumber: 1,
                selectedFilterItemTempTableEntriesForFilter,
                tempTableCreator,
                context,
                cancellationToken
            );

            var (sql, sqlParameters, _) = await queryGenerator.GetMatchingObservationsQuery(
                context,
                subjectId,
                selectedFilterItemIds,
                [],
                null,
                cancellationToken
            );

            VerifyAllMocks(tempTableCreator, sqlHelper, filterItemIdTempTableReference);

            //
            // Ensure that the DELETE statement uses a NOT EXISTS check against
            // its temp table, that includes the Filter Item Ids selected by
            // the user. This allows the DELETE statement to exclude any Observations
            // that don't have any of the Filter Items selected for that particular
            // Filter in the most efficient manner, by only evaluating the smaller
            // number of Filter Item Ids.
            //
            const string expectedSql =
                @"
                    INSERT INTO #MatchedObservation WITH (TABLOCK)
                    SELECT o.id FROM Observation o
                    WHERE o.SubjectId = @subjectId
                    OPTION(RECOMPILE, MAXDOP 4);
                
                    CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedObservation_Id_1234]
                    ON #MatchedObservation(Id) WITH (MAXDOP = 4);

                    DELETE CandidateObservation WITH (TABLOCK)
                    FROM #MatchedObservation CandidateObservation
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM ObservationFilterItem OFI
                        JOIN #Filter1TempTable SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                        WHERE OFI.ObservationId = CandidateObservation.Id
                    )
                    OPTION(RECOMPILE, MAXDOP 4);

                    UPDATE STATISTICS #MatchedObservation WITH FULLSCAN;
                    ";

            Assert.Equal(NormaliseSqlFormatting(expectedSql), NormaliseSqlFormatting(sql));

            sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
        }
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public async Task QueryGenerator_FilterItems_MoreThanHalfItemsChosenForFilter_ExistsCheck(int numberOfSelectedItems)
    {
        var subjectId = Guid.NewGuid();

        // Create a Filter with 6 Filter Items in total.
        var filter = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(numberOfFilterItemsPerFilterGroup: 6),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            await context.AddRangeAsync(filter);
            await context.SaveChangesAsync();
        }

        var tempTableCreator = new Mock<ITemporaryTableCreator>(Strict);

        var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

        sqlHelper
            .Setup(s => s.CreateRandomIndexName("#MatchedObservation", "Id"))
            .Returns("IX_#MatchedObservation_Id_1234");

        var queryGenerator = new MatchingObservationsQueryGenerator(
            tempTableCreator: tempTableCreator.Object,
            sqlHelper: sqlHelper.Object
        );

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // We are selecting more than half of the Filter Items out of the
            // available 6 for the Filter, meaning that it is cheaper to exclude
            // Observations by checking for the *existence* of the *unselected*
            // Filter Items against Observation rows.
            var selectedFilterItems = filter.FilterGroups[0].FilterItems.Take(numberOfSelectedItems).ToList();

            var selectedFilterItemIds = selectedFilterItems.Select(fi => fi.Id).ToList();

            var unselectedFilterItemIds = filter
                .FilterGroups[0]
                .FilterItems.Except(selectedFilterItems)
                .Select(fi => fi.Id)
                .ToList();

            var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
            matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(context, cancellationToken))
                .ReturnsAsync(matchingObservationsTable.Object);

            var unselectedFilterItemTempTableEntriesForFilter = unselectedFilterItemIds
                .Select(id => new IdTempTable(id))
                .ToList();

            var filterItemIdTempTableReference = GenerateSetupsForFilterItemTempTable(
                filterNumber: 1,
                // Note here we are populating THIS Filter's temp table with the
                // opposite of the user's Filter Item selection, as that
                // provides the smaller set to test against.
                unselectedFilterItemTempTableEntriesForFilter,
                tempTableCreator,
                context,
                cancellationToken
            );

            var (sql, sqlParameters, _) = await queryGenerator.GetMatchingObservationsQuery(
                context,
                subjectId,
                selectedFilterItemIds,
                [],
                null,
                cancellationToken
            );

            VerifyAllMocks(tempTableCreator, sqlHelper, filterItemIdTempTableReference);

            //
            // Ensure that the DELETE statement uses an EXISTS check against
            // its temp table, that includes the Filter Item Ids *not* selected by
            // the user. This allows the DELETE statement to exclude any Observations
            // that have any of the Filter Items *not* selected for that particular
            // Filter in the most efficient manner, by only evaluating the smaller
            // number of Filter Item Ids.
            //
            const string expectedSql =
                @"
                    INSERT INTO #MatchedObservation WITH (TABLOCK)
                    SELECT o.id FROM Observation o
                    WHERE o.SubjectId = @subjectId
                    OPTION(RECOMPILE, MAXDOP 4);
                
                    CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedObservation_Id_1234]
                    ON #MatchedObservation(Id) WITH (MAXDOP = 4);

                    DELETE CandidateObservation WITH (TABLOCK)
                    FROM #MatchedObservation CandidateObservation
                    WHERE EXISTS (
                        SELECT 1
                        FROM ObservationFilterItem OFI
                        JOIN #Filter1TempTable SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                        WHERE OFI.ObservationId = CandidateObservation.Id
                    )
                    OPTION(RECOMPILE, MAXDOP 4);

                    UPDATE STATISTICS #MatchedObservation WITH FULLSCAN;
                    ";

            Assert.Equal(NormaliseSqlFormatting(expectedSql), NormaliseSqlFormatting(sql));

            sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
        }
    }

    [Fact]
    public async Task QueryGenerator_FilterItems_NoItemsChosenForFilter_FilterStatementExcluded()
    {
        var subjectId = Guid.NewGuid();

        // Create a Filter with 10 Filter Items in total.
        var filter1 = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(5, 5),
        };

        // Create a Filter with 10 Filter Items in total.
        var filter2 = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(10),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            await context.AddRangeAsync(filter1, filter2);
            await context.SaveChangesAsync();
        }

        var tempTableCreator = new Mock<ITemporaryTableCreator>(Strict);

        var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

        sqlHelper
            .Setup(s => s.CreateRandomIndexName("#MatchedObservation", "Id"))
            .Returns("IX_#MatchedObservation_Id_1234");

        var queryGenerator = new MatchingObservationsQueryGenerator(
            tempTableCreator: tempTableCreator.Object,
            sqlHelper: sqlHelper.Object
        );

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Set up some selected Filter Item Ids for only 1 Filter from this Subject.
            // The Filter with no selections should not have any DELETE statement as
            // it does not affect the outcome - it's treated in the same way as if the
            // user had selected ALL Filter Items for that Filter.
            //

            // We are selecting Filter Items for Filter 1.
            var selectedFilterItemIds = ListOf(
                filter1.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                filter1.FilterGroups.ToList()[0].FilterItems.ToList()[2].Id,
                filter1.FilterGroups.ToList()[1].FilterItems.ToList()[1].Id
            );

            var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
            matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(context, cancellationToken))
                .ReturnsAsync(matchingObservationsTable.Object);

            var selectedFilterItemTempTableEntriesForFilter1 = selectedFilterItemIds
                .Select(id => new IdTempTable(id))
                .ToList();

            // Note that we're only expecting a temp table to be created for Filter 1.  Filter 2 will not get one
            // as it has none of its Filter Items selected.
            var filterItemIdTempTableReference = GenerateSetupsForFilterItemTempTable(
                filterNumber: 1,
                selectedFilterItemTempTableEntriesForFilter1,
                tempTableCreator,
                context,
                cancellationToken
            );

            var (sql, sqlParameters, _) = await queryGenerator.GetMatchingObservationsQuery(
                context,
                subjectId,
                selectedFilterItemIds,
                [],
                null,
                cancellationToken
            );

            VerifyAllMocks(tempTableCreator, sqlHelper, filterItemIdTempTableReference);

            // Ensure that the DELETE statements appear in the expected order in the generated query,
            // with the most selective Filter (smallest ratio of selected Filter Items) first.
            // This allows us to hopefully eliminate many more #MatchedObservation rows on the first pass,
            // meaning that less #MatchedObservation rows need to be scanned for the 2nd Filter, and
            // then less for the 3rd, etc.
            //
            const string expectedSql =
                @"
                    INSERT INTO #MatchedObservation WITH (TABLOCK)
                    SELECT o.id FROM Observation o
                    WHERE o.SubjectId = @subjectId
                    OPTION(RECOMPILE, MAXDOP 4);
                
                    CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedObservation_Id_1234]
                    ON #MatchedObservation(Id) WITH (MAXDOP = 4);

                    DELETE CandidateObservation WITH (TABLOCK)
                    FROM #MatchedObservation CandidateObservation
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM ObservationFilterItem OFI
                        JOIN #Filter1TempTable SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                        WHERE OFI.ObservationId = CandidateObservation.Id
                    )
                    OPTION(RECOMPILE, MAXDOP 4);

                    UPDATE STATISTICS #MatchedObservation WITH FULLSCAN;
                    ";

            Assert.Equal(NormaliseSqlFormatting(expectedSql), NormaliseSqlFormatting(sql));

            sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
        }
    }

    [Fact]
    public async Task QueryGenerator_FilterItems_AllItemsChosenForFilter_FilterStatementExcluded()
    {
        var subjectId = Guid.NewGuid();

        // Create a Filter with 10 Filter Items in total.
        var filter1 = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(10),
        };

        // Create a Filter with 10 Filter Items in total.
        var filter2 = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(5, 5),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            await context.AddRangeAsync(filter1, filter2);
            await context.SaveChangesAsync();
        }

        var tempTableCreator = new Mock<ITemporaryTableCreator>(Strict);

        var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

        sqlHelper
            .Setup(s => s.CreateRandomIndexName("#MatchedObservation", "Id"))
            .Returns("IX_#MatchedObservation_Id_1234");

        var queryGenerator = new MatchingObservationsQueryGenerator(
            tempTableCreator: tempTableCreator.Object,
            sqlHelper: sqlHelper.Object
        );

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Set up ALL selected Filter Item Ids for Filter1, and select just some
            // Filter Item Ids for Filter 2.
            //
            // The Filter with all Items selected should not have any DELETE statement as
            // it does not affect the outcome - it cannot exclude any Observation rows if
            // all its Items have been chosen, as it's guaranteed that any Observation row
            // will have one of the selected Items.
            //

            // We are selecting ALL Filter Items for Filter 1.
            var selectedFilter1ItemIds = filter1
                .FilterGroups.SelectMany(fg => fg.FilterItems)
                .Select(fi => fi.Id)
                .ToList();

            // We are selecting some Filter Items for Filter 2.
            var selectedFilter2ItemIds = ListOf(
                filter2.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                filter2.FilterGroups.ToList()[0].FilterItems.ToList()[2].Id,
                filter2.FilterGroups.ToList()[1].FilterItems.ToList()[1].Id
            );

            var selectedFilterItemIds = selectedFilter1ItemIds.Concat(selectedFilter2ItemIds).ToList();

            var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
            matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(context, cancellationToken))
                .ReturnsAsync(matchingObservationsTable.Object);

            var selectedFilterItemTempTableEntriesForFilter2 = selectedFilter2ItemIds
                .Select(id => new IdTempTable(id))
                .ToList();

            // Note that we're only expecting a temp table to be created for Filter 2.  Filter 1 will not get one
            // as it has all of its Filter Items selected, and therefore cannot be used to exclude anything.
            var filterItemIdTempTableReference = GenerateSetupsForFilterItemTempTable(
                filterNumber: 1,
                selectedFilterItemTempTableEntriesForFilter2,
                tempTableCreator,
                context,
                cancellationToken
            );

            var (sql, sqlParameters, _) = await queryGenerator.GetMatchingObservationsQuery(
                context,
                subjectId,
                selectedFilterItemIds,
                [],
                null,
                cancellationToken
            );

            VerifyAllMocks(tempTableCreator, sqlHelper, filterItemIdTempTableReference);

            // Ensure that a DELETE statement only appears for Filter 2.
            const string expectedSql =
                @"
                    INSERT INTO #MatchedObservation WITH (TABLOCK)
                    SELECT o.id FROM Observation o
                    WHERE o.SubjectId = @subjectId
                    OPTION(RECOMPILE, MAXDOP 4);
                
                    CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedObservation_Id_1234]
                    ON #MatchedObservation(Id) WITH (MAXDOP = 4);

                    DELETE CandidateObservation WITH (TABLOCK)
                    FROM #MatchedObservation CandidateObservation
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM ObservationFilterItem OFI
                        JOIN #Filter1TempTable SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                        WHERE OFI.ObservationId = CandidateObservation.Id
                    )
                    OPTION(RECOMPILE, MAXDOP 4);

                    UPDATE STATISTICS #MatchedObservation WITH FULLSCAN;
                    ";

            Assert.Equal(NormaliseSqlFormatting(expectedSql), NormaliseSqlFormatting(sql));

            sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
        }
    }

    [Fact]
    public async Task QueryGenerator_MultipleFiltersAndFilterItems()
    {
        var subjectId = Guid.NewGuid();

        // Create a Filter with 10 Filter Items in total.
        var filter1 = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(5, 5),
        };

        // Create a Filter with 10 Filter Items in total.
        var filter2 = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(10),
        };

        // Create a Filter with 100 Filter Items in total.
        var filter3 = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(20, 20, 20, 20, 20),
        };

        // Create a Filter with 2 Filter Items in total.
        var filter4 = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(2),
        };

        var unselectedFilter5 = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            FilterGroups = CreateFilterGroups(2),
        };

        var unrelatedFilter = new Filter
        {
            Id = Guid.NewGuid(),
            SubjectId = Guid.NewGuid(),
            FilterGroups = CreateFilterGroups(5),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            await context.AddRangeAsync(filter1, filter2, filter3, filter4, unselectedFilter5, unrelatedFilter);
            await context.SaveChangesAsync();
        }

        var tempTableCreator = new Mock<ITemporaryTableCreator>(Strict);

        var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

        sqlHelper
            .Setup(s => s.CreateRandomIndexName("#MatchedObservation", "Id"))
            .Returns("IX_#MatchedObservation_Id_1234");

        var queryGenerator = new MatchingObservationsQueryGenerator(
            tempTableCreator: tempTableCreator.Object,
            sqlHelper: sqlHelper.Object
        );

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Set up some selected Filter Item Ids from 3 of the 4 Filters on this Subject.
            //
            // We are selecting 3 Filter Items out of 10 for Filter 1, making it
            // a 30% ratio.
            //
            // We are selecting 7 filter items out of 10 for Filter 2, making it
            // a 70% ratio.
            //
            // We are selecting 12 filter items out of 100 for Filter 3,
            // making it a 12% ratio.
            //
            // We are selecting 1 filter item out of 2 for Filter 4,
            // making it a 50% ratio.
            //
            // Therefore, based on using the most selective Filter first, we expect the
            // order of Filter deletion statements to look like:
            //
            // * Filter 3 (12%)
            // * Filter 1 (30%)
            // * Filter 4 (50%)
            // * Filter 2 (70%)

            // We are selecting 3 Filter Items out of 10 for Filter 1, making it
            // a 30% ratio.
            var selectedFilter1FilterItemIds = ListOf(
                filter1.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                filter1.FilterGroups.ToList()[0].FilterItems.ToList()[2].Id,
                filter1.FilterGroups.ToList()[1].FilterItems.ToList()[1].Id
            );

            // We are selecting 7 filter items out of 10 for Filter 2, making it
            // a 70% ratio.  Because this is more than half the items selected,
            // we'll expect the opposite selection to be used during the exclusion
            // process in tandem with an "EXISTS" clause.
            var selectedFilter2FilterItemIds = ListOf(
                filter2.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                filter2.FilterGroups.ToList()[0].FilterItems.ToList()[2].Id,
                filter2.FilterGroups.ToList()[0].FilterItems.ToList()[4].Id,
                filter2.FilterGroups.ToList()[0].FilterItems.ToList()[3].Id,
                filter2.FilterGroups.ToList()[0].FilterItems.ToList()[7].Id,
                filter2.FilterGroups.ToList()[0].FilterItems.ToList()[6].Id,
                filter2.FilterGroups.ToList()[0].FilterItems.ToList()[8].Id
            );

            var unselectedFilter2FilterItemIds = filter2
                .FilterGroups.ToList()[0]
                .FilterItems.Select(fi => fi.Id)
                .Except(selectedFilter2FilterItemIds)
                .ToList();

            // We are selecting 12 filter items out of 100 for Filter 3,
            // making it a 12% ratio.
            var selectedFilter3FilterItemIds = ListOf(
                filter3.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                filter3.FilterGroups.ToList()[0].FilterItems.ToList()[1].Id,
                filter3.FilterGroups.ToList()[1].FilterItems.ToList()[0].Id,
                filter3.FilterGroups.ToList()[2].FilterItems.ToList()[1].Id,
                filter3.FilterGroups.ToList()[3].FilterItems.ToList()[0].Id,
                filter3.FilterGroups.ToList()[4].FilterItems.ToList()[1].Id,
                filter3.FilterGroups.ToList()[4].FilterItems.ToList()[2].Id,
                filter3.FilterGroups.ToList()[4].FilterItems.ToList()[3].Id,
                filter3.FilterGroups.ToList()[4].FilterItems.ToList()[4].Id,
                filter3.FilterGroups.ToList()[4].FilterItems.ToList()[5].Id,
                filter3.FilterGroups.ToList()[4].FilterItems.ToList()[6].Id,
                filter3.FilterGroups.ToList()[4].FilterItems.ToList()[7].Id
            );

            // We are selecting 1 filter item out of 2 for Filter 4,
            // making it a 50% ratio.
            var selectedFilter4FilterItemIds = ListOf(filter4.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id);

            // An accidental inclusion of a Filter Item Id that doesn't belong to a
            // Filter on this Subject will be ignored.
            var invalidUnrelatedFilterFilterItemIds = ListOf(
                unrelatedFilter.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id
            );

            var allSelectedFilterItemIds = selectedFilter1FilterItemIds
                .Concat(selectedFilter2FilterItemIds)
                .Concat(selectedFilter3FilterItemIds)
                .Concat(selectedFilter4FilterItemIds)
                .Concat(invalidUnrelatedFilterFilterItemIds)
                .ToList();

            var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
            matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(context, cancellationToken))
                .ReturnsAsync(matchingObservationsTable.Object);

            var selectedFilterItemsByFilter = ListOf(
                selectedFilter1FilterItemIds,
                // Expect the inverse selection for Filter 2.
                unselectedFilter2FilterItemIds,
                selectedFilter3FilterItemIds,
                selectedFilter4FilterItemIds
            );

            var selectedFilterItemTempTableEntriesByFilter = selectedFilterItemsByFilter
                .Select(filterItemIds => filterItemIds.Select(id => new IdTempTable(id)).ToList())
                .ToList();

            var filterItemIdTempTableReferences = selectedFilterItemTempTableEntriesByFilter
                .Select(
                    (tempTableEntries, i) =>
                        GenerateSetupsForFilterItemTempTable(
                            filterNumber: i + 1,
                            tempTableEntries,
                            tempTableCreator,
                            context,
                            cancellationToken
                        )
                )
                .ToList();

            var (sql, sqlParameters, _) = await queryGenerator.GetMatchingObservationsQuery(
                context,
                subjectId,
                allSelectedFilterItemIds,
                [],
                null,
                cancellationToken
            );

            VerifyAllMocks(tempTableCreator, sqlHelper);
            VerifyAllMocks(filterItemIdTempTableReferences.Cast<Mock>().ToArray());

            // Ensure that the DELETE statements appear in the expected order in the generated query,
            // with the most selective Filter (smallest ratio of selected Filter Items) first.
            // This allows us to hopefully eliminate many more #MatchedObservation rows on the first pass,
            // meaning that less #MatchedObservation rows need to be scanned for the 2nd Filter, and
            // then less for the 3rd, etc.
            //
            const string expectedSql =
                @"
                    INSERT INTO #MatchedObservation WITH (TABLOCK)
                    SELECT o.id FROM Observation o
                    WHERE o.SubjectId = @subjectId
                    OPTION(RECOMPILE, MAXDOP 4);
                
                    CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedObservation_Id_1234]
                    ON #MatchedObservation(Id) WITH (MAXDOP = 4);

                    DELETE CandidateObservation WITH (TABLOCK)
                    FROM #MatchedObservation CandidateObservation
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM ObservationFilterItem OFI
                        JOIN #Filter3TempTable SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                        WHERE OFI.ObservationId = CandidateObservation.Id
                    )
                    OPTION(RECOMPILE, MAXDOP 4);

                    DELETE CandidateObservation WITH (TABLOCK)
                    FROM #MatchedObservation CandidateObservation
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM ObservationFilterItem OFI
                        JOIN #Filter1TempTable SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                        WHERE OFI.ObservationId = CandidateObservation.Id
                    )
                    OPTION(RECOMPILE, MAXDOP 4);

                    DELETE CandidateObservation WITH (TABLOCK)
                    FROM #MatchedObservation CandidateObservation
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM ObservationFilterItem OFI
                        JOIN #Filter4TempTable SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                        WHERE OFI.ObservationId = CandidateObservation.Id
                    )
                    OPTION(RECOMPILE, MAXDOP 4);

                    DELETE CandidateObservation WITH (TABLOCK)
                    FROM #MatchedObservation CandidateObservation
                    WHERE EXISTS (
                        SELECT 1
                        FROM ObservationFilterItem OFI
                        JOIN #Filter2TempTable SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                        WHERE OFI.ObservationId = CandidateObservation.Id
                    )
                    OPTION(RECOMPILE, MAXDOP 4);

                    UPDATE STATISTICS #MatchedObservation WITH FULLSCAN;
                    ";

            Assert.Equal(NormaliseSqlFormatting(expectedSql), NormaliseSqlFormatting(sql));

            sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
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
            FilterGroups = CreateFilterGroups(5, 5),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            await context.AddAsync(filter);
            await context.SaveChangesAsync();
        }

        var tempTableCreator = new Mock<ITemporaryTableCreator>(Strict);

        var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

        sqlHelper
            .Setup(s => s.CreateRandomIndexName("#MatchedObservation", "Id"))
            .Returns("IX_#MatchedObservation_Id_1234");

        var queryGenerator = new MatchingObservationsQueryGenerator(
            tempTableCreator: tempTableCreator.Object,
            sqlHelper: sqlHelper.Object
        );

        await using (var context = InMemoryStatisticsDbContext(contextId))
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var selectedFilterItemIds = ListOf(
                filter.FilterGroups.ToList()[0].FilterItems.ToList()[0].Id,
                filter.FilterGroups.ToList()[0].FilterItems.ToList()[2].Id,
                filter.FilterGroups.ToList()[1].FilterItems.ToList()[1].Id
            );

            var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
            matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedObservation, StatisticsDbContext>(context, cancellationToken))
                .ReturnsAsync(matchingObservationsTable.Object);

            var selectedLocationIds = ListOf(Guid.NewGuid(), Guid.NewGuid());

            var selectedLocationIdTempTableEntries = selectedLocationIds.Select(id => new IdTempTable(id)).ToList();

            var locationIdsTempTableReference = new Mock<ITempTableQuery<IdTempTable>>();
            locationIdsTempTableReference.SetupGet(t => t.Name).Returns("#LocationTempTable");

            tempTableCreator
                .Setup(s =>
                    s.CreateAnonymousTemporaryTableAndPopulate(
                        context,
                        ItIs.EnumerableSequenceEqualTo(selectedLocationIdTempTableEntries),
                        cancellationToken
                    )
                )
                .ReturnsAsync(locationIdsTempTableReference.Object);

            var filterItemIdsTempTableReference = new Mock<ITempTableQuery<IdTempTable>>();
            filterItemIdsTempTableReference.SetupGet(t => t.Name).Returns("#FilterTempTable");

            tempTableCreator
                .Setup(s =>
                    s.CreateAnonymousTemporaryTableAndPopulate(
                        context,
                        ItIs.ListSequenceEqualTo(
                            selectedFilterItemIds.OrderBy(id => id).Select(id => new IdTempTable(id)).ToList()
                        ),
                        cancellationToken
                    )
                )
                .ReturnsAsync(filterItemIdsTempTableReference.Object);

            var tempTableMocks = ListOf(locationIdsTempTableReference);
            tempTableMocks.Add(filterItemIdsTempTableReference);

            var (sql, sqlParameters, _) = await queryGenerator.GetMatchingObservationsQuery(
                context,
                subjectId,
                selectedFilterItemIds,
                selectedLocationIds,
                new TimePeriodQuery
                {
                    StartCode = TimeIdentifier.AcademicYear,
                    StartYear = 2015,
                    EndCode = TimeIdentifier.AcademicYear,
                    EndYear = 2017,
                },
                cancellationToken
            );

            VerifyAllMocks(tempTableCreator, sqlHelper);
            VerifyAllMocks(tempTableMocks.Cast<Mock>().ToArray());

            const string expectedSql =
                @"
                    INSERT INTO #MatchedObservation WITH (TABLOCK)
                    SELECT o.id FROM Observation o
                    WHERE o.SubjectId = @subjectId
                    AND (
                      (o.TimeIdentifier = 'AY' AND o.Year = 2015) OR
                      (o.TimeIdentifier = 'AY' AND o.Year = 2016) OR
                      (o.TimeIdentifier = 'AY' AND o.Year = 2017)
                    )
                    AND (o.LocationId IN (SELECT Id FROM #LocationTempTable))
                    OPTION(RECOMPILE, MAXDOP 4);
                
                    CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedObservation_Id_1234]
                    ON #MatchedObservation(Id) WITH (MAXDOP = 4);

                    DELETE CandidateObservation WITH (TABLOCK)
                    FROM #MatchedObservation CandidateObservation
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM ObservationFilterItem OFI
                        JOIN #FilterTempTable SelectedFilterItem ON SelectedFilterItem.Id = OFI.FilterItemId
                        WHERE OFI.ObservationId = CandidateObservation.Id
                    )
                    OPTION(RECOMPILE, MAXDOP 4);

                    UPDATE STATISTICS #MatchedObservation WITH FULLSCAN;
                    ";

            Assert.Equal(NormaliseSqlFormatting(expectedSql), NormaliseSqlFormatting(sql));

            sqlParameters.AssertDeepEqualTo(ListOf(new SqlParameter("subjectId", subjectId)));
        }
    }

    private static ObservationService BuildService(
        StatisticsDbContext context,
        IMatchingObservationsQueryGenerator? queryGenerator = null,
        IRawSqlExecutor? sqlExecutor = null
    )
    {
        return new ObservationService(
            context: context,
            queryGenerator: queryGenerator ?? Mock.Of<IMatchingObservationsQueryGenerator>(Strict),
            sqlExecutor: sqlExecutor ?? Mock.Of<IRawSqlExecutor>(Strict),
            logger: Mock.Of<ILogger<ObservationService>>()
        );
    }

    private static List<FilterGroup> CreateFilterGroups(params int[] numberOfFilterItemsPerFilterGroup)
    {
        return numberOfFilterItemsPerFilterGroup
            .Select(filterItemCount => new FilterGroup
            {
                Id = Guid.NewGuid(),
                FilterItems = Enumerable
                    .Range(0, filterItemCount)
                    .Select(_ => new FilterItem { Id = Guid.NewGuid() })
                    .ToList(),
            })
            .ToList();
    }

    private static Mock<ITempTableQuery<IdTempTable>> GenerateSetupsForFilterItemTempTable(
        int filterNumber,
        List<IdTempTable> tempTableEntries,
        Mock<ITemporaryTableCreator> tempTableCreator,
        StatisticsDbContext? context,
        CancellationToken cancellationToken
    )
    {
        var orderedIds = tempTableEntries.OrderBy(t => t.Id).ToList();

        var tempTableReference = new Mock<ITempTableQuery<IdTempTable>>();
        tempTableReference.SetupGet(t => t.Name).Returns($"#Filter{filterNumber}TempTable");

        tempTableCreator
            .Setup(s =>
                s.CreateAnonymousTemporaryTableAndPopulate(
                    context,
                    ItIs.ListSequenceEqualTo(orderedIds),
                    cancellationToken
                )
            )
            .ReturnsAsync(tempTableReference.Object);

        return tempTableReference;
    }
}

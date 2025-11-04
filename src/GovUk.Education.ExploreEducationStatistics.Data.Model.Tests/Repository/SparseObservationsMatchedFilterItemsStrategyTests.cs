using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;
using Thinktecture.EntityFrameworkCore.TempTables;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.SqlTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository;

public abstract class SparseObservationsMatchedFilterItemsStrategyTests
{
    public class GetFilterItemsFromMatchedObservationIdsTests : SparseObservationsMatchedFilterItemsStrategyTests
    {
        private static readonly DataFixture Fixture = new();

        [Fact]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task TempTablesCreatedAndQueriesExecuted()
        {
            var subjectId = Guid.NewGuid();

            await using var context = InMemoryStatisticsDbContext();

            var tempTableCreator = new Mock<ITemporaryTableCreator>(Strict);
            var sqlExecutor = new Mock<IRawSqlExecutor>(Strict);
            var sqlHelper = new Mock<ISqlStatementsHelper>(Strict);

            var matchingObservationsTable = new Mock<ITempTableReference>(Strict);
            matchingObservationsTable.SetupGet(t => t.Name).Returns("#MatchedObservation");

            var matchedFilterItemsTable = new Mock<ITempTableReference>(Strict);
            matchedFilterItemsTable.SetupGet(t => t.Name).Returns("#MatchedFilterItem");

            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedFilterItem, StatisticsDbContext>(context, default))
                .ReturnsAsync(matchedFilterItemsTable.Object);

            var expectedQuerySql =
                $@"
                INSERT INTO #MatchedFilterItem WITH (TABLOCK)
                SELECT DISTINCT o.FilterItemId
                FROM #MatchedObservation AS mo
                JOIN ObservationFilterItem AS o
                  ON o.ObservationId = mo.Id
                OPTION(RECOMPILE, MAXDOP 4);";

            sqlExecutor
                .Setup(s =>
                    s.ExecuteSqlRaw(
                        context,
                        It.Is<string>(sql => NormaliseSqlFormatting(sql) == NormaliseSqlFormatting(expectedQuerySql)),
                        default
                    )
                )
                .Returns(Task.CompletedTask);

            sqlHelper
                .Setup(s => s.CreateRandomIndexName("#MatchedFilterItem", "Id"))
                .Returns("IX_#MatchedFilterItem_Id_1234");

            var expectedIndexSql =
                @"
                    CREATE UNIQUE CLUSTERED INDEX [IX_#MatchedFilterItem_Id_1234]
                    ON #MatchedFilterItem(Id) WITH (MAXDOP = 4);
                    
                    UPDATE STATISTICS #MatchedFilterItem WITH FULLSCAN;
                    ";

            sqlExecutor
                .Setup(s =>
                    s.ExecuteSqlRaw(
                        context,
                        It.Is<string>(sql => NormaliseSqlFormatting(expectedIndexSql) == NormaliseSqlFormatting(sql)),
                        default
                    )
                )
                .Returns(Task.CompletedTask);

            var strategy = new SparseObservationsMatchedFilterItemsStrategy(
                sqlExecutor: sqlExecutor.Object,
                temporaryTableCreator: tempTableCreator.Object,
                sqlHelper: sqlHelper.Object,
                context: context,
                logger: Mock.Of<ILogger<SparseObservationsMatchedFilterItemsStrategy>>()
            );

            await strategy.GetFilterItemsFromMatchedObservationIds(
                subjectId: subjectId,
                matchedObservationsTableReference: matchingObservationsTable.Object,
                cancellationToken: default
            );

            VerifyAllMocks(tempTableCreator, sqlHelper, sqlExecutor);
        }

        [Fact]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task MatchedFilterItemTempTablePopulated_FilterItemsReturned()
        {
            // Create 4 filters for this Subject.
            var filters = Fixture
                .DefaultFilter(filterGroupCount: 2, filterItemCount: 2)
                .WithSubject(Fixture.DefaultSubject())
                .GenerateList(4);

            // Create some filters for another Subject too.
            var otherSubjectFilters = Fixture
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

            context.Filter.AddRange(filters.Concat(otherSubjectFilters));
            await context.SaveChangesAsync();

            FilterItem[] expectedMatchedFilterItems =
            [
                matchedFilter1Group2Item2,
                matchedFilter2Group1Item1,
                matchedFilter2Group1Item2,
                matchedFilter3Group1Item1,
                matchedFilter3Group2Item1,
            ];

            // Fake populating the temporary table with results from executing the generated
            // query from the MatchedFilterItemsQueryGenerator.
            context.MatchedFilterItems.AddRange(expectedMatchedFilterItems.Select(fi => new MatchedFilterItem(fi.Id)));
            await context.SaveChangesAsync();

            var matchingObservationsTable = new Mock<ITempTableReference>(Loose);
            var tempTableCreator = new Mock<ITemporaryTableCreator>(Strict);
            var sqlExecutor = new Mock<IRawSqlExecutor>(Loose);

            var matchedFilterItemsTable = new Mock<ITempTableReference>(Strict);
            matchedFilterItemsTable.SetupGet(t => t.Name).Returns("#MatchedFilterItem");

            tempTableCreator
                .Setup(s => s.CreateTemporaryTable<MatchedFilterItem, StatisticsDbContext>(context, default))
                .ReturnsAsync(matchedFilterItemsTable.Object);

            var strategy = new SparseObservationsMatchedFilterItemsStrategy(
                sqlExecutor: sqlExecutor.Object,
                temporaryTableCreator: tempTableCreator.Object,
                sqlHelper: Mock.Of<ISqlStatementsHelper>(),
                context: context,
                logger: Mock.Of<ILogger<SparseObservationsMatchedFilterItemsStrategy>>()
            );

            var matchedFilterItems = await strategy.GetFilterItemsFromMatchedObservationIds(
                subjectId: subjectId,
                matchedObservationsTableReference: matchingObservationsTable.Object,
                cancellationToken: default
            );

            VerifyAllMocks(tempTableCreator);

            Assert.Equal(expectedMatchedFilterItems.OrderBy(f => f.Id), matchedFilterItems.OrderBy(f => f.Id));
        }
    }
}

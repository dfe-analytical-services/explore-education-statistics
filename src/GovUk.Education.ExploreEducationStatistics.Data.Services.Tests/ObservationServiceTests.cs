#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using Thinktecture.EntityFrameworkCore.TempTables;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
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
}

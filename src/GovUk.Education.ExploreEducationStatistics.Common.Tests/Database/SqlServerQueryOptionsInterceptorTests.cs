#nullable enable
using System.Data;
using System.Data.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Database;

public abstract class SqlServerQueryOptionsInterceptorTests
{
    public class SqlServerQueryOptionsInterceptorSqlProcessorDependencyTests : SqlServerQueryOptionsInterceptorTests
    {
        [Fact]
        public void GivenSqlProcessor_WhenReaderExecutingCalled_ThenSqlProcessorIsCalled()
        {
            AssertSqlProcessorCalled(
                (interceptor, dbCommand) =>
                    interceptor.ReaderExecuting(command: dbCommand, eventData: null!, result: default)
            );
        }

        [Fact]
        public async Task GivenSqlProcessor_WhenReaderExecutingAsyncCalled_ThenSqlProcessorIsCalled()
        {
            await AssertSqlProcessorCalledAsync(
                (interceptor, dbCommand) =>
                    interceptor.ReaderExecutingAsync(command: dbCommand, eventData: null!, result: default)
            );
        }

        [Fact]
        public void GivenSqlProcessor_WhenScalarExecutingCalled_ThenSqlProcessorIsCalled()
        {
            AssertSqlProcessorCalled(
                (interceptor, dbCommand) =>
                    interceptor.ScalarExecuting(command: dbCommand, eventData: null!, result: default)
            );
        }

        [Fact]
        public async Task GivenSqlProcessor_WhenScalarExecutingAsyncCalled_ThenSqlProcessorIsCalled()
        {
            await AssertSqlProcessorCalledAsync(
                (interceptor, dbCommand) =>
                    interceptor.ScalarExecutingAsync(command: dbCommand, eventData: null!, result: default)
            );
        }

        [Fact]
        public void GivenSqlProcessor_WhenNonQueryExecutingCalled_ThenSqlProcessorIsCalled()
        {
            AssertSqlProcessorCalled(
                (interceptor, dbCommand) =>
                    interceptor.NonQueryExecuting(command: dbCommand, eventData: null!, result: default)
            );
        }

        [Fact]
        public async Task GivenSqlProcessor_WhenNonQueryExecutingAsyncCalled_ThenSqlProcessorIsCalled()
        {
            await AssertSqlProcessorCalledAsync(
                (interceptor, dbCommand) =>
                    interceptor.NonQueryExecutingAsync(command: dbCommand, eventData: null!, result: default)
            );
        }

        private static async Task AssertSqlProcessorCalledAsync<T>(
            Func<SqlServerQueryOptionsInterceptor, DbCommand, ValueTask<InterceptionResult<T>>> sqlInvokingMethod
        )
        {
            var sqlProcessor = new Mock<IQueryOptionsInterceptorSqlProcessor>(MockBehavior.Strict);

            sqlProcessor.Setup(s => s.Process("sql string")).Returns("processed sql string");

            var sut = new SqlServerQueryOptionsInterceptor(sqlProcessor.Object);

            var dbCommand = new FakeCommand { CommandText = "sql string" };

            await sqlInvokingMethod(sut, dbCommand);

            Assert.Equal("processed sql string", dbCommand.CommandText);
        }

        private static void AssertSqlProcessorCalled(
            Action<SqlServerQueryOptionsInterceptor, DbCommand> sqlInvokingMethod
        )
        {
            AssertSqlProcessorCalledAsync(
                    (interceptor, dbCommand) =>
                    {
                        sqlInvokingMethod(interceptor, dbCommand);
                        return ValueTask.FromResult(new InterceptionResult<int>());
                    }
                )
                .Wait();
        }
    }

    public class SqlServerQueryOptionsInterceptorSqlProcessorTests : SqlServerQueryOptionsInterceptorTests
    {
        [Fact]
        public void GivenExistingSqlHasNoOptionClause_WhenWithOptionsPresent_ThenOptionsAddedToQuery()
        {
            const string originalSql = """
                -- Any other EF tag
                -- WithOptions: OPTION(RECOMPILE, OPTIMIZE FOR UNKNOWN)
                SELECT * FROM [DataSetVersion];
                """;

            const string expectedSql = """
                -- Any other EF tag
                -- WithOptions: OPTION(RECOMPILE, OPTIMIZE FOR UNKNOWN)
                SELECT * FROM [DataSetVersion] OPTION(RECOMPILE, OPTIMIZE FOR UNKNOWN);
                """;

            var processedSql = ProcessSql(originalSql);

            Assert.Equal(expectedSql, processedSql);
        }

        [Fact]
        public void GivenExistingSqlHasOptionClause_WhenWithOptionsPresent_ThenOptionsAreMergedInQuery()
        {
            const string sql = """
                -- WithOptions: OPTION(OPTIMIZE FOR UNKNOWN, RECOMPILE)
                SELECT * FROM [DataSetVersion]
                OPTION(RECOMPILE);
                """;

            const string expectedSql = """
                -- WithOptions: OPTION(OPTIMIZE FOR UNKNOWN, RECOMPILE)
                SELECT * FROM [DataSetVersion]
                OPTION(RECOMPILE, OPTIMIZE FOR UNKNOWN);
                """;

            var processedSql = ProcessSql(sql);

            Assert.Equal(expectedSql, processedSql);
        }

        [Fact]
        public void GivenNoLeadingCommentBlockExists_WhenProcessed_ThenQueryUnchanged()
        {
            const string originalSql = "SELECT 1;";
            var processedSql = ProcessSql(originalSql);
            Assert.Equal(originalSql, processedSql);
        }

        [Fact]
        public void GivenExistingSqlHasNoOptionClause_WhenMultipleWithOptionsCommentsPresent_ThenOptionsAreMergedAndDeduplicated()
        {
            const string originalSql = """
                -- tenant: 42
                -- WithOptions: OPTION(USE HINT('DISABLE_OPTIMIZED_PLAN_FORCING'))
                -- note: something else
                -- WithOptions: OPTION(QUERYTRACEON 8649, RECOMPILE)
                SELECT TOP(10) * FROM [DataSetVersion];
                """;

            const string expectedSql = """
                -- tenant: 42
                -- WithOptions: OPTION(USE HINT('DISABLE_OPTIMIZED_PLAN_FORCING'))
                -- note: something else
                -- WithOptions: OPTION(QUERYTRACEON 8649, RECOMPILE)
                SELECT TOP(10) * FROM [DataSetVersion] OPTION(USE HINT('DISABLE_OPTIMIZED_PLAN_FORCING'), QUERYTRACEON 8649, RECOMPILE);
                """;

            var processedSql = ProcessSql(originalSql);

            Assert.Equal(expectedSql, processedSql);
        }

        [Fact]
        public void GivenExistingSqlHasOptionClause_WhenMultipleWithOptionsCommentsPresent_ThenOptionsAreMergedAndDeduplicated()
        {
            const string originalSql = """
                -- Some comments.
                -- WithOptions: OPTION(USE HINT('DISABLE_OPTIMIZED_PLAN_FORCING'))
                -- Some other comments.
                -- WithOptions: OPTION(QUERYTRACEON 8649, RECOMPILE)
                SELECT TOP(10) * FROM [DataSetVersion]
                OPTION(RECOMPILE, HASH JOIN);
                """;

            const string expectedSql = """
                -- Some comments.
                -- WithOptions: OPTION(USE HINT('DISABLE_OPTIMIZED_PLAN_FORCING'))
                -- Some other comments.
                -- WithOptions: OPTION(QUERYTRACEON 8649, RECOMPILE)
                SELECT TOP(10) * FROM [DataSetVersion]
                OPTION(RECOMPILE, HASH JOIN, USE HINT('DISABLE_OPTIMIZED_PLAN_FORCING'), QUERYTRACEON 8649);
                """;

            var processedSql = ProcessSql(originalSql);

            Assert.Equal(expectedSql, processedSql);
        }

        private static string ProcessSql(string sql)
        {
            return new SqlServerQueryOptionsInterceptorSqlProcessor().Process(sql);
        }
    }
}

/// <summary>
/// Fake DbCommand class for use in these tests, to allow the CommandText within instances of
/// this class to be updated during test execution and allowing the tests to ensure that the
/// DbCommand emitted from the interceptor methods has the expected CommandText set.
/// </summary>
internal sealed class FakeCommand : DbCommand
{
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override string CommandText { get; set; } = string.Empty;
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; } = CommandType.Text;
    protected override DbConnection? DbConnection { get; set; } = null!;
    protected override DbParameterCollection DbParameterCollection => null!;
    protected override DbTransaction? DbTransaction { get; set; } = null!;
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }

    public override void Cancel() => throw new NotImplementedException();

    public override int ExecuteNonQuery() => throw new NotImplementedException();

    public override object ExecuteScalar() => throw new NotImplementedException();

    public override void Prepare() => throw new NotImplementedException();

    protected override DbParameter CreateDbParameter() => throw new NotImplementedException();

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) =>
        throw new NotImplementedException();

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => Task.FromResult(0);

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken) =>
        Task.FromResult<object?>(0);

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(
        CommandBehavior behavior,
        CancellationToken cancellationToken
    ) => throw new NotImplementedException();
}

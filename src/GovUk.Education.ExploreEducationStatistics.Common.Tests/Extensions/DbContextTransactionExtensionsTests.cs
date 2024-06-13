#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.DbContextTransactionExtensionsTests;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

/// <summary>
///
/// This test suite covers convenience extension methods for removing the verbosity of
/// creating shared transactions between different DbContexts, and the behaviours around
/// how they interact and some of the issues to look out for when using.
///
/// From the tests covered below, we establish that:
///
/// * multiple DbContexts are able to coordinate under the same transaction boundary and
///   all roll back if a failure occurs.
/// * Transactions can be nested within each other and the parent has control of completing
///   or failing the transaction. If the child transaction fails, the parent needs to
///   acknowledge that failure in order to fail itself (e.g. rethrow an exception, return a
///   failing Either etc). 
/// * only a single DbContext that supports RetryOnFailure need be the one to create an
///   ExecutionContext, and thereafter all DbContexts supporting RetryOnFailure will
///   operate successfully.
/// * If a DbContext that doesn't support RetryOnFailure is used as the one that creates
///   the ExecutionStrategy and subsequently a DbContext that *does* support RetryOnFailure
///   is used, it will throw an InvalidOperationException, showing therefore that it is
///   best to use a RetryOnFailure-supporting DbContext if possible to originally create the
///   transaction.
/// 
/// </summary>
public abstract class DbContextTransactionExtensionsTests 
    : IClassFixture<DbContextTransactionExtensionsTestFixture>,
        IDisposable
{
    protected readonly WebApplicationFactory<TestStartup> TestApp;

    /// <summary>
    ///
    /// This test suite covers convenience extension methods for removing the verbosity of
    /// creating shared transactions between different DbContexts, and the behaviours around
    /// how they interact and some of the issues to look out for when using.
    ///
    /// From the tests covered below, we establish that:
    ///
    /// * multiple DbContexts are able to coordinate under the same transaction boundary and
    ///   all roll back if a failure occurs.
    /// * Transactions can be nested within each other and the parent has control of completing
    ///   or failing the transaction. If the child transaction fails, the parent needs to
    ///   acknowledge that failure in order to fail itself (e.g. rethrow an exception, return a
    ///   failing Either etc). 
    /// * only a single DbContext that supports RetryOnFailure need be the one to create an
    ///   ExecutionContext, and thereafter all DbContexts supporting RetryOnFailure will
    ///   operate successfully.
    /// * If a DbContext that doesn't support RetryOnFailure is used as the one that creates
    ///   the ExecutionStrategy and subsequently a DbContext that *does* support RetryOnFailure
    ///   is used, it will throw an InvalidOperationException, showing therefore that it is
    ///   best to use a RetryOnFailure-supporting DbContext if possible to originally create the
    ///   transaction.
    /// 
    /// </summary>
    protected DbContextTransactionExtensionsTests(DbContextTransactionExtensionsTestFixture fixture)
    {
        TestApp = new TestApplicationFactory<TestStartup>()
            .ConfigureServices(services => services
                .AddTransient<TestService>()
                .AddDbContext<TestDbContext1>(options =>
                    options.UseNpgsql(fixture.PostgreSqlContainers[0].GetConnectionString(),
                        psqlOptions => psqlOptions.EnableRetryOnFailure()))
                .AddDbContext<TestDbContext2>(options =>
                    options.UseNpgsql(fixture.PostgreSqlContainers[1].GetConnectionString(),
                        psqlOptions => psqlOptions.EnableRetryOnFailure()))
                .AddDbContext<TestDbContext3WithoutRetry>(options =>
                    options.UseNpgsql(fixture.PostgreSqlContainers[2].GetConnectionString())));
    }

    protected void ClearTestData<TDbContext>() where TDbContext : DbContext
    {
        var context = TestApp.GetDbContext<TDbContext, TestStartup>();
        context.ClearTestData();
    }

    public void Dispose()
    {
        ClearTestData<TestDbContext1>();
        ClearTestData<TestDbContext2>();
        ClearTestData<TestDbContext3WithoutRetry>();
    }

    public class NoTransactionTests(DbContextTransactionExtensionsTestFixture fixture)
        : DbContextTransactionExtensionsTests(fixture)
    {
        [Fact]
        public async Task SucceedWithoutTransaction()
        {
            await GetTestService().SucceedWithoutTransaction();
            AssertEntitiesInAllDbContexts();
        }
    }

    public class RequireTransactionTests(DbContextTransactionExtensionsTestFixture fixture)
        : DbContextTransactionExtensionsTests(fixture)
    {
        public class FlatTransactionTests(DbContextTransactionExtensionsTestFixture fixture)
            : RequireTransactionTests(fixture)
        {
            [Fact]
            public async Task Succeed()
            {
                await GetTestService().SucceedWithinFlatTransaction();
                AssertEntitiesInAllDbContexts();
            }

            [Fact]
            public async Task SucceedWithEither()
            {
                await GetTestService().SucceedWithinFlatTransaction_WithEither();
                AssertEntitiesInAllDbContexts();
            }

            [Fact]
            public async Task Fail()
            {
                await Assert.ThrowsAsync<SimulateFailureException>(GetTestService().FailWithinFlatTransaction);
                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailWithEither()
            {
                await GetTestService().FailWithinFlatTransaction_WithEither();
                AssertNoEntitiesInAnyDbContexts();
            }
        }

        public class NestedTransactionTests(DbContextTransactionExtensionsTestFixture fixture)
            : RequireTransactionTests(fixture)
        {
            [Fact]
            public async Task SucceedWithinNestedTransaction()
            {
                await GetTestService().SucceedWithinNestedTransaction();
                AssertEntitiesInAllDbContexts(1);
                AssertEntitiesInAllDbContexts(2);
            }

            [Fact]
            public async Task SucceedWithinNestedTransaction_MultipleContextsRequestTransaction()
            {
                await GetTestService().SucceedWithinNestedTransaction_MultipleContextsRequestTransaction();
                AssertEntitiesInAllDbContexts(1);
                AssertEntitiesInAllDbContexts(2);
                AssertEntitiesInAllDbContexts(3);
            }

            [Fact]
            public async Task TransactionInitiatedByNonRetryDbContext_ThrowsException()
            {
                await Assert.ThrowsAsync<InvalidOperationException>(GetTestService()
                    .TransactionInitiatedByNonRetryDbContext);
                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailWithinNestedTransaction()
            {
                await Assert.ThrowsAsync<SimulateFailureException>(GetTestService().FailWithinNestedTransaction);
                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailWithinNestedTransaction_WithEither()
            {
                await GetTestService().FailWithinNestedTransaction_WithEither();
                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailAtTopLevelWithNestedTransaction()
            {
                await Assert.ThrowsAsync<SimulateFailureException>(GetTestService()
                    .FailAtTopLevelWithNestedTransaction);
                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailAtTopLevelWithNestedTransaction_WithEither()
            {
                await GetTestService().FailAtTopLevelWithNestedTransaction_WithEither();
                AssertNoEntitiesInAnyDbContexts();
            }
        }
    }

    internal class TestService(
        TestDbContext1 dbContext1,
        TestDbContext2 dbContext2,
        TestDbContext3WithoutRetry dbContext3WithoutRetry)
    {
        public async Task SucceedWithoutTransaction() => await WriteToAllDbContexts();

        public async Task SucceedWithinFlatTransaction() =>
            await dbContext1.RequireTransaction(() => WriteToAllDbContexts());

        public async Task SucceedWithinFlatTransaction_WithEither() =>
            await dbContext1.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                return new Either<int, string>("success!");
            });

        public async Task FailWithinFlatTransaction() =>
            await dbContext1.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                throw new SimulateFailureException();
            });

        public async Task FailWithinFlatTransaction_WithEither() =>
            await dbContext1.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                return new Either<int, string>(1);
            });

        public async Task SucceedWithinNestedTransaction() =>
            await dbContext1.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                await dbContext1.RequireTransaction(() =>
                    WriteToAllDbContexts(2));
            });

        public async Task SucceedWithinNestedTransaction_MultipleContextsRequestTransaction() =>
            await dbContext1.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                await dbContext2.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts(2);
                    await dbContext3WithoutRetry.RequireTransaction(async () =>
                    {
                        await WriteToAllDbContexts(3);
                    });
                });
            });

        public async Task TransactionInitiatedByNonRetryDbContext() =>
            await dbContext3WithoutRetry.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                await dbContext2.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts(2);
                    await dbContext1.RequireTransaction(async () =>
                    {
                        await WriteToAllDbContexts(3);
                    });
                });
            });

        public async Task FailWithinNestedTransaction() =>
            await dbContext1.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                await dbContext1.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts(2);
                    throw new SimulateFailureException();
                });
            });

        public async Task FailWithinNestedTransaction_WithEither() =>
            await dbContext1.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                return await dbContext1.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts(2);
                    return new Either<int, string>(1);
                });
            });

        public async Task FailAtTopLevelWithNestedTransaction() =>
            await dbContext1.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                await dbContext1.RequireTransaction(() => WriteToAllDbContexts(2));
                throw new SimulateFailureException();
            });

        public async Task FailAtTopLevelWithNestedTransaction_WithEither() =>
            await dbContext1.RequireTransaction(async () =>
            {
                await WriteToAllDbContexts();
                await dbContext1.RequireTransaction(() => WriteToAllDbContexts(2));
                return new Either<int, string>(1);
            });

        private async Task WriteToAllDbContexts(int id = 1)
        {
            await dbContext1.Entities.AddAsync(new TestEntity {Id = id});
            await dbContext1.SaveChangesAsync();
            await dbContext2.Entities.AddAsync(new TestEntity {Id = id});
            await dbContext2.SaveChangesAsync();
            await dbContext3WithoutRetry.Entities.AddAsync(new TestEntity {Id = id});
            await dbContext3WithoutRetry.SaveChangesAsync();
        }
    }

    private void AssertEntitiesInAllDbContexts(int expectedId = 1)
    {
        var dbContext1 = TestApp.Services.GetRequiredService<TestDbContext1>();
        var dbContext2 = TestApp.Services.GetRequiredService<TestDbContext2>();
        var dbContext3 = TestApp.Services.GetRequiredService<TestDbContext3WithoutRetry>();
        Assert.NotNull(dbContext1.Entities.SingleOrDefaultAsync(entity => entity.Id == expectedId));
        Assert.NotNull(dbContext2.Entities.SingleOrDefaultAsync(entity => entity.Id == expectedId));
        Assert.NotNull(dbContext3.Entities.SingleOrDefaultAsync(entity => entity.Id == expectedId));
    }

    private void AssertNoEntitiesInAnyDbContexts()
    {
        var dbContext1 = TestApp.Services.GetRequiredService<TestDbContext1>();
        var dbContext2 = TestApp.Services.GetRequiredService<TestDbContext2>();
        var dbContext3 = TestApp.Services.GetRequiredService<TestDbContext3WithoutRetry>();
        Assert.Empty(dbContext1.Entities);
        Assert.Empty(dbContext2.Entities);
        Assert.Empty(dbContext3.Entities);
    }

    private TestService GetTestService() => TestApp.Services.GetRequiredService<TestService>();

    internal class TestEntity
    {
        public int Id { get; set; }
    }

    internal class TestDbContext1(DbContextOptions<TestDbContext1> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities { get; init; } = null!;
    }

    internal class TestDbContext2(DbContextOptions<TestDbContext2> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities { get; init; } = null!;
    }

    internal class TestDbContext3WithoutRetry(DbContextOptions<TestDbContext3WithoutRetry> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities { get; init; } = null!;
    }

    private class SimulateFailureException : Exception;
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DbContextTransactionExtensionsTestFixture :
        IAsyncLifetime
    {
        public readonly PostgreSqlContainer[] PostgreSqlContainers =
            Enumerable.Range(0, 3).Select(_ => new PostgreSqlBuilder()
                    .WithImage("postgres:16.1-alpine")
                    .Build())
                .ToArray();

        /// <summary>
        /// Add prepared transaction support to the PostgreSQL containers so that a shared transaction
        /// can be created for the various DbContexts in this test.
        ///
        /// Create an "Entities" table in each database for Entity Framework to read and write to.
        /// </summary>
        public async Task InitializeAsync() =>
            await Task.WhenAll(PostgreSqlContainers.SelectAsync(async container =>
            {
                await container.StartAsync();
                await container.ExecScriptAsync("ALTER SYSTEM SET max_prepared_transactions = 100");
                await container.StopAsync();
                await container.StartAsync();
                await container.ExecScriptAsync("""CREATE TABLE IF NOT EXISTS "Entities" ("Id" int PRIMARY KEY)""");
                return Task.CompletedTask;
            }));

        public async Task DisposeAsync() =>
            await Task.WhenAll(PostgreSqlContainers.Select(async container =>
                await container.DisposeAsync()));
    }
}

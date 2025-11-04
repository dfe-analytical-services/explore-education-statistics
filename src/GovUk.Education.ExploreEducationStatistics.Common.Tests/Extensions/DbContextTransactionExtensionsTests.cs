#nullable enable
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
/// </summary>
public abstract class DbContextTransactionExtensionsTests
    : IClassFixture<DbContextTransactionExtensionsTestFixture>,
        IAsyncLifetime
{
    private readonly WebApplicationFactory<TestStartup> _testApp;

    public DbContextTransactionExtensionsTests(DbContextTransactionExtensionsTestFixture fixture)
    {
        _testApp = new TestApplicationFactory<TestStartup>().ConfigureServices(services =>
            services
                .AddDbContext<TestDbContext1>(options =>
                    options.UseNpgsql(
                        fixture.PostgreSqlContainers[0].GetConnectionString(),
                        psqlOptions => psqlOptions.EnableRetryOnFailure()
                    )
                )
                .AddDbContext<TestDbContext2>(options =>
                    options.UseNpgsql(
                        fixture.PostgreSqlContainers[1].GetConnectionString(),
                        psqlOptions => psqlOptions.EnableRetryOnFailure()
                    )
                )
                .AddDbContext<TestDbContext3WithoutRetry>(options =>
                    options.UseNpgsql(fixture.PostgreSqlContainers[2].GetConnectionString())
                )
        );
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await ClearTestData<TestDbContext1>();
        await ClearTestData<TestDbContext2>();
        await ClearTestData<TestDbContext3WithoutRetry>();
    }

    public class NoTransactionTests(DbContextTransactionExtensionsTestFixture fixture)
        : DbContextTransactionExtensionsTests(fixture)
    {
        [Fact]
        public async Task SucceedWithoutTransaction()
        {
            await WriteToAllDbContexts();
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
                await using var dbContext1 = GetDbContext1();

                await dbContext1.RequireTransaction(() => WriteToAllDbContexts());

                AssertEntitiesInAllDbContexts();
            }

            [Fact]
            public async Task SucceedWithEither()
            {
                await using var dbContext1 = GetDbContext1();

                await dbContext1.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts();
                    return new Either<int, string>("success!");
                });

                AssertEntitiesInAllDbContexts();
            }

            [Fact]
            public async Task Fail()
            {
                await using var dbContext1 = GetDbContext1();

                await Assert.ThrowsAsync<SimulateFailureException>(() =>
                    dbContext1.RequireTransaction(async () =>
                    {
                        await WriteToAllDbContexts();
                        throw new SimulateFailureException();
                    })
                );

                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailWithEither()
            {
                await using var dbContext1 = GetDbContext1();

                await dbContext1.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts();
                    return new Either<int, string>(1);
                });

                AssertNoEntitiesInAnyDbContexts();
            }
        }

        public class NestedTransactionTests(DbContextTransactionExtensionsTestFixture fixture)
            : RequireTransactionTests(fixture)
        {
            [Fact]
            public async Task SucceedWithinNestedTransaction()
            {
                await using var dbContext1 = GetDbContext1();

                await dbContext1.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts(1);
                    await dbContext1.RequireTransaction(() => WriteToAllDbContexts(2));
                });

                AssertEntitiesInAllDbContexts(1);
                AssertEntitiesInAllDbContexts(2);
            }

            [Fact]
            public async Task SucceedWithinNestedTransaction_MultipleContextsRequestTransaction()
            {
                await using var dbContext1 = GetDbContext1();
                await using var dbContext2 = GetDbContext2();
                await using var dbContext3WithoutRetry = GetDbContext3WithoutRetry();

                await dbContext1.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts();
                    await dbContext2.RequireTransaction(async () =>
                    {
                        await WriteToAllDbContexts(2);
                        await dbContext3WithoutRetry.RequireTransaction(() => WriteToAllDbContexts(3));
                    });
                });

                AssertEntitiesInAllDbContexts(1);
                AssertEntitiesInAllDbContexts(2);
                AssertEntitiesInAllDbContexts(3);
            }

            [Fact]
            public async Task TransactionInitiatedByNonRetryDbContext_ThrowsException()
            {
                await using var dbContext2 = GetDbContext2();
                await using var dbContext3WithoutRetry = GetDbContext3WithoutRetry();

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    dbContext3WithoutRetry.RequireTransaction(async () =>
                    {
                        await WriteToAllDbContexts();
                        // ReSharper disable once AccessToDisposedClosure
                        await dbContext2.RequireTransaction(() => WriteToAllDbContexts(2));
                    })
                );

                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailWithinNestedTransaction()
            {
                await using var dbContext1 = GetDbContext1();

                await Assert.ThrowsAsync<SimulateFailureException>(() =>
                    dbContext1.RequireTransaction(async () =>
                    {
                        await WriteToAllDbContexts();
                        // ReSharper disable once AccessToDisposedClosure
                        await dbContext1.RequireTransaction(async () =>
                        {
                            await WriteToAllDbContexts(2);
                            throw new SimulateFailureException();
                        });
                    })
                );

                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailWithinNestedTransaction_WithEither()
            {
                await using var dbContext1 = GetDbContext1();
                await using var dbContext2 = GetDbContext1();

                await dbContext1.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts();
                    return await dbContext2.RequireTransaction(async () =>
                    {
                        await WriteToAllDbContexts(2);
                        return new Either<int, string>(1);
                    });
                });

                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailAtTopLevelWithNestedTransaction()
            {
                await using var dbContext1 = GetDbContext1();

                await Assert.ThrowsAsync<SimulateFailureException>(() =>
                    dbContext1.RequireTransaction(async () =>
                    {
                        await WriteToAllDbContexts();
                        // ReSharper disable once AccessToDisposedClosure
                        await dbContext1.RequireTransaction(() => WriteToAllDbContexts(2));
                        throw new SimulateFailureException();
                    })
                );

                AssertNoEntitiesInAnyDbContexts();
            }

            [Fact]
            public async Task FailAtTopLevelWithNestedTransaction_WithEither()
            {
                await using var dbContext1 = GetDbContext1();

                await dbContext1.RequireTransaction(async () =>
                {
                    await WriteToAllDbContexts();
                    await dbContext1.RequireTransaction(() => WriteToAllDbContexts(2));
                    return new Either<int, string>(1);
                });

                AssertNoEntitiesInAnyDbContexts();
            }
        }
    }

    private void AssertEntitiesInAllDbContexts(int expectedId = 1)
    {
        using var dbContext1 = GetDbContext1();
        using var dbContext2 = GetDbContext2();
        using var dbContext3 = GetDbContext3WithoutRetry();

        Assert.NotNull(dbContext1.Entities.SingleOrDefault(entity => entity.Id == expectedId));

        Assert.NotNull(dbContext2.Entities.SingleOrDefault(entity => entity.Id == expectedId));

        Assert.NotNull(dbContext3.Entities.SingleOrDefault(entity => entity.Id == expectedId));
    }

    private void AssertNoEntitiesInAnyDbContexts()
    {
        using var dbContext1 = GetDbContext1();
        using var dbContext2 = GetDbContext2();
        using var dbContext3 = GetDbContext3WithoutRetry();

        Assert.Empty(dbContext1.Entities);
        Assert.Empty(dbContext2.Entities);
        Assert.Empty(dbContext3.Entities);
    }

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
    public class DbContextTransactionExtensionsTestFixture : IAsyncLifetime
    {
        public readonly PostgreSqlContainer[] PostgreSqlContainers = Enumerable
            .Range(0, 3)
            .Select(_ =>
                new PostgreSqlBuilder()
                    .WithImage("postgres:16.1-alpine")
                    .WithCommand("-c", "max_prepared_transactions=100")
                    .Build()
            )
            .ToArray();

        /// <summary>
        /// Add prepared transaction support to the PostgreSQL containers so that a shared transaction
        /// can be created for the various DbContexts in this test.
        ///
        /// Create an "Entities" table in each database for Entity Framework to read and write to.
        /// </summary>
        public async Task InitializeAsync() =>
            await Task.WhenAll(
                PostgreSqlContainers.Select(async container =>
                {
                    await container.StartAsync();
                    await container.ExecScriptAsync("""CREATE TABLE IF NOT EXISTS "Entities" ("Id" int PRIMARY KEY)""");
                    return Task.CompletedTask;
                })
            );

        public async Task DisposeAsync() =>
            await Task.WhenAll(PostgreSqlContainers.Select(async container => await container.DisposeAsync()));
    }

    private async Task WriteToAllDbContexts(int id = 1)
    {
        var dbContext1 = _testApp.Services.GetRequiredService<TestDbContext1>();
        var dbContext2 = _testApp.Services.GetRequiredService<TestDbContext2>();
        var dbContext3WithoutRetry = _testApp.Services.GetRequiredService<TestDbContext3WithoutRetry>();
        await dbContext1.Entities.AddAsync(new TestEntity { Id = id });
        await dbContext1.SaveChangesAsync();
        await dbContext2.Entities.AddAsync(new TestEntity { Id = id });
        await dbContext2.SaveChangesAsync();
        await dbContext3WithoutRetry.Entities.AddAsync(new TestEntity { Id = id });
        await dbContext3WithoutRetry.SaveChangesAsync();
    }

    private async Task ClearTestData<TDbContext>()
        where TDbContext : DbContext
    {
        var context = _testApp.GetDbContext<TDbContext, TestStartup>();
        await context.ClearTestData();
    }

    private TDbContext GetDbContext<TDbContext>()
        where TDbContext : DbContext
    {
        return _testApp.GetDbContext<TDbContext, TestStartup>();
    }

    private TestDbContext1 GetDbContext1()
    {
        return _testApp.GetDbContext<TestDbContext1, TestStartup>();
    }

    private TestDbContext2 GetDbContext2()
    {
        return _testApp.GetDbContext<TestDbContext2, TestStartup>();
    }

    private TestDbContext3WithoutRetry GetDbContext3WithoutRetry()
    {
        return _testApp.GetDbContext<TestDbContext3WithoutRetry, TestStartup>();
    }
}

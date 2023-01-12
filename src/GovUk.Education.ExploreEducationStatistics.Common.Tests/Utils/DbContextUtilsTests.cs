#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public class DbContextUtilsTests
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local, MemberHidesStaticFromOuterClass
        public class UpdateTimestamps
        {
            private class CreatedDateTimeEntity : ICreatedTimestamp<DateTime>
            {
                public Guid Id { get; set; }
                public DateTime Created { get; set; }
            }

            private class CreatedDateTimeOffsetEntity : ICreatedTimestamp<DateTimeOffset>
            {
                public Guid Id { get; set; }
                public DateTimeOffset Created { get; set; }
            }

            private class CreatedInvalidEntity : ICreatedTimestamp<string>
            {
                public Guid Id { get; set; }
                public string Created { get; set; } = string.Empty;
            }

            private class UpdatedDateTimeEntity : IUpdatedTimestamp<DateTime?>
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public DateTime? Updated { get; set; }
            }

            private class UpdatedDateTimeOffsetEntity : IUpdatedTimestamp<DateTimeOffset?>
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public DateTimeOffset? Updated { get; set; }
            }

            private class UpdatedInvalidEntity : IUpdatedTimestamp<string>
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;

                public string Updated { get; set; } = string.Empty;
            }

            private class CreatedUpdatedDateTimeEntity : ICreatedUpdatedTimestamps<DateTime?, DateTime?>
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public DateTime? Created { get; set; }
                public DateTime? Updated { get; set; }
            }

            private class CreatedUpdatedDateTimeOffsetEntity : ICreatedUpdatedTimestamps<DateTimeOffset?, DateTimeOffset?>
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public DateTimeOffset? Created { get; set; }
                public DateTimeOffset? Updated { get; set; }
            }

            private class CreatedUpdatedInvalidEntity : ICreatedUpdatedTimestamps<string, int>
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public string Created { get; set; } = string.Empty;
                public int Updated { get; set; }
            }

            private class SoftDeletedDateTimeEntity : ISoftDeletedTimestamp<DateTime?>
            {
                public Guid Id { get; set; }
                public DateTime? SoftDeleted { get; set; }
            }

            private class SoftDeletedDateTimeOffsetEntity : ISoftDeletedTimestamp<DateTimeOffset?>
            {
                public Guid Id { get; set; }

                public DateTimeOffset? SoftDeleted { get; set; }
            }

            private class SoftDeletedInvalidEntity : ISoftDeletedTimestamp<string>
            {
                public Guid Id { get; set; }
                public string SoftDeleted { get; set; } = string.Empty;
            }

            private sealed class TestContext : DbContext
            {
                public DbSet<CreatedDateTimeEntity> CreatedDateTimeEntity { get; set; } = null!;
                public DbSet<CreatedDateTimeOffsetEntity> CreatedDateTimeOffsetEntity { get; set; } = null!;
                public DbSet<CreatedInvalidEntity> CreatedInvalidEntity { get; set; } = null!;

                public DbSet<UpdatedDateTimeEntity> UpdateDateTimeEntity { get; set; } = null!;
                public DbSet<UpdatedDateTimeOffsetEntity> UpdatedDateTimeOffsetEntity { get; set; } = null!;
                public DbSet<UpdatedInvalidEntity> UpdatedInvalidEntity { get; set; } = null!;

                public DbSet<CreatedUpdatedDateTimeEntity> CreatedUpdatedDateTimeEntity { get; set; } = null!;
                public DbSet<CreatedUpdatedDateTimeOffsetEntity> CreatedUpdatedDateTimeOffsetEntity { get; set; } = null!;
                public DbSet<CreatedUpdatedInvalidEntity> CreatedUpdatedInvalidEntity { get; set; } = null!;

                public DbSet<SoftDeletedDateTimeEntity> SoftDeletedDateTimeEntity { get; set; } = null!;
                public DbSet<SoftDeletedDateTimeOffsetEntity> SoftDeletedDateTimeOffsetEntity { get; set; } = null!;
                public DbSet<SoftDeletedInvalidEntity> SoftDeletedInvalidEntity { get; set; } = null!;

                public TestContext(string contextName) : base(GetOptions(contextName))
                {
                    ChangeTracker.StateChanged += DbContextUtils.UpdateTimestamps;
                    ChangeTracker.Tracked += DbContextUtils.UpdateTimestamps;
                }

                private static DbContextOptions<TestContext> GetOptions(string contextName)
                {
                    return new DbContextOptionsBuilder<TestContext>()
                        .UseInMemoryDatabase(contextName)
                        .Options;
                }
            }
            // ReSharper enable UnusedAutoPropertyAccessor.Local, MemberHidesStaticFromOuterClass

            [Fact]
            public async Task CreatedTimestamp_DateTime()
            {
                var entity = new CreatedDateTimeEntity();

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                // Check entity was created correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.CreatedDateTimeEntity.FindAsync(entity.Id);

                    Assert.NotNull(saved);
                    Assert.NotEqual(default, saved!.Created);
                    Assert.True(DateTime.UtcNow > saved.Created);
                }
            }

            [Fact]
            public async Task CreatedTimestamp_DateTimeOffset()
            {
                var entity = new CreatedDateTimeOffsetEntity();

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                // Check entity was created correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.CreatedDateTimeOffsetEntity.FindAsync(entity.Id);

                    Assert.NotNull(saved);
                    Assert.NotEqual(default, saved!.Created);
                    Assert.True(DateTime.UtcNow > saved.Created);
                }
            }
            
            [Fact]
            public async Task CreatedTimestamp_DateTime_AlreadySet()
            {
                var entityCreated = DateTime.UtcNow.AddDays(-10);
                
                var entity = new CreatedDateTimeEntity
                {
                    Created = entityCreated
                };

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                // Check entity's created data was left alone as it was already set
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.CreatedDateTimeEntity.FindAsync(entity.Id);

                    Assert.Equal(entityCreated, saved!.Created);
                }
            }

            [Fact]
            public async Task CreatedTimestamp_DateTimeOffset_AlreadySet()
            {
                var entityCreated = DateTimeOffset.UtcNow.AddDays(-10);

                var entity = new CreatedDateTimeOffsetEntity
                {
                    Created = entityCreated
                };

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                // Check entity's created data was left alone as it was already set
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.CreatedDateTimeOffsetEntity.FindAsync(entity.Id);

                    Assert.Equal(entityCreated, saved!.Created);
                }
            }

            [Fact]
            public async Task CreatedTimestamp_InvalidType_Throws()
            {
                var entity = new CreatedInvalidEntity();

                var contextId = Guid.NewGuid().ToString();

                await using var context = new TestContext(contextId);

                var exception = await Assert.ThrowsAsync<NotImplementedException>(
                    async () =>
                    {
                        await context.AddAsync(entity);
                        await context.SaveChangesAsync();
                    }
                );

                Assert.Equal(
                    "Entity does not implement valid timestamp field for ICreatedTimestamp",
                    exception.Message
                );
            }

            [Fact]
            public async Task UpdatedTimestamp_DateTime()
            {
                var entity = new UpdatedDateTimeEntity
                {
                    Name = "Old name"
                };

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                // Check entity was created correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.UpdateDateTimeEntity.FindAsync(entity.Id);

                    Assert.Null(saved.Updated);
                }

                // Update the entity
                await using (var context = new TestContext(contextId))
                {
                    context.Update(entity);

                    entity.Name = "New name";

                    await context.SaveChangesAsync();
                }

                // Check entity was updated correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.UpdateDateTimeEntity.FindAsync(entity.Id);

                    Assert.Equal("New name", saved.Name);

                    Assert.NotNull(saved.Updated);
                    Assert.True(DateTime.UtcNow > saved.Updated);
                }
            }

            [Fact]
            public async Task UpdatedTimestamp_DateTimeOffset()
            {
                var entity = new UpdatedDateTimeOffsetEntity
                {
                    Name = "Old name"
                };

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                // Check entity was created correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.UpdatedDateTimeOffsetEntity.FindAsync(entity.Id);

                    Assert.Null(saved.Updated);
                }

                // Update the entity
                await using (var context = new TestContext(contextId))
                {
                    context.Update(entity);

                    entity.Name = "New name";

                    await context.SaveChangesAsync();
                }

                // Check entity was updated correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.UpdatedDateTimeOffsetEntity.FindAsync(entity.Id);

                    Assert.Equal("New name", saved.Name);

                    Assert.NotNull(saved.Updated);
                    Assert.True(DateTime.UtcNow > saved.Updated);
                }
            }

            [Fact]
            public async Task UpdatedTimestamp_InvalidType_Throws()
            {
                var entity = new UpdatedInvalidEntity();

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                await using (var context = new TestContext(contextId))
                {
                    var exception = await Assert.ThrowsAsync<NotImplementedException>(
                        async () =>
                        {
                            context.Update(entity);

                            entity.Name = "New name";

                            await context.SaveChangesAsync();
                        }
                    );

                    Assert.Equal(
                        "Entity does not implement valid timestamp field for IUpdatedTimestamp",
                        exception.Message
                    );
                }
            }

            [Fact]
            public async Task CreatedUpdatedTimestamps_DateTime()
            {
                var entity = new CreatedUpdatedDateTimeEntity
                {
                    Name = "Old name"
                };

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                DateTime? previousCreated;
                DateTime? previousUpdated;

                // Check entity was created correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.CreatedUpdatedDateTimeEntity.FindAsync(entity.Id);

                    Assert.NotNull(saved.Created);
                    Assert.True(DateTime.UtcNow > saved.Created);

                    Assert.Null(saved.Updated);

                    previousCreated = saved.Created;
                    previousUpdated = saved.Updated;
                }

                // Update the entity
                await using (var context = new TestContext(contextId))
                {
                    context.Update(entity);

                    entity.Name = "New name";

                    await context.SaveChangesAsync();
                }

                // Check entity was updated correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.CreatedUpdatedDateTimeEntity.FindAsync(entity.Id);

                    Assert.Equal("New name", saved.Name);

                    Assert.NotNull(saved.Created);
                    Assert.True(DateTime.UtcNow > saved.Created);
                    Assert.Equal(previousCreated, saved.Created);

                    Assert.NotNull(saved.Updated);
                    Assert.True(DateTime.UtcNow > saved.Updated);
                    Assert.NotEqual(previousUpdated, saved.Updated);
                }
            }

            [Fact]
            public async Task CreatedUpdatedTimestamps_DateTimeOffsets()
            {
                var entity = new CreatedUpdatedDateTimeOffsetEntity
                {
                    Name = "Old name"
                };

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                DateTimeOffset? previousCreated;
                DateTimeOffset? previousUpdated;

                // Check entity was created correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.CreatedUpdatedDateTimeOffsetEntity.FindAsync(entity.Id);

                    Assert.NotNull(saved.Created);
                    Assert.True(DateTime.UtcNow > saved.Created);

                    Assert.Null(saved.Updated);

                    previousCreated = saved.Created;
                    previousUpdated = saved.Updated;
                }

                // Update the entity
                await using (var context = new TestContext(contextId))
                {
                    context.Update(entity);

                    entity.Name = "New name";

                    await context.SaveChangesAsync();
                }

                // Check entity was updated correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.CreatedUpdatedDateTimeOffsetEntity.FindAsync(entity.Id);

                    Assert.Equal("New name", saved.Name);

                    Assert.NotNull(saved.Created);
                    Assert.True(DateTime.UtcNow > saved.Created);
                    Assert.Equal(previousCreated, saved.Created);

                    Assert.NotNull(saved.Updated);
                    Assert.True(DateTime.UtcNow > saved.Updated);
                    Assert.NotEqual(previousUpdated, saved.Updated);
                }
            }

            [Fact]
            public async Task CreatedUpdatedTimestamps_InvalidType_Throws()
            {
                var entity = new CreatedUpdatedInvalidEntity();

                var contextId = Guid.NewGuid().ToString();

                await using var context = new TestContext(contextId);

                var exception = await Assert.ThrowsAsync<NotImplementedException>(
                    async () =>
                    {
                        await context.AddAsync(entity);
                        await context.SaveChangesAsync();

                        entity.Name = "New name";

                        await context.SaveChangesAsync();
                    }
                );

                Assert.Equal(
                    "Entity does not implement valid timestamp field for ICreatedTimestamp",
                    exception.Message
                );
            }

            [Fact]
            public async Task SoftDeletedTimestamp_DateTime()
            {
                var entity = new SoftDeletedDateTimeEntity();

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                // Delete the entity
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.SoftDeletedDateTimeEntity.FindAsync(entity.Id);

                    context.Remove(saved);
                    await context.SaveChangesAsync();
                }

                // Check entity was soft deleted correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.SoftDeletedDateTimeEntity.FindAsync(entity.Id);

                    Assert.NotNull(saved.SoftDeleted);
                    Assert.True(DateTime.UtcNow > saved.SoftDeleted);
                }
            }

            [Fact]
            public async Task SoftDeletedTimestamp_DateTimeOffset()
            {
                var entity = new SoftDeletedDateTimeOffsetEntity();

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                // Delete the entity
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.SoftDeletedDateTimeOffsetEntity.FindAsync(entity.Id);

                    context.Remove(saved);
                    await context.SaveChangesAsync();
                }

                // Check entity was soft deleted correctly
                await using (var context = new TestContext(contextId))
                {
                    var saved = await context.SoftDeletedDateTimeOffsetEntity.FindAsync(entity.Id);

                    Assert.NotNull(saved.SoftDeleted);
                    Assert.True(DateTime.UtcNow > saved.SoftDeleted);
                }
            }

            [Fact]
            public async Task SoftDeletedTimestamp_InvalidType_Throws()
            {
                var entity = new SoftDeletedInvalidEntity();

                var contextId = Guid.NewGuid().ToString();

                await using (var context = new TestContext(contextId))
                {
                    await context.AddAsync(entity);
                    await context.SaveChangesAsync();
                }

                await using (var context = new TestContext(contextId))
                {
                    var exception = await Assert.ThrowsAsync<NotImplementedException>(
                        async () =>
                        {
                            context.Remove(entity);

                            await context.SaveChangesAsync();
                        }
                    );

                    Assert.Equal(
                        "Entity does not implement valid timestamp field for ISoftDeletedTimestamp",
                        exception.Message
                    );
                }
            }
        }
    }
}

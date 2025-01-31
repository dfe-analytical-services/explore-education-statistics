#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public static class DbContextUtils
    {
        public static void UpdateTimestamps(object? sender, EntityEntryEventArgs e)
        {
            var entity = e.Entry.Entity;
            var now = DateTime.UtcNow;

            if (e.Entry.State == EntityState.Added && entity is ITimestampsInternal.ICreated)
            {
                switch (e.Entry.Entity)
                {
                    case ICreatedTimestamp<DateTime> entityWithCreated:
                        if (entityWithCreated.Created.Equals(default))
                        {
                            entityWithCreated.Created = now;
                        }
                        break;

                    case ICreatedTimestamp<DateTime?> entityWithCreated:
                        if (entityWithCreated.Created == null)
                        {
                            entityWithCreated.Created = now;
                        }
                        break;

                    case ICreatedTimestamp<DateTimeOffset> entityWithCreated:
                        if (entityWithCreated.Created.Equals(default))
                        {
                            entityWithCreated.Created = now;
                        }
                        break;

                    case ICreatedTimestamp<DateTimeOffset?> entityWithCreated:
                        if (entityWithCreated.Created == null)
                        {
                            entityWithCreated.Created = now;
                        }
                        break;

                    default:
                        throw new NotImplementedException(
                            "Entity does not implement valid timestamp field for " +
                            $"{typeof(ICreatedTimestamp<>).GetNameWithoutGenericArity()}"
                        );
                }

                return;
            }

            if (e.Entry.State == EntityState.Modified && entity is ITimestampsInternal.IUpdated)
            {
                switch (e.Entry.Entity)
                {
                    case IUpdatedTimestamp<DateTime> entityWithUpdated:
                        entityWithUpdated.Updated = now;
                        break;

                    case IUpdatedTimestamp<DateTime?> entityWithUpdated:
                        entityWithUpdated.Updated = now;
                        break;

                    case IUpdatedTimestamp<DateTimeOffset> entityWithUpdated:
                        entityWithUpdated.Updated = now;
                        break;

                    case IUpdatedTimestamp<DateTimeOffset?> entityWithUpdated:
                        entityWithUpdated.Updated = now;
                        break;

                    default:
                        throw new NotImplementedException(
                            "Entity does not implement valid timestamp field for " +
                            $"{typeof(IUpdatedTimestamp<>).GetNameWithoutGenericArity()}"
                        );
                }

                return;
            }

            if (e.Entry.State == EntityState.Deleted && entity is ITimestampsInternal.ISoftDeleted)
            {
                switch (e.Entry.Entity)
                {
                    case ISoftDeletedTimestamp<DateTime> entityWithDeleted:
                        entityWithDeleted.SoftDeleted = now;
                        break;

                    case ISoftDeletedTimestamp<DateTime?> entityWithDeleted:
                        entityWithDeleted.SoftDeleted = now;
                        break;

                    case ISoftDeletedTimestamp<DateTimeOffset> entityWithDeleted:
                        entityWithDeleted.SoftDeleted = now;
                        break;

                    case ISoftDeletedTimestamp<DateTimeOffset?> entityWithDeleted:
                        entityWithDeleted.SoftDeleted = now;
                        break;

                    default:
                        throw new NotImplementedException(
                            "Entity does not implement valid timestamp field for " +
                            $"{typeof(ISoftDeletedTimestamp<>).GetNameWithoutGenericArity()}"
                        );
                }

                // Note that deletion will not actually delete the entity.
                // We mark the state as `Unchanged` to prevent all of the
                // entity's fields being updated in the database.
                e.Entry.State = EntityState.Unchanged;
            }
        }
    }
}

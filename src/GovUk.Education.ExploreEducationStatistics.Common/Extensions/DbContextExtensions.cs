#nullable enable
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Thinktecture.Internal;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Ensure that an entity is not detached from the database context, reloading it if necessary before returning.
        /// Throws an exception if the entity cannot be reloaded.
        /// </summary>
        public static T AssertEntityLoaded<T>(this DbContext context, T entity) where T : class
        {
            var reloaded = context.ReloadEntity(entity);

            if (reloaded is null)
            {
                var displayString = context.GetDisplayString(entity);
                throw new InvalidOperationException($"Unable to reload {displayString}");
            }

            return reloaded;
        }

        /// <summary>
        /// Reload an entity into the context if it has been detached.
        /// </summary>
        /// <remarks>
        /// If it has been deleted, or cannot be reloaded, we return null as the
        /// entity would be unsafe to work with.
        /// <para>
        /// This is most useful in code where we receive an entity from some other
        /// service (e.g. in authorization handlers). As we cannot be sure about the
        /// tracking state of the entity, it is recommended to call this method to
        /// ensure that it is not stale and has not been deleted.
        /// </para>
        /// </remarks>
        public static T? ReloadEntity<T>(this DbContext context, T entity) where T : class
        {
            var entry = context.Entry(entity);

            // The entity has been detached from the context
            // and may be stale, so we need to reload it for
            // the most up-to-date version.
            if (entry.State == EntityState.Detached)
            {
                entry.Reload();
            }

            if (entry.State == EntityState.Deleted)
            {
                return null;
            }

            // If entry is still detached, we can only assume
            // it does not exist in the database anymore and
            // is now unsafe to work with (it should be null).
            if (entry.State == EntityState.Detached)
            {
                return null;
            }

            return entity;
        }

        /// <summary>
        /// Try to reload an entity, returning false if it cannot be reloaded.
        /// </summary>
        public static bool TryReloadEntity<T>(this DbContext context, T entity, out T reloadedEntity)
            where T : class
        {
            reloadedEntity = entity;

            var reloaded = context.ReloadEntity(entity);

            if (reloaded is null)
            {
                return false;
            }

            reloadedEntity = reloaded;
            return true;
        }

        public static DbSet<TEntity> TempTableSet<TEntity>(this DbContext dbContext) where TEntity : class
        {
            return dbContext.Set<TEntity>(EntityNameProvider.GetTempTableName(typeof(TEntity)));
        } 

        private static string GetDisplayString(this DbContext context, object entity)
        {
            var entry = context.Entry(entity);
            var primaryKey = entry.Metadata.FindPrimaryKey();
            var typeName = entity.GetType().ShortDisplayName();
            var primaryKeyValues = primaryKey.Properties
                .Select(property => $"{property.Name}: '{property.PropertyInfo.GetValue(entity)}'")
                .JoinToString(", ");

            return $"{typeName} {{{primaryKeyValues}}}";
        }
    }
}

#nullable enable
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class DbContextExtensions
    {
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

    }
}
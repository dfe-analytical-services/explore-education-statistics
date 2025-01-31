#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public interface ICreatedTimestamp<TDate> : ITimestampsInternal.ICreated
    {
        TDate Created { get; set; }
    }

    public interface IUpdatedTimestamp<TDate> : ITimestampsInternal.IUpdated
    {
        TDate Updated { get; set; }
    }

    public interface ISoftDeletedTimestamp<TDate> : ITimestampsInternal.ISoftDeleted
    {
        TDate SoftDeleted { get; set; }
    }

    public interface ICreatedUpdatedTimestamps<TCreated, TUpdated>
        : ICreatedTimestamp<TCreated>, IUpdatedTimestamp<TUpdated>
    {
    }

    /// <summary>
    /// Marker interfaces for type guards without needing reflection.
    /// Don't use this application level code (should only be
    /// used in Entity Framework infrastructure code).
    /// </summary>
    public interface ITimestampsInternal
    {
        public interface ICreated {}
        public interface IUpdated {}
        public interface ISoftDeleted {}
    }
}
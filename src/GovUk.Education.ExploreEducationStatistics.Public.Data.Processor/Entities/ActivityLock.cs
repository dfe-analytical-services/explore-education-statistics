namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Entities;

/// <summary>
/// Durable entity which has no state or operations defined except for the built-in lock and unlock operations.
/// Obtaining a lock on this entity can be used to create a critical section during orchestration.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
internal class ActivityLock;

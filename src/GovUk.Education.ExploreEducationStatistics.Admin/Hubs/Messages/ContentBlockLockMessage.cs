#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Messages;

public record ContentBlockLockMessage
{
    public Guid Id { get; init; }

    public bool Force { get; init; } = false;
}
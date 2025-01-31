#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Messages;

public record ReleaseMessage
{
    public Guid Id { get; init; }
}
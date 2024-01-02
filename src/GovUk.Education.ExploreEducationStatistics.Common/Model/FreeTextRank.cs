#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public record FreeTextRank(Guid Id, int Rank);

public record FreeTextValueResult<TValue>
{
    public TValue Value { get; init; }
    public int Rank { get; init; }
}

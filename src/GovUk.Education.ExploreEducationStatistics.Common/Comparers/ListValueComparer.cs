#nullable enable
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GovUk.Education.ExploreEducationStatistics.Common.Comparers;

public class ListValueComparer<TValue>(IEqualityComparer<TValue>? equalityComparer = null)
    : ValueComparer<List<TValue>>(
        equalsExpression: (list, otherList) => list != null
                                               && otherList != null
                                               && list.SequenceEqual(otherList, equalityComparer),
        hashCodeExpression: list => list.Aggregate(0, (acc, value) =>
            value != null
                ? HashCode.Combine(acc, value.GetHashCode())
                : acc),
        snapshotExpression: list => list.ToList()
    );

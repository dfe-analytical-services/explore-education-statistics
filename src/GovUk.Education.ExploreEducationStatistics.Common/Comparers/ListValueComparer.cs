#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GovUk.Education.ExploreEducationStatistics.Common.Comparers;

public class ListValueComparer<TValue>(IEqualityComparer<TValue>? equalityComparer = null)
    : ValueComparer<List<TValue>>(
        (c1, c2) => c1!.SequenceEqual(c2!, equalityComparer),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
        c => c.ToList()
    );

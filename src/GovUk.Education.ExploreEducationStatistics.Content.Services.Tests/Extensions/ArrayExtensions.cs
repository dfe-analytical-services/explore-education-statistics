using System;
using System.Diagnostics.Contracts;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Extensions;

public static class ArrayExtensions
{
    [Pure]
    public static T[] Shuffle<T>(this T[] array, int? seed = null)
    {
        var shuffledArray = new T[array.Length];
        array.CopyTo(shuffledArray, 0);
        var rnd = seed.HasValue ? new Random(seed.Value) : new Random();
        for (var i = shuffledArray.Length-1; i > 0 ; i--)
        {
            var swapIndex = rnd.NextInt64(i + 1);
            (shuffledArray[i], shuffledArray[swapIndex]) = (shuffledArray[swapIndex], shuffledArray[i]);
        }
        return shuffledArray;
    }
}

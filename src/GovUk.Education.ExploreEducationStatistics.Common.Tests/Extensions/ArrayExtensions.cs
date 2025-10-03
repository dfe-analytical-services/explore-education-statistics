using System.Diagnostics.Contracts;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class ArrayExtensions
{
    [Pure]
    public static T[] Shuffle<T>(this T[] array, int? seed = null)
    {
        var shuffledArray = new T[array.Length];
        array.CopyTo(shuffledArray, 0);

        var rnd = seed.HasValue ? new Random(seed.Value) : new Random();

        for (var i = shuffledArray.Length - 1; i > 0; i--)
        {
            var swapIndex = rnd.NextInt64(i + 1);
            (shuffledArray[i], shuffledArray[swapIndex]) = (shuffledArray[swapIndex], shuffledArray[i]);
        }
        return shuffledArray;
    }

    /// <summary>
    /// Take an array of items and distribute them evenly across the specified
    /// number of arrays.
    /// e.g. [1,2,3,4,5] into 2 groups would yield [[1,3,5],[2,4]]
    /// </summary>
    [Pure]
    public static T[][] DistributeIntoGroups<T>(this T[] array, int numberOfGroups)
    {
        var numberOfItems = array.Length;
        var itemsPerGroup = numberOfItems / numberOfGroups;
        var remainder = numberOfItems % numberOfGroups;
        var arrays = Enumerable
            .Range(0, numberOfGroups)
            .Select(i => i < remainder ? itemsPerGroup + 1 : itemsPerGroup)
            .Select(sizeOfArray => new T[sizeOfArray])
            .ToArray();

        for (var i = 0; i < numberOfItems; i++)
        {
            var arrayIndex = i % numberOfGroups;
            var indexOfArray = i / numberOfGroups;
            arrays[arrayIndex][indexOfArray] = array[i];
        }
        return arrays;
    }
}

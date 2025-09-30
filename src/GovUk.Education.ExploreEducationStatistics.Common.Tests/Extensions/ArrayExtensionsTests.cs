using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public class ArrayExtensionsTests
{
    public class ShuffleTests
    {
        [Fact]
        public void Shuffle_should_not_reorder_items_in_source_array()
        {
            var source = Enumerable.Range(1, 10).ToArray();
            var expected = source.ToArray();
            var _ = source.Shuffle();
            Assert.Equal(expected, source);
        }

        [Fact]
        public void Shuffle_should_retain_all_items_in_source_array()
        {
            var source = Enumerable.Range(1, 100).ToArray();
            var actual = source.Shuffle();
            Assert.Equal(source, actual.OrderBy(i => i));
        }

        [Fact]
        public void Shuffle_should_reorder_items_in_array()
        {
            var source = Enumerable.Range(1, 10).ToArray();
            var actual = source.Shuffle();
            Assert.NotEqual(source, actual);
        }
    }

    public class DistributeIntoGroupsTests
    {
        public static TheoryData<int[], int, int[][]> SplitIntoGroupsData = new()
        {
            {
                [],
                1,
                [
                    [],
                ]
            },
            {
                [],
                2,
                [
                    [],
                    [],
                ]
            },
            {
                [1],
                2,
                [
                    [1],
                    [],
                ]
            },
            {
                [1, 2],
                1,
                [
                    [1, 2],
                ]
            },
            {
                [1, 2],
                2,
                [
                    [1],
                    [2],
                ]
            },
            {
                [1, 2, 3, 4],
                2,
                [
                    [1, 3],
                    [2, 4],
                ]
            },
            {
                [1, 2, 3, 4, 5],
                2,
                [
                    [1, 3, 5],
                    [2, 4],
                ]
            },
            {
                [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
                3,
                [
                    [1, 4, 7, 10],
                    [2, 5, 8],
                    [3, 6, 9],
                ]
            },
        };

        [Theory]
        [MemberData(nameof(SplitIntoGroupsData))]
        public void SplitIntoGroups(int[] source, int numberOfGroups, int[][] expected) =>
            Assert.Equal(expected, source.DistributeIntoGroups(numberOfGroups));
    }
}

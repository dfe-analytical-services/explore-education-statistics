#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public class EnumerableExtensionsTests
{
    public class NaturalOrderByTests
    {
        [Fact]
        public void OrdersSingleCharacters()
        {
            TestRecord[] testRecords =
            [
                new() { Value = "d" },
                new() { Value = "1" },
                new() { Value = "b" },
                new() { Value = "c" },
                new() { Value = "a" },
            ];

            TestRecord[] expected =
            [
                new() { Value = "1" },
                new() { Value = "a" },
                new() { Value = "b" },
                new() { Value = "c" },
                new() { Value = "d" },
            ];

            var orderedRecords = testRecords.NaturalOrderBy(r => r.Value);

            Assert.Equal(expected, orderedRecords);
        }

        [Fact]
        public void OrdersMultipleCharacters()
        {
            TestRecord[] testRecords =
            [
                new() { Value = "z24" },
                new() { Value = "z2" },
                new() { Value = "abcde" },
                new() { Value = "z15" },
                new() { Value = "abd" },
                new() { Value = "z1" },
                new() { Value = "z3" },
                new() { Value = "abc" },
                new() { Value = "z20" },
                new() { Value = "z5" },
                new() { Value = "abcd" },
                new() { Value = "z11" },
                new() { Value = "z22" },
            ];

            TestRecord[] expected =
            [
                new() { Value = "abc" },
                new() { Value = "abcd" },
                new() { Value = "abcde" },
                new() { Value = "abd" },
                new() { Value = "z1" },
                new() { Value = "z2" },
                new() { Value = "z3" },
                new() { Value = "z5" },
                new() { Value = "z11" },
                new() { Value = "z15" },
                new() { Value = "z20" },
                new() { Value = "z22" },
                new() { Value = "z24" },
            ];

            var orderedRecords = testRecords.NaturalOrderBy(r => r.Value);

            Assert.Equal(expected, orderedRecords);
        }

        private record TestRecord
        {
            public required string Value { get; init; }
        }
    }

    public class NaturalThenByTests
    {
        [Fact]
        public void OrdersSingleCharacters()
        {
            TestRecord[] testRecords =
            [
                new() { FirstString = "1", SecondString = "d" },
                new() { FirstString = "1", SecondString = "1" },
                new() { FirstString = "1", SecondString = "b" },
                new() { FirstString = "1", SecondString = "c" },
                new() { FirstString = "1", SecondString = "a" },
            ];

            TestRecord[] expected =
            [
                new() { FirstString = "1", SecondString = "1" },
                new() { FirstString = "1", SecondString = "a" },
                new() { FirstString = "1", SecondString = "b" },
                new() { FirstString = "1", SecondString = "c" },
                new() { FirstString = "1", SecondString = "d" },
            ];

            var orderedRecords = testRecords.NaturalOrderBy(r => r.FirstString).NaturalThenBy(r => r.SecondString);

            Assert.Equal(expected, orderedRecords);
        }

        [Fact]
        public void OrdersMultipleCharacters()
        {
            TestRecord[] testRecords =
            [
                new() { FirstString = "1", SecondString = "z24" },
                new() { FirstString = "1", SecondString = "z2" },
                new() { FirstString = "1", SecondString = "abcde" },
                new() { FirstString = "1", SecondString = "z15" },
                new() { FirstString = "1", SecondString = "abd" },
                new() { FirstString = "1", SecondString = "z1" },
                new() { FirstString = "1", SecondString = "z3" },
                new() { FirstString = "1", SecondString = "abc" },
                new() { FirstString = "1", SecondString = "z20" },
                new() { FirstString = "1", SecondString = "z5" },
                new() { FirstString = "1", SecondString = "abcd" },
                new() { FirstString = "1", SecondString = "z11" },
                new() { FirstString = "1", SecondString = "z22" },
            ];

            TestRecord[] expected =
            [
                new() { FirstString = "1", SecondString = "abc" },
                new() { FirstString = "1", SecondString = "abcd" },
                new() { FirstString = "1", SecondString = "abcde" },
                new() { FirstString = "1", SecondString = "abd" },
                new() { FirstString = "1", SecondString = "z1" },
                new() { FirstString = "1", SecondString = "z2" },
                new() { FirstString = "1", SecondString = "z3" },
                new() { FirstString = "1", SecondString = "z5" },
                new() { FirstString = "1", SecondString = "z11" },
                new() { FirstString = "1", SecondString = "z15" },
                new() { FirstString = "1", SecondString = "z20" },
                new() { FirstString = "1", SecondString = "z22" },
                new() { FirstString = "1", SecondString = "z24" },
            ];

            var orderedRecords = testRecords.NaturalOrderBy(r => r.FirstString).NaturalThenBy(r => r.SecondString);

            Assert.Equal(expected, orderedRecords);
        }

        [Fact]
        public void PreservesOrderingOfOrderBy()
        {
            TestRecord[] testRecords =
            [
                new() { FirstString = "2", SecondString = "a" },
                new() { FirstString = "2", SecondString = "b" },
                new() { FirstString = "1", SecondString = "b" },
                new() { FirstString = "1", SecondString = "a" },
                new() { FirstString = "3", SecondString = "b" },
                new() { FirstString = "3", SecondString = "a" },
            ];

            TestRecord[] expected =
            [
                new() { FirstString = "1", SecondString = "a" },
                new() { FirstString = "1", SecondString = "b" },
                new() { FirstString = "2", SecondString = "a" },
                new() { FirstString = "2", SecondString = "b" },
                new() { FirstString = "3", SecondString = "a" },
                new() { FirstString = "3", SecondString = "b" },
            ];

            var orderedRecords = testRecords.NaturalOrderBy(r => r.FirstString).NaturalThenBy(r => r.SecondString);

            Assert.Equal(expected, orderedRecords);
        }

        private record TestRecord
        {
            public required string FirstString { get; init; }
            public required string SecondString { get; init; }
        }
    }

    [Fact]
    public void ToDictionaryIndexed()
    {
        var source = new List<string> { "a", "b", "c" };

        var result = source.ToDictionaryIndexed(value => value, (value, index) => (Value: value, Index: index));

        var expected = new Dictionary<string, (string S, int Index)>
        {
            { "a", ("a", 0) },
            { "b", ("b", 1) },
            { "c", ("c", 2) },
        };

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IndexOfFirst()
    {
        var result = new List<int> { 1, 2, 3 }.IndexOfFirst(value => value == 2);

        Assert.Equal(1, result);
    }

    [Fact]
    public void IndexOfFirst_NoMatch()
    {
        var result = new List<int> { 1, 2, 3 }.IndexOfFirst(value => value == 4);

        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task ForEachAsync_SuccessfulEitherList()
    {
        var results = await new List<int> { 1, 2 }.ForEachAsync(GetSuccessfulEither);

        Assert.True(results.IsRight);

        var resultsList = results.Right;
        Assert.Equal(2, resultsList.Count);
        Assert.Contains(1, resultsList);
        Assert.Contains(2, resultsList);
    }

    [Fact]
    public async Task ForEachAsync_FailingEither()
    {
        var results = await new List<int> { 1, -1, 2 }.ForEachAsync(async value =>
        {
            if (value == -1)
            {
                return await GetFailingEither();
            }

            return await GetSuccessfulEither(value);
        });

        Assert.True(results.IsLeft);
    }

    [Fact]
    public void SelectNullSafe_NotNull()
    {
        var list = new List<int> { 1, 2, 3 };
        var results = list.SelectNullSafe(value => value * 2).ToList();
        Assert.Equal([2, 4, 6], results);
    }

    [Fact]
    public void SelectNullSafe_Null()
    {
        var results = ((List<int>?)null).SelectNullSafe(value => value * 2).ToList();
        Assert.Equal(new List<int>(), results);
    }

    [Fact]
    public void IsNullOrEmpty_Null()
    {
        Assert.True(((List<string>?)null).IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_Empty()
    {
        Assert.True(new List<string>().IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_NeitherNullNorEmpty()
    {
        var list = new List<string> { "foo", "bar" };
        Assert.False(list.IsNullOrEmpty());
    }

    [Fact]
    public void JoinToString()
    {
        var list = new List<string> { "foo", "bar", "baz" };

        Assert.Equal("foo-bar-baz", list.JoinToString('-'));
        Assert.Equal("foo - bar - baz", list.JoinToString(" - "));
        Assert.Equal("foo, bar, baz", list.JoinToString(", "));
    }

    [Fact]
    public void Generate_Tuple2()
    {
        var (item1, item2) = new[] { "test1", "test2" }.ToTuple2();
        Assert.Equal("test1", item1);
        Assert.Equal("test2", item2);
    }

    [Fact]
    public void Generate_Tuple2_LengthTooShort()
    {
        Assert.Throws<ArgumentException>(() => new[] { "test1" }.ToTuple2());
    }

    [Fact]
    public void Generate_Tuple2_LengthTooLong()
    {
        Assert.Throws<ArgumentException>(() => new[] { "test1", "test2", "test3" }.ToTuple2());
    }

    [Fact]
    public void Generate_Tuple3()
    {
        var (item1, item2, item3) = new[] { "test1", "test2", "test3" }.ToTuple3();
        Assert.Equal("test1", item1);
        Assert.Equal("test2", item2);
        Assert.Equal("test3", item3);
    }

    [Fact]
    public void Generate_Tuple3_LengthTooShort()
    {
        Assert.Throws<ArgumentException>(() => new[] { "test1", "test2" }.ToTuple3());
    }

    [Fact]
    public void Generate_Tuple3_LengthTooLong()
    {
        Assert.Throws<ArgumentException>(() => new[] { "test1", "test2", "test3", "test4" }.ToTuple3());
    }

    [Fact]
    public void ContainsAll_BothEmptyLists_ReturnsTrue()
    {
        var source = Enumerable.Empty<string>();
        var values = Enumerable.Empty<string>();

        var containsAll = source.ContainsAll(values);

        Assert.True(containsAll);
    }

    [Fact]
    public void ContainsAll_SourceListIsEmpty_ReturnsFalse()
    {
        var source = Enumerable.Empty<string>();
        var values = new List<string>() { "" };

        var containsAll = source.ContainsAll(values);

        Assert.False(containsAll);
    }

    [Fact]
    public void ContainsAll_ValuesListIsEmpty_ReturnsTrue()
    {
        var source = new List<string>() { "" };
        var values = Enumerable.Empty<string>();

        var containsAll = source.ContainsAll(values);

        Assert.True(containsAll);
    }

    [Fact]
    public void ContainsAll_BothNullLists_ThrowsArgumentNullException()
    {
        IEnumerable<string>? source = null;
        IEnumerable<string>? values = null;

        Assert.Throws<ArgumentNullException>(() => source!.ContainsAll(values!));
    }

    [Fact]
    public void ContainsAll_SourceListIsNull_ThrowsArgumentNullException()
    {
        IEnumerable<string>? source = null;
        var values = new List<string>() { "" };

        Assert.Throws<ArgumentNullException>(() => source!.ContainsAll(values));
    }

    [Fact]
    public void ContainsAll_ValuesListIsNull_ThrowsArgumentNullException()
    {
        var source = new List<string>() { "" };
        IEnumerable<string>? values = null;

        Assert.Throws<ArgumentNullException>(() => source.ContainsAll(values!));
    }

    [Fact]
    public void ContainsAll_SourceListDoesNotContainSingleValue_ReturnsFalse()
    {
        var source = new List<string>() { "a", "b", "c" };
        var values = new List<string>() { "d" };

        var containsAll = source.ContainsAll(values);

        Assert.False(containsAll);
    }

    [Fact]
    public void ContainsAll_SourceListContainsOneValueButNotAnother_ReturnsFalse()
    {
        var source = new List<string>() { "a", "b", "c" };
        var values = new List<string>() { "a", "d" };

        var containsAll = source.ContainsAll(values);

        Assert.False(containsAll);
    }

    [Fact]
    public void ContainsAll_SourceListContainsAllValues_ReturnsTrue()
    {
        var source = new List<string>() { "a", "b", "c" };
        var values = new List<string>() { "a", "b" };

        var containsAll = source.ContainsAll(values);

        Assert.True(containsAll);
    }

    public class CartesianTests
    {
        [Fact]
        public void TwoLists_Cartesian()
        {
            List<int> list1 = [1, 2];
            List<string> list2 = ["3", "4"];

            List<(int, string)> expected = [new(1, "3"), new(1, "4"), new(2, "3"), new(2, "4")];

            var actual = list1.Cartesian(list2);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TwoLists_EmptyList()
        {
            List<int> list1 = [1, 2];
            Assert.Empty(list1.Cartesian(new List<string>()));
        }

        [Fact]
        public void TwoLists_NullList()
        {
            List<int> list1 = [1, 2];
            Assert.Empty(list1.Cartesian((List<string>?)null));
        }

        [Fact]
        public void ThreeLists_Cartesian()
        {
            List<int> list1 = [1, 2];
            List<string> list2 = ["3", "4"];
            List<char> list3 = ['5', '6'];

            List<(int, string, char)> expected =
            [
                new(1, "3", '5'),
                new(1, "3", '6'),
                new(1, "4", '5'),
                new(1, "4", '6'),
                new(2, "3", '5'),
                new(2, "3", '6'),
                new(2, "4", '5'),
                new(2, "4", '6'),
            ];

            var actual = list1.Cartesian(list2, list3);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ThreeLists_EmptyFirstList()
        {
            List<int> list1 = [1, 2];
            List<char> list3 = ['5', '6'];
            Assert.Empty(list1.Cartesian(new List<string>(), list3));
        }

        [Fact]
        public void ThreeLists_EmptySecondList()
        {
            List<int> list1 = [1, 2];
            List<string> list2 = ["3", "4"];
            Assert.Empty(list1.Cartesian(list2, new List<char>()));
        }

        [Fact]
        public void ThreeLists_NullFirstList()
        {
            List<int> list1 = [1, 2];
            List<char> list3 = ['5', '6'];
            Assert.Empty(list1.Cartesian((List<string>?)null, list3));
        }

        [Fact]
        public void ThreeLists_NullSecondList()
        {
            List<int> list1 = [1, 2];
            List<string> list2 = ["3", "4"];
            Assert.Empty(list1.Cartesian(list2, (List<char>?)null));
        }
    }

    private static async Task<Either<Unit, int>> GetSuccessfulEither(int value)
    {
        await Task.Delay(5);
        return value;
    }

    private static async Task<Either<Unit, int>> GetFailingEither()
    {
        await Task.Delay(5);
        return Unit.Instance;
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class CollectionExtensionTests
{
    public class AddRangeTests
    {
        [Fact]
        public void Set()
        {
            var set = new HashSet<string>();

            set.AddRange(ListOf("a", "b"));

            Assert.Equal(2, set.Count);
            Assert.Contains("a", set);
            Assert.Contains("b", set);
        }

        [Fact]
        public void Set_Varargs()
        {
            var set = new HashSet<string>();

            set.AddRange("a", "b");

            Assert.Equal(2, set.Count);
            Assert.Contains("a", set);
            Assert.Contains("b", set);
        }

        [Fact]
        public void Set_Duplicates()
        {
            var set = new HashSet<string> { "a" };

            set.AddRange(ListOf("a", "b"));

            Assert.Equal(2, set.Count);
            Assert.Contains("a", set);
            Assert.Contains("b", set);
        }

        [Fact]
        public void List()
        {
            var list = new List<string>();

            list.AddRange(ListOf("a", "b"));

            Assert.Equal(2, list.Count);
            Assert.Equal("a", list[0]);
            Assert.Equal("b", list[1]);
        }

        [Fact]
        public void List_Duplicates()
        {
            var list = new List<string> { "a" };

            list.AddRange(ListOf("a", "b"));

            Assert.Equal(3, list.Count);
            Assert.Equal("a", list[0]);
            Assert.Equal("a", list[1]);
            Assert.Equal("b", list[2]);
        }

        [Fact]
        public void List_Varargs()
        {
            var list = new List<string> { "a" };

            list.AddRange("a", "b");

            Assert.Equal(3, list.Count);
            Assert.Equal("a", list[0]);
            Assert.Equal("a", list[1]);
            Assert.Equal("b", list[2]);
        }
    }

    public class LastIndexTests
    {
        [Theory]
        [InlineData(0, "a")]
        [InlineData(1, "a", "b")]
        [InlineData(2, "a", "b", "c")]
        [InlineData(3, "a", "b", "c", "d")]
        public void CorrectLastIndex(int expectedLastIndex, params string[] values)
        {
            var list = new List<string>(values);

            Assert.Equal(expectedLastIndex, list.LastIndex());
        }
    }

    public class IsLastIndexTests
    {
        [Theory]
        [InlineData(0, "a")]
        [InlineData(1, "a", "b")]
        [InlineData(2, "a", "b", "c")]
        [InlineData(3, "a", "b", "c", "d")]
        public void TrueForCorrectLastIndex(int correctIndex, params string[] values)
        {
            var list = new List<string>(values);

            Assert.True(list.IsLastIndex(correctIndex));
        }

        [Theory]
        [InlineData(1, "a")]
        [InlineData(2, "a", "b")]
        [InlineData(3, "a", "b", "c")]
        [InlineData(4, "a", "b", "c", "d")]
        public void FalseForIncorrectLastIndex(int incorrectIndex, params string[] values)
        {
            var list = new List<string>(values);

            Assert.False(list.IsLastIndex(incorrectIndex));
        }
    }
}

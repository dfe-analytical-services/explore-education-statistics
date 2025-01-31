using System.Collections.Generic;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.ComparerUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public class ComparerUtilsTests
    {
        [Fact]
        public void CreateComparerByProperty()
        {
            var comparer = CreateComparerByProperty<TestClass>(x=> x.Value);

            Assert.True(comparer.Equals(new TestClass(1), new TestClass(1)));
            Assert.True(comparer.Equals(null, null));

            Assert.False(comparer.Equals(new TestClass(1), new TestClass(2)));
            Assert.False(comparer.Equals(null, new TestClass(2)));
            Assert.False(comparer.Equals(new TestClass(1), null));
            
            Assert.Equal(comparer.GetHashCode(new TestClass(1)), 1.GetHashCode());
        }

        [Fact]
        public void SequencesAreEqualIgnoringOrder_ListsAreEmpty()
        {
            Assert.True(SequencesAreEqualIgnoringOrder(new List<string>(), new List<string>()));
        }

        [Fact]
        public void SequencesAreEqualIgnoringOrder_ListsAreSame()
        {
            Assert.True(SequencesAreEqualIgnoringOrder(ListOf("a", "b"), ListOf("a", "b")));
        }

        [Fact]
        public void SequencesAreEqualIgnoringOrder_ListsAreSameIgnoringOrder()
        {
            Assert.True(SequencesAreEqualIgnoringOrder(ListOf("a", "b"), ListOf("b", "a")));
        }

        [Fact]
        public void SequencesAreEqualIgnoringOrder_FirstHasElementNotInSecond()
        {
            var first = ListOf("a", "b", "c");
            var second = ListOf("b", "a");

            Assert.False(SequencesAreEqualIgnoringOrder(first, second));
        }

        [Fact]
        public void SequencesAreEqualIgnoringOrder_SecondHasElementNotInFirst()
        {
            var first = ListOf("a", "b");
            var second = ListOf("c", "b", "a");

            Assert.False(SequencesAreEqualIgnoringOrder(first, second));
        }

        private class TestClass
        {
            public readonly int Value;

            public TestClass(int value)
            {
                Value = value;
            }
        }
    }
}

using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public class ComparerUtilsTests
    {
        [Fact]
        public void CreateComparerByProperty()
        {
            var comparer = ComparerUtils.CreateComparerByProperty<TestClass>(x=> x.Value);

            Assert.True(comparer.Equals(new TestClass(1), new TestClass(1)));
            Assert.True(comparer.Equals(null, null));

            Assert.False(comparer.Equals(new TestClass(1), new TestClass(2)));
            Assert.False(comparer.Equals(null, new TestClass(2)));
            Assert.False(comparer.Equals(new TestClass(1), null));
            
            Assert.Equal(comparer.GetHashCode(new TestClass(1)), 1.GetHashCode());
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
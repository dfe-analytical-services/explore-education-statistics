using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void DistinctByProperty()
        {
            var list = new List<TestClass>
            {
                new TestClass(1),
                new TestClass(1),
                new TestClass(2),
            };

            var distinct = list.DistinctByProperty(x => x.Value).ToList();

            Assert.Equal(2, distinct.Count);
            Assert.Equal(1, distinct[0].Value);
            Assert.Equal(2, distinct[1].Value);
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
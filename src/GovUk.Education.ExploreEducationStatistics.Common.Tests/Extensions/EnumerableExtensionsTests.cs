using System.Collections.Generic;
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

            var distinct = list.DistinctByProperty(x => x.Value);
            
            Assert.Equal(new List<TestClass>
            {
                new TestClass(1),
                new TestClass(2),
            }, distinct);
        }

        private class TestClass
        {
            public readonly int Value;

            public TestClass(int value)
            {
                Value = value;
            }

            protected bool Equals(TestClass other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TestClass) obj);
            }

            public override int GetHashCode()
            {
                return Value;
            }
        }
    }
}
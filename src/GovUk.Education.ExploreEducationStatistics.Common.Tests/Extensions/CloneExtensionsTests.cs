#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class CloneExtensionsTests
{
    public class ShallowCloneTests
    {
        [Theory]
        [InlineData(123)]
        [InlineData(1.1)]
        [InlineData(true)]
        [InlineData(false)]
        public void NonStringPrimitives(object value)
        {
            Assert.Equal(value, value.ShallowClone());
        }

        [Fact]
        public void String()
        {
            const string value = "test";

            // Need to be able to specify the type is
            // a string to shallow clone it correctly.
            Assert.Equal(value, value.ShallowClone());
        }

        [Fact]
        public void Object()
        {
            var obj = new TestPerson
            {
                Name = "Test person",
                Age = 123,
                Addresses =
                [
                    new TestAddress { Line1 = "123 High Street" },
                    new TestAddress { Line1 = "456 Low Street" },
                ],
            };

            var clone = obj.ShallowClone();

            Assert.Equal(obj.Name, clone.Name);
            Assert.Equal(obj.Age, clone.Age);

            Assert.Same(obj.Addresses, clone.Addresses);
            Assert.Same(obj.Addresses[0], clone.Addresses[0]);
            Assert.Same(obj.Addresses[1], clone.Addresses[1]);
        }
    }

    public class DeepCloneTests
    {
        [Theory]
        [InlineData(123)]
        [InlineData(1.1)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData("test")]
        public void Primitives(object value)
        {
            Assert.Equal(value, value.DeepClone());
        }

        [Fact]
        public void Object()
        {
            var obj = new TestPerson
            {
                Name = "Test person",
                Age = 123,
                Addresses =
                [
                    new TestAddress { Line1 = "123 High Street" },
                    new TestAddress { Line1 = "456 Low Street" },
                ],
            };

            var clone = obj.DeepClone();

            Assert.Equal(obj.Name, clone.Name);
            Assert.Equal(obj.Age, clone.Age);

            Assert.NotSame(obj.Addresses, clone.Addresses);
            Assert.Equal(obj.Addresses.Count, clone.Addresses.Count);

            Assert.NotSame(obj.Addresses[0], clone.Addresses[0]);
            Assert.Equal(obj.Addresses[0].Line1, clone.Addresses[0].Line1);

            Assert.NotSame(obj.Addresses[1], clone.Addresses[1]);
            Assert.Equal(obj.Addresses[1].Line1, clone.Addresses[1].Line1);
        }
    }

    private class TestPerson
    {
        public required string Name { get; init; }

        public required int Age { get; init; }

        public List<TestAddress> Addresses { get; init; } = [];
    }

    private class TestAddress
    {
        public required string Line1 { get; init; }
    }
}

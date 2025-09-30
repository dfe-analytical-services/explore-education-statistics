using Xunit;
using Xunit.Sdk;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions.AssertExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

// ReSharper disable once ClassNeverInstantiated.Global
public class AssertExtensionsTests
{
    public class AssertDeepEqualsTo
    {
        // ReSharper disable twice NotAccessedPositionalProperty.Local
        private record Company(string Name, List<Person> Employees);

        // ReSharper disable once NotAccessedPositionalProperty.Local
        private record Person(string Name, int Age);

        [Fact]
        public void Simple()
        {
            var person1 = new Person("Name", 30);
            var person2 = new Person("Name", 30);
            person1.AssertDeepEqualTo(person2);
        }

        [Fact]
        public void Simple_NotEqual()
        {
            var person1 = new Person("Name", 30);
            var person2 = new Person("Different", 30);
            Assert.Throws<TrueException>(() => person1.AssertDeepEqualTo(person2));
        }

        [Fact]
        public void Nested()
        {
            var company1 = new Company("Company", ListOf(
                new Person("Name1", 30),
                new Person("Name2", 31)));

            var company2 = new Company("Company", ListOf(
                new Person("Name1", 30),
                new Person("Name2", 31)));

            company1.AssertDeepEqualTo(company2);
        }

        [Fact]
        public void Nested_NotEqual()
        {
            var company1 = new Company("Company", ListOf(
                new Person("Name1", 30),
                new Person("Name2", 31)));

            var company2 = new Company("Company", ListOf(
                new Person("Name1", 30),
                new Person("DifferentName", 31)));

            Assert.Throws<TrueException>(() => company1.AssertDeepEqualTo(company2));
        }

        [Fact]
        public void IgnoreProperty()
        {
            var person1 = new Person("Name", 30);
            var person2 = new Person("Different", 30);

            // Assert that all fields in Person are expected to be equal apart from the "Name" field, which is
            // expected to be unequal.
            person1.AssertDeepEqualTo(person2, ignoreProperties: [person => person.Name]);
        }
    }

    public class AssertUtcNow
    {
        [Fact]
        public void DateTimesWithinTolerance()
        {
            DateTime.UtcNow.AssertUtcNow();
            DateTime.UtcNow.AddMilliseconds(-400).AssertUtcNow();
        }

        [Fact]
        public void DateTimesOutsideTolerance()
        {
            Assert.Throws<EqualException>(() => DateTime.UtcNow.AddMilliseconds(-1 - TimeWithinMillis).AssertUtcNow());
            Assert.Throws<EqualException>(() => DateTime.UtcNow.AddMilliseconds(-101).AssertUtcNow(withinMillis: 100));
        }

        [Fact]
        public void NullableDateTime()
        {
            DateTime? nullableDateTime = DateTime.UtcNow;
            nullableDateTime.AssertUtcNow();
        }

        [Fact]
        public void NullableDateTime_Null()
        {
            Assert.Throws<NotNullException>(() => ((DateTime?)null).AssertUtcNow());
        }
    }
}

#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Newtonsoft.Json;
using Snapshooter.Xunit;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public abstract class JsonSerializationUtilsTests
{
    private record TestClass
    {
        public string Field2 { get; init; } = null!;

        public string Field1 { get; init; } = null!;

        public string Field3 { get; init; } = null!;

        public TestClass? Child { get; init; }
    }

    public class DefaultSettingsTests : JsonSerializationUtilsTests
    {
        [Fact]
        public void Serialize()
        {
            var obj = new TestClass
            {
                Field2 = "Value2",
                Field1 = "Value1",
                Field3 = "Value3",
                Child = new TestClass
                {
                    Field2 = "ChildValue2",
                    Field1 = "ChildValue1",
                    Field3 = "ChildValue3",
                }
            };

            var serialized = JsonSerializationUtils.Serialize(obj: obj);

            Snapshot.Match(serialized);
        }
    }

    public class IndentsTests : JsonSerializationUtilsTests
    {
        [Fact]
        public void Serialize()
        {
            var obj = new TestClass
            {
                Field2 = "Value2",
                Field1 = "Value1",
                Field3 = "Value3",
                Child = new TestClass
                {
                    Field2 = "ChildValue2",
                    Field1 = "ChildValue1",
                    Field3 = "ChildValue3",
                }
            };

            var serialized = JsonSerializationUtils.Serialize(
                obj: obj,
                formatting: Formatting.Indented);

            Snapshot.Match(serialized);
        }
    }

    public class OrderedPropertiesTests : JsonSerializationUtilsTests
    {
        [Fact]
        public void Serialize()
        {
            var obj = new TestClass
            {
                Field2 = "Value2",
                Field1 = "Value1",
                Field3 = "Value3",
                Child = new TestClass
                {
                    Field2 = "ChildValue2",
                    Field1 = "ChildValue1",
                    Field3 = "ChildValue3",
                }
            };

            var serialized = JsonSerializationUtils.Serialize(
                obj: obj,
                formatting: Formatting.Indented,
                orderedProperties: true);

            Snapshot.Match(serialized);
        }
    }

    public class CamelCaseTests : JsonSerializationUtilsTests
    {
        [Fact]
        public void Serialize()
        {
            var obj = new TestClass
            {
                Field2 = "Value2",
                Field1 = "Value1",
                Field3 = "Value3",
                Child = new TestClass
                {
                    Field2 = "ChildValue2",
                    Field1 = "ChildValue1",
                    Field3 = "ChildValue3",
                }
            };

            var serialized = JsonSerializationUtils.Serialize(
                obj: obj,
                formatting: Formatting.Indented,
                camelCase: true);

            Snapshot.Match(serialized);
        }
    }

    public class CamelCaseOrderedTests : JsonSerializationUtilsTests
    {
        [Fact]
        public void Serialize()
        {
            var obj = new TestClass
            {
                Field2 = "Value2",
                Field1 = "Value1",
                Field3 = "Value3",
                Child = new TestClass
                {
                    Field2 = "ChildValue2",
                    Field1 = "ChildValue1",
                    Field3 = "ChildValue3",
                }
            };

            var serialized = JsonSerializationUtils.Serialize(
                obj: obj,
                formatting: Formatting.Indented,
                camelCase: true,
                orderedProperties: true);

            Snapshot.Match(serialized);
        }
    }
}

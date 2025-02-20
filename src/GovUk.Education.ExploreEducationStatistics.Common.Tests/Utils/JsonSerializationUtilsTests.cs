#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Newtonsoft.Json;
using Snapshooter.Xunit;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public abstract class JsonSerializationUtilsTests
{
    public class SerializeWithOrderedProperties : JsonSerializationUtilsTests
    {
        private record TestClass
        {
            public string Field2 { get; init; } = null!;

            public string Field1 { get; init; } = null!;

            public string Field3 { get; init; } = null!;

            public TestClass? Child { get; init; }
        }
        
        [Fact]
        public void Serialize_WithIndents()
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
            
            var serialized = JsonSerializationUtils
                .SerializeWithOrderedProperties(obj, Formatting.Indented);
            
            Snapshot.Match(serialized);
        }
        
        [Fact]
        public void Serialize_NoIndents()
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
            
            var serialized = JsonSerializationUtils
                .SerializeWithOrderedProperties(obj, Formatting.None);
            
            Snapshot.Match(serialized);
        }
    }
}

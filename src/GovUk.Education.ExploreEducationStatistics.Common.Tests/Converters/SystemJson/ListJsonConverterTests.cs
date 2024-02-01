using GovUk.Education.ExploreEducationStatistics.Common.Database;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using AngleSharp.Text;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters.SystemJson;

public class ListJsonConverterTests
{
    private abstract class SampleClass<T>
    {
        public abstract IReadOnlyList<T> SampleField { get; set; }

        protected bool Equals(SampleClass<T> other)
        {
            return Enumerable.SequenceEqual(SampleField, other.SampleField);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SampleClass<T>)obj);
        }

        public override int GetHashCode()
        {
            return SampleField.Sum(s => s.GetHashCode() * 17);
        }
    }

    private class SampleObject
    {
        public string Name { get; }

        public SampleObject(string name)
        {
            Name = name;
        }

        protected bool Equals(SampleObject other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SampleObject)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    private class SampleObjectJsonConverter : JsonConverter<SampleObject>
    {
        public override SampleObject Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            reader.Read();
            var name = reader.GetString();

            reader.Read();

            return new SampleObject(name);
        }

        public override void Write(
            Utf8JsonWriter writer,
            SampleObject value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Name);
        }
    }

    private enum SampleEnum
    {
        SampleValue1,
        SampleValue2
    }

    private class ClassWithObjectConverter : SampleClass<SampleObject>
    {
        [JsonConverter(typeof(ListJsonConverter<SampleObject, SampleObjectJsonConverter>))]
        public override IReadOnlyList<SampleObject> SampleField { get; set; }
    }

    private class ClassWithEnumConverter : SampleClass<SampleEnum>
    {
        [JsonConverter(typeof(ListJsonConverter<SampleEnum, JsonStringEnumConverter>))]
        public override IReadOnlyList<SampleEnum> SampleField { get; set; }
    }

    [Fact]
    public void WithObject_SerializeObject()
    {
        var objectToSerialize = new ClassWithObjectConverter
        {
            SampleField = new List<SampleObject>
            {
                new("SampleValue1"),
                new("SampleValue2")
            }
        };

        var expectedJson = new string(
            @"{
                ""SampleField"" : [
                    ""SampleValue1"",
                    ""SampleValue2""
                ]
            }"
            .Where(c => !c.IsWhiteSpaceCharacter())
            .ToArray());

        Assert.Equal(expectedJson, JsonSerializer.Serialize(objectToSerialize));
    }

    [Fact]
    public void WithObject_DeserializeObject()
    {
        const string jsonText = 
            @"{
                ""SampleField"" : [
                    {
                        ""Name"": ""SampleValue1""
                    },
                    {
                        ""Name"": ""SampleValue2""
                    }
                ]
            }";

        var expected = new ClassWithObjectConverter
        {
            SampleField = new List<SampleObject>
            {
                new("SampleValue1"),
                new("SampleValue2")
            }
        };

        Assert.Equal(expected, JsonSerializer.Deserialize<ClassWithObjectConverter>(jsonText));
    }

    [Fact]
    public void WithEnum_SerializeObject()
    {
        var objectToSerialize = new ClassWithEnumConverter
        {
            SampleField = new List<SampleEnum>
            {
                SampleEnum.SampleValue1,
                SampleEnum.SampleValue2
            }
        };

        var expectedJson = new string(
            @"{
                ""SampleField"" : [
                    ""SampleValue1"",
                    ""SampleValue2""
                ]
            }"
            .Where(c => !c.IsWhiteSpaceCharacter())
            .ToArray());

        Assert.Equal(expectedJson, JsonSerializer.Serialize(objectToSerialize));
    }

    [Fact]
    public void WithEnum_DeserializeObject()
    {
        const string jsonText =
            @"{
                ""SampleField"" : [
                    ""SampleValue1"",
                    ""SampleValue2""
                ]
            }";

        var expected = new ClassWithEnumConverter
        {
            SampleField = new List<SampleEnum>
            {
                SampleEnum.SampleValue1,
                SampleEnum.SampleValue2
            }
        };

        Assert.Equal(expected, JsonSerializer.Deserialize<ClassWithEnumConverter>(jsonText));
    }
}

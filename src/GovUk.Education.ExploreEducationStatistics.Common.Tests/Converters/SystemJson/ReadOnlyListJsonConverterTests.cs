#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using AngleSharp.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters.SystemJson;

public class ReadOnlyListJsonConverterTests
{
    private abstract class SampleClass<T>
    {
        public abstract IReadOnlyList<T>? SampleField { get; set; }
    }

    private record SampleObject(string Name);

    private enum SampleEnum
    {
        SampleValue1,
        SampleValue2
    }

    private class ClassWithObjectConverter : SampleClass<SampleObject>
    {
        [JsonConverter(typeof(ReadOnlyListJsonConverter<SampleObject, SampleObjectJsonConverter>))]
        public override IReadOnlyList<SampleObject>? SampleField { get; set; }
    }

    private class ClassWithEnumConverter : SampleClass<SampleEnum>
    {
        [JsonConverter(typeof(ReadOnlyListJsonConverter<SampleEnum, JsonStringEnumConverter>))]
        public override IReadOnlyList<SampleEnum>? SampleField { get; set; }
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
            var name = reader.GetString()!;

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

    [Fact]
    public void WithObjectConverter_SerializeObject()
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
    public void WithObjectConverter_SerializeObject_ListNull()
    {
        var objectToSerialize = new ClassWithObjectConverter
        {
            SampleField = null
        };

        var expectedJson = new string(
            @"{
                ""SampleField"" : null
            }"
                .Where(c => !c.IsWhiteSpaceCharacter())
                .ToArray());

        Assert.Equal(expectedJson, JsonSerializer.Serialize(objectToSerialize));
    }

    [Fact]
    public void WithObjectConverter_SerializeObject_ListEmpty()
    {
        var objectToSerialize = new ClassWithObjectConverter
        {
            SampleField = new List<SampleObject>()
        };

        var expectedJson = new string(
            @"{
                ""SampleField"" : []
            }"
                .Where(c => !c.IsWhiteSpaceCharacter())
                .ToArray());

        Assert.Equal(expectedJson, JsonSerializer.Serialize(objectToSerialize));
    }

    [Fact]
    public void WithObjectConverter_DeserializeObject()
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

        JsonSerializer.Deserialize<ClassWithObjectConverter>(jsonText).AssertDeepEqualTo(expected);
    }

    [Fact]
    public void WithObjectConverter_DeserializeObject_ListEmpty()
    {
        var expected = new ClassWithObjectConverter
        {
            SampleField = new List<SampleObject>()
        };

        const string jsonText =
            @"{
                ""SampleField"" : []
            }";

        JsonSerializer.Deserialize<ClassWithObjectConverter>(jsonText).AssertDeepEqualTo(expected);
    }

    [Fact]
    public void WithObjectConverter_DeserializeObject_ListNull()
    {
        var expected = new ClassWithObjectConverter
        {
            SampleField = null
        };

        const string jsonText =
            @"{
                ""SampleField"" : null
            }";

        JsonSerializer.Deserialize<ClassWithObjectConverter>(jsonText).AssertDeepEqualTo(expected);
    }

    [Fact]
    public void WithObjectConverter_DeserializeObject_ListMissingStartingBracket_Throws()
    {
        const string jsonText =
            @"{
                ""SampleField"" : 
                    {
                        ""Name"": ""SampleValue1""
                    },
                    {
                        ""Name"": ""SampleValue2""
                    }
                ]
            }";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClassWithObjectConverter>(jsonText));
    }

    [Fact]
    public void WithObjectConverter_DeserializeObject_ListMissingClosingBracket_Throws()
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
                
            }";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClassWithObjectConverter>(jsonText));
    }

    [Fact]
    public void WithEnumConverter_SerializeObject()
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
    public void WithEnumConverter_SerializeObject_ListNull()
    {
        var objectToSerialize = new ClassWithEnumConverter
        {
            SampleField = null
        };

        var expectedJson = new string(
            @"{
                ""SampleField"" : null
            }"
                .Where(c => !c.IsWhiteSpaceCharacter())
                .ToArray());

        Assert.Equal(expectedJson, JsonSerializer.Serialize(objectToSerialize));
    }

    [Fact]
    public void WithEnumConverter_SerializeObject_ListEmpty()
    {
        var objectToSerialize = new ClassWithEnumConverter
        {
            SampleField = new List<SampleEnum>()
        };

        var expectedJson = new string(
            @"{
                ""SampleField"" : []
            }"
                .Where(c => !c.IsWhiteSpaceCharacter())
                .ToArray());

        Assert.Equal(expectedJson, JsonSerializer.Serialize(objectToSerialize));
    }

    [Fact]
    public void WithEnumConverter_DeserializeObject()
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

        JsonSerializer.Deserialize<ClassWithEnumConverter>(jsonText).AssertDeepEqualTo(expected);
    }

    [Fact]
    public void WithEnumConverter_DeserializeObject_ListEmpty()
    {
        var expected = new ClassWithEnumConverter
        {
            SampleField = new List<SampleEnum>()
        };

        const string jsonText =
            @"{
                ""SampleField"" : []
            }";

        JsonSerializer.Deserialize<ClassWithEnumConverter>(jsonText).AssertDeepEqualTo(expected);
    }

    [Fact]
    public void WithEnumConverter_DeserializeObject_ListNull()
    {
        var expected = new ClassWithEnumConverter
        {
            SampleField = null
        };

        const string jsonText =
            @"{
                ""SampleField"" : null
            }";

        JsonSerializer.Deserialize<ClassWithEnumConverter>(jsonText).AssertDeepEqualTo(expected);
    }

    [Fact]
    public void WithEnumConverter_DeserializeObject_ListMissingStartingBracket_Throws()
    {
        const string jsonText =
            @"{
                ""SampleField"" : 
                    ""SampleValue1"",
                    ""SampleValue2""
                ]
            }";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClassWithEnumConverter>(jsonText));
    }

    [Fact]
    public void WithEnumConverter_DeserializeObject_ListMissingClosingBracket_Throws()
    {
        const string jsonText =
            @"{
                ""SampleField"" : [
                    ""SampleValue1"",
                    ""SampleValue2""
            }";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClassWithEnumConverter>(jsonText));
    }
}

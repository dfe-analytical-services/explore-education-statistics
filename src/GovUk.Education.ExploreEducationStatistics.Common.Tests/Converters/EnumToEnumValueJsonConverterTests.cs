#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Newtonsoft.Json;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Converters;

public class EnumToEnumValueJsonConverterTests
{
    private enum SampleEnum
    {
        [EnumLabelValue("SampleLabel", "SampleValue")]
        Sample,
    }

    private record SampleClass
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local - It's being used by the serialiser
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<SampleEnum>))]
        public SampleEnum SampleField { get; init; }
    }

    [Fact]
    public void SerializeObject()
    {
        var objectToSerialize = new SampleClass { SampleField = SampleEnum.Sample };

        Assert.Equal(
            "{\"SampleField\":\"SampleValue\"}",
            JsonConvert.SerializeObject(objectToSerialize)
        );
    }

    [Fact]
    public void DeserializeObject()
    {
        const string jsonText = "{\"SampleField\":\"SampleValue\"}";

        var expected = new SampleClass { SampleField = SampleEnum.Sample };

        Assert.Equal(expected, JsonConvert.DeserializeObject<SampleClass>(jsonText));
    }
}

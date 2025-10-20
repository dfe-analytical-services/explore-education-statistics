using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters;

public class ContentBlockChartConverter : JsonConverter<IChart>
{
    public override bool CanWrite => false;

    public override IChart ReadJson(
        JsonReader reader,
        Type objectType,
        IChart? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["Type"] ?? jsonObject["type"] ?? throw new JsonSerializationException("Type not found");
        var chartType = EnumUtil.GetFromEnumValue<ChartType>(type.Value<string>()!);

        IChart contentBlock = chartType switch
        {
            ChartType.Line => new LineChart(),
            ChartType.HorizontalBar => new HorizontalBarChart(),
            ChartType.VerticalBar => new VerticalBarChart(),
            ChartType.Map => new MapChart(),
            ChartType.Infographic => new InfographicChart(),
            _ => throw new ArgumentOutOfRangeException(),
        };

        serializer.Populate(jsonObject.CreateReader(), contentBlock);
        return contentBlock;
    }

    public override void WriteJson(JsonWriter writer, IChart? value, JsonSerializer serializer)
    {
        throw new InvalidOperationException("Use default serialization.");
    }
}

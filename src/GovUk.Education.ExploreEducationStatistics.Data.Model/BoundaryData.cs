#nullable enable
using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class BoundaryData
{
    public int Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public string GeoJson { get; set; }

    public Feature Feature
        => JsonConvert.DeserializeObject<Feature>(
            GeoJson,
            new JsonSerializerSettings { CheckAdditionalContent = false });

    public BoundaryLevel BoundaryLevel { get; set; }
}

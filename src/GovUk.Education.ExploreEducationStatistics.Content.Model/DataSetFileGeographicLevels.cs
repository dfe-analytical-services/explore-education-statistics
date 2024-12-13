#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileGeographicLevel
{
    public Guid DataSetFileVersionId { get; set; } // Currently Files.Id, but will become DataSetFileVersion.Id in EES-5105

    public File DataSetFileVersion { get; set; } = null!;

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public GeographicLevel GeographicLevel { get; set; }
}

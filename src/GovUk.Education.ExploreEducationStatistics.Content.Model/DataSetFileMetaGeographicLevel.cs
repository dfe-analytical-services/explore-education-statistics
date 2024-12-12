#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileMetaGeographicLevel
{
    public Guid DataSetFileId;

    [JsonConverter(typeof(EnumToEnumValueConverter<GeographicLevel>))]
    public GeographicLevel GeographicLevel;
}

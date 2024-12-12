#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileMetaTimePeriod
{
    public Guid DataSetFileId;

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public TimeIdentifier StartTimeIdentifier { get; set; }

    public string StartPeriod { get; set; } = "";

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public TimeIdentifier EndTimeIdentifier { get; set; }

    public string EndPeriod { get; set; } = "";
}

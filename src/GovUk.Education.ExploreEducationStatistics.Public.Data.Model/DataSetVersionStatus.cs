using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
public enum DataSetVersionStatus
{
    Processing,
    Failed,
    Mapping,
    Draft,
    Published,
    Deprecated,
    Withdrawn,
    Cancelled,
}

public class DataSetVersionStatusConstants
{
    public static readonly IReadOnlyList<DataSetVersionStatus> PublicStatuses = new List<DataSetVersionStatus>(
        [
            DataSetVersionStatus.Published,
            DataSetVersionStatus.Withdrawn,
            DataSetVersionStatus.Deprecated
        ]
    );
    
    public static readonly IReadOnlyList<DataSetVersionStatus> PrivateStatuses = EnumUtil
        .GetEnums<DataSetVersionStatus>()
        .Except(PublicStatuses)
        .ToList();
}

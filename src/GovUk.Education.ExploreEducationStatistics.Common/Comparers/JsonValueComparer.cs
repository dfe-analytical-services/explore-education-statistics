using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Comparers;

/// <summary>
/// Compares and snapshots a JSON-converted property by its serialized value, so that EF only detects a change when
/// the JSON stored in the column would change.
/// </summary>
public class JsonValueComparer<T>()
    : ValueComparer<T?>(
        (left, right) => JsonConvert.SerializeObject(left) == JsonConvert.SerializeObject(right),
        value => JsonConvert.SerializeObject(value).GetHashCode(),
        value => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value))
    );

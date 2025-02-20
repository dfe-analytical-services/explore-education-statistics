using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class JsonSerializationUtils
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        ContractResolver = new OrderedContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateFormatString = "yyyy-MM-ddThh:mm:ss.fffZ"
    };
    
    public static string SerializeWithOrderedProperties(object obj, Formatting formatting)
    {
        return JsonConvert.SerializeObject(
            value: obj,
            formatting: formatting,
            settings: JsonSerializerSettings);
    }
    
    private class OrderedContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                .OrderBy(p => p.Order ?? int.MaxValue)  // Honour any explit ordering first
                .ThenBy(p => p.PropertyName)
                .ToList();
        }
    }
}

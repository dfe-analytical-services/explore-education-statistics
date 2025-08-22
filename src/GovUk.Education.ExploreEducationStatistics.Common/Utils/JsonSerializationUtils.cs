using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class JsonSerializationUtils
{
    public static string Serialize(
        object obj,
        Formatting formatting = Formatting.None,
        bool camelCase = false,
        bool orderedProperties = false)
    {
        var contractResolver = GetContractResolver(camelCase, orderedProperties);

        var settings = new JsonSerializerSettings
        {
            ContractResolver = contractResolver,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
        };
        settings.Converters.Add(new StringEnumConverter());
        
        return JsonConvert.SerializeObject(
            value: obj,
            formatting: formatting,
            settings: settings);
    }

    private static IContractResolver GetContractResolver(bool camelCase, bool orderedProperties)
    {
        if (camelCase && orderedProperties)
        {
            return new OrderedFieldsCamelCaseContractResolver();
        }

        if (camelCase)
        {
            return new CamelCasePropertyNamesContractResolver();
        }

        if (orderedProperties)
        {
            return new OrderedFieldsContractResolver();
        }

        return new DefaultContractResolver();
    }

    private class OrderedFieldsCamelCaseContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            // Honour any explicit ordering first, and then alphabetically.
            return base.CreateProperties(type, memberSerialization)
                .OrderBy(p => p.Order ?? int.MaxValue)
                .ThenBy(p => p.PropertyName)
                .ToList();
        }
    }
    
    private class OrderedFieldsContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            // Honour any explicit ordering first, and then alphabetically.
            return base.CreateProperties(type, memberSerialization)
                .OrderBy(p => p.Order ?? int.MaxValue)
                .ThenBy(p => p.PropertyName)
                .ToList();
        }
    }
}

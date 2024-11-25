#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters;

public class EnumToEnumValueConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : Enum
{
    private static readonly Dictionary<string, TEnum> Lookup =
        EnumUtil.GetEnums<TEnum>().ToDictionary(value => value.GetEnumValue());

    public EnumToEnumValueConverter(
        ConverterMappingHints? mappingHints = null) :
        base(value => ToProvider(value),
            value => FromProvider(value),
            mappingHints)
    {
    }

    public static string ToProvider(TEnum value)
    {
        return value.GetEnumValue();
    }

    public static TEnum FromProvider(string value)
    {
        if (Lookup.TryGetValue(value, out var enumVal))
        {
            return enumVal;
        }

        throw new ArgumentOutOfRangeException($"No enum value found for {value}");
    }

    public static string EnumValueListToJsonStr(List<TEnum> list)
    {
        var strList = list
            .Select(ToProvider)
            .Select(str => $"\"{str}\"")
            .ToList();

        return $"{strList.JoinToString(",")}";
        //return JsonConvert.SerializeObject(strList);
    }

    public static List<TEnum> JsonStrToEnumList(string jsonArray)
    {
        var strList = JsonConvert.DeserializeObject<List<string>>(jsonArray);

        return strList
            .Select(FromProvider)
            .ToList();
    }
}

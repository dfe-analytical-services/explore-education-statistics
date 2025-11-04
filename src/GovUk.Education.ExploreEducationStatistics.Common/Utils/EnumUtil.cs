using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class EnumUtil
{
    private static class EnumCache<TEnum>
        where TEnum : Enum
    {
        public static readonly Lazy<Dictionary<string, TEnum>> Values = new(() =>
            GetEnums<TEnum>().ToDictionary(e => e.GetEnumValue())
        );

        public static readonly Lazy<IReadOnlyList<string>> ValuesList = new(() =>
            GetEnumValuesUncached<TEnum>().ToList()
        );

        public static readonly Lazy<IReadOnlySet<string>> ValuesSet = new(() =>
            GetEnumValuesUncached<TEnum>().ToHashSet()
        );

        public static readonly Lazy<Dictionary<string, TEnum>> Labels = new(() =>
            GetEnums<TEnum>().ToDictionary(e => e.GetEnumLabel())
        );

        public static readonly Lazy<IReadOnlyList<string>> LabelsList = new(() =>
            GetEnumLabelsUncached<TEnum>().ToList()
        );

        public static readonly Lazy<IReadOnlySet<string>> LabelsSet = new(() =>
            GetEnumLabelsUncached<TEnum>().ToHashSet()
        );
    }

    public static TEnum GetFromEnumValue<TEnum>(string value)
        where TEnum : Enum
    {
        if (TryGetFromEnumValue<TEnum>(value, out var enumValue))
        {
            return enumValue;
        }

        throw new ArgumentOutOfRangeException(
            paramName: nameof(value),
            message: $"The value '{value}' is not a valid {typeof(TEnum).Name}"
        );
    }

    public static bool TryGetFromEnumValue<TEnum>(string value, [MaybeNullWhen(false)] out TEnum @enum)
        where TEnum : Enum
    {
        if (EnumCache<TEnum>.Values.Value.TryGetValue(value, out var enumValue))
        {
            @enum = enumValue;
            return true;
        }

        @enum = default;
        return false;
    }

    public static TEnum GetFromEnumLabel<TEnum>(string label)
        where TEnum : Enum
    {
        if (EnumCache<TEnum>.Labels.Value.TryGetValue(label, out var enumValue))
        {
            return enumValue;
        }

        throw new ArgumentOutOfRangeException(
            paramName: nameof(label),
            message: $"The label '{label}' is not a valid {typeof(TEnum).Name}"
        );
    }

    public static bool TryGetFromEnumLabel<TEnum>(string value, [MaybeNullWhen(false)] out TEnum @enum)
        where TEnum : Enum
    {
        if (EnumCache<TEnum>.Labels.Value.TryGetValue(value, out var enumValue))
        {
            @enum = enumValue;
            return true;
        }

        @enum = default;
        return false;
    }

    public static List<TEnum> GetEnums<TEnum>()
        where TEnum : Enum
    {
        return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
    }

    public static TEnum[] GetEnumsArray<TEnum>()
        where TEnum : Enum
    {
        return GetEnums<TEnum>().ToArray();
    }

    public static IReadOnlyList<string> GetEnumValues<TEnum>()
        where TEnum : Enum => EnumCache<TEnum>.ValuesList.Value;

    public static IReadOnlySet<string> GetEnumValuesSet<TEnum>()
        where TEnum : Enum => EnumCache<TEnum>.ValuesSet.Value;

    public static IReadOnlyList<string> GetEnumLabels<TEnum>()
        where TEnum : Enum => EnumCache<TEnum>.LabelsList.Value;

    public static IReadOnlySet<string> GetEnumLabelsSet<TEnum>()
        where TEnum : Enum => EnumCache<TEnum>.LabelsSet.Value;

    private static IEnumerable<string> GetEnumValuesUncached<TEnum>()
        where TEnum : Enum => GetEnums<TEnum>().Select(e => e.GetEnumValue());

    private static IEnumerable<string> GetEnumLabelsUncached<TEnum>()
        where TEnum : Enum => GetEnums<TEnum>().Select(e => e.GetEnumLabel());
}

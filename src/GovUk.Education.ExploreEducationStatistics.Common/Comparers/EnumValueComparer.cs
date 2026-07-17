using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GovUk.Education.ExploreEducationStatistics.Common.Comparers;

public class EnumValueComparer<TEnum>() : ValueComparer<TEnum>((a, b) => a.Equals(b), v => v.GetHashCode(), v => v)
    where TEnum : struct, Enum;

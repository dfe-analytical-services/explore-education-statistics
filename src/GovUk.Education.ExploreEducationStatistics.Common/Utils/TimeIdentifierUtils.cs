#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class TimeIdentifierUtils
{
    private static readonly Lazy<IReadOnlyList<TimeIdentifier>> EnumsLazy = new(() =>
        EnumUtil.GetEnums<TimeIdentifier>()
            .ToList());

    private static readonly Lazy<IReadOnlyList<string>> CodesLazy = new(() =>
        EnumsLazy.Value
            .Select(ti => ti.GetEnumValue())
            .ToList());

    private static readonly Lazy<IReadOnlyList<string>> LabelsLazy = new(() =>
        EnumsLazy.Value
            .Select(ti => ti.GetEnumLabel())
            .ToList());

    public static IReadOnlyList<TimeIdentifier> Enums => EnumsLazy.Value;

    public static IReadOnlyList<string> Codes => CodesLazy.Value;

    public static IReadOnlyList<string> Labels => LabelsLazy.Value;
}

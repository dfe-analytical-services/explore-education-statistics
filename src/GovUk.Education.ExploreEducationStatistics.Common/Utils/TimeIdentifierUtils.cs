#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class TimeIdentifierUtils
{
    // TODO: EES-3959 - Remove when we've decoupled time identifiers from releases
    private static readonly HashSet<TimeIdentifier> ReleaseOnlyEnums = new()
    {
        TimeIdentifier.AcademicYearNationalTutoringProgramme
    };

    private static readonly Lazy<IReadOnlyList<TimeIdentifier>> DataEnumsLazy = new(() =>
        EnumUtil.GetEnums<TimeIdentifier>()
            .Where(ti => !ReleaseOnlyEnums.Contains(ti))
            .ToList());

    private static readonly Lazy<IReadOnlyList<string>> DataCodesLazy = new(() =>
        DataEnumsLazy.Value
            .Select(ti => ti.GetEnumValue())
            .ToList());

    private static readonly Lazy<IReadOnlyList<string>> DataLabelsLazy = new(() =>
        DataEnumsLazy.Value
            .Select(ti => ti.GetEnumLabel())
            .ToList());

    public static IReadOnlyList<TimeIdentifier> DataEnums => DataEnumsLazy.Value;

    public static IReadOnlyList<string> DataCodes => DataCodesLazy.Value;

    public static IReadOnlyList<string> DataLabels => DataLabelsLazy.Value;
}

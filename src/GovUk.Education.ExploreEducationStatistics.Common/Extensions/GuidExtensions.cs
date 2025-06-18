using System;
using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class GuidExtensions
{
    public static bool IsBlank([NotNullWhen(false)]this Guid? guid) =>
        !guid.HasValue || guid.Value == Guid.Empty;
}

using System;
using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;

public static class GuidExtensions
{
    public static bool IsNotBlank([NotNullWhen(true)]this Guid? guid) =>
        guid.HasValue && guid.Value != Guid.Empty;
}

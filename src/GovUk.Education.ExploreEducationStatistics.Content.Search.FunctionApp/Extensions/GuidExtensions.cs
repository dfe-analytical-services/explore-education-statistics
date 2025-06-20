using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class GuidExtensions
{
    public static bool IsBlank([NotNullWhen(false)]this Guid? guid) => 
        !guid.HasValue || guid.Value == Guid.Empty;
}

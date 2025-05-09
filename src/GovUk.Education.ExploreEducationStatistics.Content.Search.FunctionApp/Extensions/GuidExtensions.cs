using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class GuidExtensions
{
    public static bool HasNonEmptyValue([NotNullWhen(true)]this Guid? guid) => !(guid is null || guid == Guid.Empty);
}

#nullable enable
using System.Reflection;
using Namotion.Reflection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class ParameterInfoExtensions
    {
        public static bool IsNullable(this ParameterInfo parameter)
        {
            // Need to use a custom library for this as out of the box reflection
            // currently isn't good enough to detect this. Think this potentially gets
            // solved by .NET 6. See: https://github.com/dotnet/runtime/issues/29723
            return parameter.ToContextualParameter().Nullability == Nullability.Nullable;
        }

        public static string ToShortString(this ParameterInfo parameter)
        {
            return $"{parameter.ParameterType.Name} {parameter.Name}";
        }
    }
}
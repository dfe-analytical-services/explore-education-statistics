#nullable enable
using System;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Utils
{
    public static class EnvironmentUtils
    {
        /// <summary>
        /// Use the presence of 'Development' as the environment value to detect if the function is running on a local
        /// computer.
        /// </summary>
        /// <remarks>
        /// Azure Functions Core Tools sets AZURE_FUNCTIONS_ENVIRONMENT as 'Development' when running on a local
        /// computer and this can't overriden in local.settings.json file. In all Azure environments the environment is
        /// 'Production' unless altered.
        /// </remarks>
        /// <returns>True if the function is running on a local computer otherwise false</returns>
        // TODO DW - OK to remove this?
        // public static bool IsLocalEnvironment()
        // {
        //     var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
        //     return environment?.Equals(EnvironmentName.Development) ?? false;
        // }
    }
}

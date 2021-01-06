using System;
using Microsoft.Extensions.Hosting;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Utils
{
    public static class PublisherUtils
    {
        public static bool IsDevelopment()
        {
            var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            return environment?.Equals(EnvironmentName.Development) ?? false;
        }
    }
}
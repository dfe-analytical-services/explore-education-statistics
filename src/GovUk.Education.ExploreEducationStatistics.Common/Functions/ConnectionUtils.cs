using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Functions
{
    public static class ConnectionUtils
    {
        public static string GetConnectionString(string name, string connectionTypeValue)
        {
            // Attempt to get a connection string defined for running locally.
            // Settings in the local.settings.json file are only used by Functions tools when running locally.
            var connectionString =
                Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(connectionString))
            {
                // Get the connection string from the Azure Functions App using the naming convention for type SQLAzure.
                connectionString = Environment.GetEnvironmentVariable($"{connectionTypeValue}_{name}", EnvironmentVariableTarget.Process);
            }

            return connectionString;
        }
        
        public enum ConnectionTypes
        {
            AZURE_STORAGE,
            AZURE_SQL
        }
        
        public static readonly Dictionary<ConnectionTypes, string> ConnectionTypeValues =
            new Dictionary<ConnectionTypes, string>
            {
                {
                    ConnectionTypes.AZURE_STORAGE, "CUSTOMCONNSTR"
                },
                {
                    ConnectionTypes.AZURE_SQL, "SQLAZURECONNSTR"
                }
            }; 
    }
}
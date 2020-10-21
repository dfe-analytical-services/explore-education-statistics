using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Functions
{
    public enum ConnectionTypes
    {
        AZURE_STORAGE,
        AZURE_SQL
    }

    public static class ConnectionUtils
    {

        public static string GetAzureStorageConnectionString(string name)
        {
            return GetConnectionString(name,
                $"{ConnectionTypeValues[ConnectionTypes.AZURE_STORAGE]}");
        }
        
        public static string GetAzureSqlConnectionString(string name)
        {
            return GetConnectionString(name,
                $"{ConnectionTypeValues[ConnectionTypes.AZURE_SQL]}");
        }
        
        private static string GetConnectionString(string name, string connectionTypeValue)
        {
            // Attempt to get a connection string defined for running locally.
            // Settings in the local.settings.json file are only used by Functions tools when running locally.
            var connectionString =
                Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);

            if (connectionString.IsNullOrEmpty())
            {
                // Get the connection string from the Azure Functions App using the naming convention for type SQLAzure.
                connectionString = Environment.GetEnvironmentVariable($"{connectionTypeValue}_{name}", EnvironmentVariableTarget.Process);
            }

            return connectionString;
        }

        
        private static readonly Dictionary<ConnectionTypes, string> ConnectionTypeValues =
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
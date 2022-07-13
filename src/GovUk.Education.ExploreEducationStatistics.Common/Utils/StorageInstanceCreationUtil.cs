using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public class StorageInstanceCreationUtil : IStorageInstanceCreationUtil
{
    private static readonly ConcurrentDictionary<string, Unit> CreatedInstances = new();

    public async Task CreateInstanceIfNotExistsAsync(
        string connectionString,
        AzureStorageType storageType,
        string instance,
        Func<Task> createIfNotExists)
    {
        var identifyingString = $"{instance}{storageType.ToString()}{connectionString}";
        if (!CreatedInstances.ContainsKey(identifyingString))
        {
            await createIfNotExists.Invoke();
            CreatedInstances[identifyingString] = Unit.Instance;
        }
    }

    public void CreateInstanceIfNotExists(
        string connectionString,
        AzureStorageType storageType,
        string instance,
        Action createIfNotExists)
    {
        var identifyingString = $"{instance}{storageType.ToString()}{connectionString}";
        if (!CreatedInstances.ContainsKey(identifyingString))
        {
            createIfNotExists.Invoke();
            CreatedInstances[identifyingString] = Unit.Instance;
        }
    }
}

public enum AzureStorageType
{
    Blob,
    Queue,
    Table,
}

using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils.Interfaces;

public interface IStorageInstanceCreationUtil
{

    Task CreateInstanceIfNotExistsAsync(
        string connectionString,
        AzureStorageType storageType,
        string instance,
        Func<Task> createIfNotExists);

    void CreateInstanceIfNotExists(
        string connectionString,
        AzureStorageType storageType,
        string instance,
        Action createIfNotExists);
}

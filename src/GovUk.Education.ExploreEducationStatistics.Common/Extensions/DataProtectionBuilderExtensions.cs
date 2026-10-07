using Azure.Security.KeyVault.Secrets;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class DataProtectionBuilderExtensions
{
    /// <summary>
    /// Persists Data Protection keys as Key Vault secrets, named
    /// "{secretNamePrefix}{keyId}".
    ///
    /// This method defines where the secrets live.
    /// </summary>
    public static IDataProtectionBuilder PersistKeysToAzureKeyVaultSecrets(
        this IDataProtectionBuilder builder,
        SecretClient secretClient,
        string secretNamePrefix
    )
    {
        builder.Services.Configure<KeyManagementOptions>(options =>
            options.XmlRepository = new KeyVaultXmlRepository(secretClient, secretNamePrefix)
        );
        return builder;
    }
}

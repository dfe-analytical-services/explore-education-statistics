using System.Xml.Linq;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

/// <summary>
/// Persists ASP.NET Core Data Protection keys as Key Vault secrets, rather than on local disk.
/// This allows the keys to be safely shareable between deployment slots during slot swapping,
/// which previously caused restarts in production slots due to malformed XML being read mid-write
/// from the other slot.
/// </summary>
public class KeyVaultXmlRepository(SecretClient secretClient, string secretNamePrefix) : IXmlRepository
{
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return secretClient
            .GetPropertiesOfSecrets()
            .Where(properties =>
                properties.Enabled != false && properties.Name.StartsWith(secretNamePrefix, StringComparison.Ordinal)
            )
            .Select(properties => XElement.Parse(secretClient.GetSecret(properties.Name).Value.Value))
            .ToList();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        secretClient.SetSecret($"{secretNamePrefix}{friendlyName}", element.ToString());
    }
}

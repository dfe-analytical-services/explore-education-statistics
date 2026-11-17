import { env } from 'process';
import { AzurePublicationSearchResult } from '@frontend/services/azurePublicationService';
import { ManagedIdentityCredential } from '@azure/identity';
import { AzureKeyCredential, SearchClient } from '@azure/search-documents';
import { AzureDataSetIndexItem } from '@frontend/services/azureDataSetService';

const {
  AZURE_SEARCH_ENDPOINT,
  AZURE_SEARCH_INDEX,
  AZURE_DATASETS_SEARCH_INDEX,
  AZURE_SEARCH_QUERY_KEY,
  AZURE_CLIENT_ID,
} = env;

// Uses the user-assigned identity when AZURE_CLIENT_ID is set, and the system-assigned identity otherwise.
function createManagedIdentityCredential() {
  return new ManagedIdentityCredential({ clientId: AZURE_CLIENT_ID });
}

export function initialiseAzurePublicationsSearchClient() {
  return new SearchClient<AzurePublicationSearchResult>(
    AZURE_SEARCH_ENDPOINT || '',
    AZURE_SEARCH_INDEX || '',
    AZURE_SEARCH_QUERY_KEY
      ? new AzureKeyCredential(AZURE_SEARCH_QUERY_KEY)
      : createManagedIdentityCredential(),
  );
}
export function initialiseAzureDataSetsSearchClient() {
  return new SearchClient<AzureDataSetIndexItem>(
    AZURE_SEARCH_ENDPOINT || '',
    AZURE_DATASETS_SEARCH_INDEX || '',
    AZURE_SEARCH_QUERY_KEY
      ? new AzureKeyCredential(AZURE_SEARCH_QUERY_KEY)
      : createManagedIdentityCredential(),
  );
}

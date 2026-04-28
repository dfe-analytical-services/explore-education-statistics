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
} = env;

export function initialiseAzurePublicationsSearchClient() {
  return new SearchClient<AzurePublicationSearchResult>(
    AZURE_SEARCH_ENDPOINT || '',
    AZURE_SEARCH_INDEX || '',
    AZURE_SEARCH_QUERY_KEY
      ? new AzureKeyCredential(AZURE_SEARCH_QUERY_KEY)
      : new ManagedIdentityCredential(),
  );
}
export function initialiseAzureDataSetsSearchClient() {
  return new SearchClient<AzureDataSetIndexItem>(
    AZURE_SEARCH_ENDPOINT || '',
    AZURE_DATASETS_SEARCH_INDEX || '',
    AZURE_SEARCH_QUERY_KEY
      ? new AzureKeyCredential(AZURE_SEARCH_QUERY_KEY)
      : new ManagedIdentityCredential(),
  );
}

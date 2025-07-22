import { env } from 'process';
import { AzurePublicationSearchResult } from '@frontend/services/azurePublicationService';
import { DefaultAzureCredential } from '@azure/identity';
import { AzureKeyCredential, SearchClient } from '@azure/search-documents';

const { AZURE_SEARCH_ENDPOINT, AZURE_SEARCH_INDEX, AZURE_SEARCH_QUERY_KEY } =
  env;

export default function initialiseAzureSearchClient() {
  return new SearchClient<AzurePublicationSearchResult>(
    AZURE_SEARCH_ENDPOINT || '',
    AZURE_SEARCH_INDEX || '',
    AZURE_SEARCH_QUERY_KEY
      ? new AzureKeyCredential(AZURE_SEARCH_QUERY_KEY)
      : new DefaultAzureCredential(),
  );
}

import { AzureDataSetSuggestResult } from '@frontend/services/azureDataSetService';

// eslint-disable-next-line import/prefer-default-export
export const testDataSetSuggestions: AzureDataSetSuggestResult[] = [
  {
    summary: 'Data set 1 summary',
    dataSetFileId: 'data-set-file-id-1',
    title: 'Data set 1',
    highlightedMatch: 'Data set <strong>1</strong>',
  },
  {
    summary: 'Data set 2 summary',
    dataSetFileId: 'data-set-file-id-2',
    title: 'Data set 2',
    highlightedMatch: 'Data set <strong>2</strong> summary',
  },
];

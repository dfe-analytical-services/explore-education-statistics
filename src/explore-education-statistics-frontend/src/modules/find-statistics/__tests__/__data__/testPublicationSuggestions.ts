import { AzurePublicationSuggestResult } from '@frontend/services/azurePublicationService';

// eslint-disable-next-line import/prefer-default-export
export const testPublicationSuggestions: AzurePublicationSuggestResult[] = [
  {
    summary: 'Publication 1 summary',
    title: 'Publication 1',
    releaseSlug: 'latest-release-slug-1',
    publicationSlug: 'publication-1-slug',
    highlightedMatch: 'Publication <strong>1</strong>',
  },
  {
    summary: 'Publication 2 summary',
    title: 'Publication 2',
    releaseSlug: 'latest-release-slug-2',
    publicationSlug: 'publication-2-slug',
    highlightedMatch: 'Publication <strong>2</strong> summary',
  },
  {
    summary: 'Publication 3 summary',
    title: 'Publication 3',
    releaseSlug: 'latest-release-slug-3',
    publicationSlug: 'publication-3-slug',
    highlightedMatch: 'Publication <strong>3</strong>',
  },
];

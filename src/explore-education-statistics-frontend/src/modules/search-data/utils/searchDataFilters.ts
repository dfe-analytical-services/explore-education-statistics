export const searchDataFilters = [
  'dataSetType',
  'geographicLevel',
  'latestDataOnly',
  'publicationId',
  'releaseType',
  'search',
  'themeId',
] as const;

export type SearchDataFilter = (typeof searchDataFilters)[number];

export const searchDataFilters = [
  'dataSetType',
  'geographicLevel',
  'latestDataOnly',
  'organisationId',
  'publicationId',
  'releaseType',
  'search',
  'themeId',
] as const;

export type SearchDataFilter = (typeof searchDataFilters)[number];

export const searchDataFilters = [
  'dataSetType',
  'geographicLevel',
  'latestDataOnly',
  'releaseType',
  'search',
  'themeId',
] as const;

export type SearchDataFilter = (typeof searchDataFilters)[number];

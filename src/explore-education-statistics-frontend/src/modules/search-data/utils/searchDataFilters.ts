export const searchDataFilters = [
  'dataSetType',
  'latestDataOnly',
  'releaseType',
  'search',
  'themeId',
] as const;

export type SearchDataFilter = (typeof searchDataFilters)[number];

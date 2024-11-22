export const dataSetFileFilters = [
  'dataSetType',
  'latest',
  'publicationId',
  'releaseId',
  'geographicLevel',
  'searchTerm',
  'themeId',
] as const;

export type DataSetFileFilter = (typeof dataSetFileFilters)[number];

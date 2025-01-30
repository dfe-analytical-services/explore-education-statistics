export const dataSetFileFilters = [
  'dataSetType',
  'latest',
  'publicationId',
  'releaseVersionId',
  'geographicLevel',
  'searchTerm',
  'themeId',
] as const;

export type DataSetFileFilter = (typeof dataSetFileFilters)[number];

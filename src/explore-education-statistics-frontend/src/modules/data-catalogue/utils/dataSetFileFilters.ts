export const dataSetFileFilters = [
  'dataSetType',
  'latest',
  'publicationId',
  'releaseId',
  'searchTerm',
  'themeId',
] as const;

export type DataSetFileFilter = (typeof dataSetFileFilters)[number];

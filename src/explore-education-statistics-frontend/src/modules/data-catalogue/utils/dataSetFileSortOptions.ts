export const dataSetFileSortOptions = [
  'newest',
  'oldest',
  'relevance',
  'title',
] as const;

export type DataSetFileSortOption = (typeof dataSetFileSortOptions)[number];

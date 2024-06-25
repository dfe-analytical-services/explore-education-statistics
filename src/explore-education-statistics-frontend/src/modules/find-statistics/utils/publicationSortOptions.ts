export const publicationSortOptions = [
  'newest',
  'oldest',
  'relevance',
  'title',
] as const;

export type PublicationSortOption = (typeof publicationSortOptions)[number];

export const publicationFilters = ['releaseType', 'search', 'themeId'] as const;

export type PublicationFilter = (typeof publicationFilters)[number];

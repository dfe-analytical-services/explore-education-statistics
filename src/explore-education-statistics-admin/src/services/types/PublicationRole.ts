export const publicationRoles = ['Owner', 'Allower'] as const;
export type PublicationRole = (typeof publicationRoles)[number];

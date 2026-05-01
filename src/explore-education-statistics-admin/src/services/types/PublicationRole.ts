export const PublicationRole = {
  Drafter: 'Drafter',
  Approver: 'Approver',
} as const;

export const publicationRoles = Object.values(PublicationRole);

export type PublicationRole =
  (typeof PublicationRole)[keyof typeof PublicationRole];

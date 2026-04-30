export const PublicationRole = {
  Drafter: 'Drafter',
  Approver: 'Approver',
} as const;

export type PublicationRole =
  (typeof PublicationRole)[keyof typeof PublicationRole];

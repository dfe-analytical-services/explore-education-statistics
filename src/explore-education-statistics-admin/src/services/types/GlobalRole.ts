export const GlobalRole = {
  StandardUser: 'StandardUser',
  // BAU Users are now referred to as Super Users in the UI, but the backend and frontend code still uses the BAU terminology.
  BauUser: 'BauUser',
} as const;

export type GlobalRole = (typeof GlobalRole)[keyof typeof GlobalRole];

export const globalRoleLabels: Record<GlobalRole, string> = {
  [GlobalRole.StandardUser]: 'Standard User',
  // BAU Users are now referred to as Super Users in the UI, but the backend and frontend code still uses the BAU terminology.
  [GlobalRole.BauUser]: 'Super User',
};

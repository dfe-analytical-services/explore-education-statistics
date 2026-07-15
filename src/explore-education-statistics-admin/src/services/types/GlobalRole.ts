export const GlobalRole = {
  StandardUser: 'StandardUser',
  BauUser: 'BauUser',
} as const;

export type GlobalRole = (typeof GlobalRole)[keyof typeof GlobalRole];

export const globalRoleLabels: Record<GlobalRole, string> = {
  [GlobalRole.StandardUser]: 'Standard User',
  [GlobalRole.BauUser]: 'BAU User',
};

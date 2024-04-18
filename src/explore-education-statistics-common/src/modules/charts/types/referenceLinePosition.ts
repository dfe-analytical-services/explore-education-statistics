export const otherAxisPositionTypes = {
  default: 'default',
  betweenDataPoints: 'between-data-points',
  custom: 'custom',
} as const;

export type OtherAxisPositionTypeKey = keyof typeof otherAxisPositionTypes;

export type OtherAxisPositionType =
  (typeof otherAxisPositionTypes)[OtherAxisPositionTypeKey];

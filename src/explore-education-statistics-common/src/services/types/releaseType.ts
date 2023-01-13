export const releaseTypes = {
  // NOTE: Set in the desired display order
  NationalStatistics: 'National statistics',
  OfficialStatistics: 'Official statistics',
  ExperimentalStatistics: 'Experimental statistics',
  AdHocStatistics: 'Ad hoc statistics',
  ManagementInformation: 'Management information',
} as const;

export type ReleaseType = keyof typeof releaseTypes;

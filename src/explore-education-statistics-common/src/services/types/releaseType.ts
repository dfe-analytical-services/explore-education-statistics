export const releaseTypes = {
  AdHocStatistics: 'Ad hoc statistics',
  ExperimentalStatistics: 'Experimental statistics',
  ManagementInformation: 'Management information',
  NationalStatistics: 'National statistics',
  OfficialStatistics: 'Official statistics',
} as const;

export type ReleaseType = keyof typeof releaseTypes;

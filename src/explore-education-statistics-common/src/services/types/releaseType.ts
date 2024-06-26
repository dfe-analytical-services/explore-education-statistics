export const releaseTypes = {
  // NOTE: Set in the desired display order
  AccreditedOfficialStatistics: 'Accredited official statistics',
  OfficialStatistics: 'Official statistics',
  OfficialStatisticsInDevelopment: 'Official statistics in development',
  ExperimentalStatistics: 'Experimental statistics',
  AdHocStatistics: 'Ad hoc statistics',
  ManagementInformation: 'Management information',
} as const;

export type ReleaseType = keyof typeof releaseTypes;

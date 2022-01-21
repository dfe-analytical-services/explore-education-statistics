export const releaseTypes = {
  AdHocStatistics: 'Ad Hoc Statistics',
  ExperimentalStatistics: 'Experimental Statistics',
  ManagementInformation: 'Management Information',
  NationalStatistics: 'National Statistics',
  OfficialStatistics: 'Official Statistics',
} as const;

export type ReleaseType = keyof typeof releaseTypes;

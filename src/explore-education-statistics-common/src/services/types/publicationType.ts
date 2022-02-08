export const publicationTypes = {
  AdHoc: 'Ad hoc statistics',
  Experimental: 'Experimental statistics',
  Legacy: 'Not yet on this service',
  ManagementInformation: 'Management information',
  NationalAndOfficial: 'National and official statistics',
} as const;

export type PublicationType = keyof typeof publicationTypes;

export const permanentExclusionChartData = [
  {
    exclusions: 4630,
    exclusionsRate: 0.06,
    name: '2012/13',
  },
  {
    exclusions: 4950,
    exclusionsRate: 0.06,
    name: '2013/14',
  },
  {
    exclusions: 5795,
    exclusionsRate: 0.07,
    name: '2014/15',
  },
  {
    exclusions: 6685,
    exclusionsRate: 0.08,
    name: '2015/16',
  },
  {
    exclusions: 7720,
    exclusionsRate: 0.1,
    name: '2016/17',
  },
];

export const fixedPeriodExclusionChartData = [
  {
    exclusions: 267520,
    exclusionsRate: 3.51,
    name: '2012/13',
  },
  {
    exclusions: 269475,
    exclusionsRate: 3.5,
    name: '2013/14',
  },
  {
    exclusions: 302975,
    exclusionsRate: 3.88,
    name: '2014/15',
  },
  {
    exclusions: 339360,
    exclusionsRate: 4.29,
    name: '2015/16',
  },
  {
    exclusions: 381865,
    exclusionsRate: 4.76,
    name: '2016/17',
  },
];

export const permanentExclusionTableData = {
  PERMANENT_EXCLUSIONS: [
    'Permanent exclusions',
    ...permanentExclusionChartData.map(({ exclusions }) => `${exclusions}`),
  ],
  PERMANENT_EXCLUSIONS_RATE: [
    'Permanent exclusions rate',
    ...permanentExclusionChartData.map(
      ({ exclusionsRate }) => `${exclusionsRate.toFixed(2)}%`,
    ),
  ],
};

export const fixedPeriodExclusionTableData = {
  FIXED_PERIOD_EXCLUSIONS: [
    'Fixed period exclusions',
    ...fixedPeriodExclusionChartData.map(({ exclusions }) => `${exclusions}`),
  ],
  FIXED_PERIOD_EXCLUSIONS_RATE: [
    'Fixed period exclusions rate',
    ...fixedPeriodExclusionChartData.map(
      ({ exclusionsRate }) => `${exclusionsRate.toFixed(2)}%`,
    ),
  ],
};

export const allTableData = {
  ...permanentExclusionTableData,
  ...fixedPeriodExclusionTableData,
};

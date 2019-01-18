export const permanentExclusionRateChartData = [
  {
    name: '2012/13',
    primary: 0.02,
    secondary: 0.12,
    special: 0.07,
    total: 0.06,
  },
  {
    name: '2013/14',
    primary: 0.02,
    secondary: 0.13,
    special: 0.07,
    total: 0.06,
  },
  {
    name: '2014/15',
    primary: 0.02,
    secondary: 0.15,
    special: 0.09,
    total: 0.07,
  },
  {
    name: '2015/16',
    primary: 0.02,
    secondary: 0.17,
    special: 0.08,
    total: 0.08,
  },
  {
    name: '2016/17',
    primary: 0.03,
    secondary: 0.2,
    special: 0.07,
    total: 0.1,
  },
];

export const permanentExclusionRateTableData = [
  [
    'primary schools',
    ...permanentExclusionRateChartData.map(({ primary }) => `${primary}%`),
  ],
  [
    'secondary schools',
    ...permanentExclusionRateChartData.map(({ secondary }) => `${secondary}%`),
  ],
  [
    'special schools',
    ...permanentExclusionRateChartData.map(({ special }) => `${special}%`),
  ],
  [
    'total',
    ...permanentExclusionRateChartData.map(({ total }) => `${total}%`),
  ],
];


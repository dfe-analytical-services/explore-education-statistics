export const sessionsAbsentChartData = [
  { name: '2012/13', unauthorised: 1.1, authorised: 4.2, overall: 5.3 },
  { name: '2013/14', unauthorised: 1.1, authorised: 3.5, overall: 4.5 },
  { name: '2014/15', unauthorised: 1.1, authorised: 3.5, overall: 4.6 },
  { name: '2015/16', unauthorised: 1.1, authorised: 3.4, overall: 4.6 },
  { name: '2016/17', unauthorised: 1.3, authorised: 3.4, overall: 4.7 },
];

export const generalChartData = [
  { name: '2012/13', enrolments: 6223740, schools: 19975 },
  { name: '2013/14', enrolments: 6276225, schools: 19940 },
  { name: '2014/15', enrolments: 6385955, schools: 20053 },
  { name: '2015/16', enrolments: 6493485, schools: 20092 },
  { name: '2016/17', enrolments: 6613550, schools: 20107 },
];

export const sessionsAbsentTableData = {
  authorisedRate: [
    'Authorised absence rate',
    ...sessionsAbsentChartData.map(
      ({ authorised }) => `${authorised.toFixed(1)}%`,
    ),
  ],
  overallRate: [
    'Overall absence rate',
    ...sessionsAbsentChartData.map(({ overall }) => `${overall.toFixed(1)}%`),
  ],
  unauthorisedRate: [
    'Unauthorised absence rate',
    ...sessionsAbsentChartData.map(
      ({ unauthorised }) => `${unauthorised.toFixed(1)}%`,
    ),
  ],
};

export const generalTableData = {
  enrolments: [
    'Enrolments',
    ...generalChartData.map(({ enrolments }) => enrolments.toFixed(0)),
  ],
  schools: [
    'Schools',
    ...generalChartData.map(({ schools }) => schools.toFixed(0)),
  ],
};

interface TableData {
  general: {
    [key: string]: string[];
  };
  sessions: {
    [key: string]: string[];
  };
}

export const allTableData: TableData = {
  general: generalTableData,
  sessions: sessionsAbsentTableData,
};

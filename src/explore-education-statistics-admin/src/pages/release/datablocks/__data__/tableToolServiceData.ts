import {
  SubjectMeta,
  TableDataResponse,
} from '@common/services/tableBuilderService';

export const testSubjectMeta: SubjectMeta = {
  filters: {
    Characteristic: {
      totalValue: '',
      hint: 'Filter by pupil characteristic',
      legend: 'Characteristic',
      name: 'characteristic',
      options: {
        EthnicGroupMajor: {
          label: 'Ethnic group major',
          options: [
            {
              label: 'Ethnicity Major Chinese',
              value: 'ethnicity-major-chinese',
            },
          ],
        },
      },
    },
    SchoolType: {
      totalValue: '',
      hint: 'Filter by school type',
      legend: 'School type',
      name: 'school_type',
      options: {
        Default: {
          label: 'Default',
          options: [
            {
              label: 'State-funded primary',
              value: 'state-funded-primary',
            },
            {
              label: 'State-funded secondary',
              value: 'state-funded-secondary',
            },
          ],
        },
      },
    },
  },
  indicators: {
    AbsenceFields: {
      label: 'Absence fields',
      options: [
        {
          value: 'authorised-absence-sessions',
          label: 'Number of authorised absence sessions',
          unit: '',
          name: 'sess_authorised',
          decimalPlaces: 2,
        },
        {
          value: 'overall-absence-sessions',
          label: 'Number of overall absence sessions',
          unit: '',
          name: 'sess_overall',
          decimalPlaces: 2,
        },
      ],
    },
  },
  locations: {
    localAuthority: {
      legend: 'Local authority',
      options: [
        { value: 'barnet', label: 'Barnet' },
        { value: 'barnsley', label: 'Barnsley' },
      ],
    },
  },
  timePeriod: {
    legend: 'Time period',
    options: [{ label: '2014/15', code: 'AY', year: 2014 }],
  },
};

export const testTableData: TableDataResponse = {
  subjectMeta: {
    filters: {
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        options: {
          EthnicGroupMajor: {
            label: 'Ethnic group major',
            options: [
              {
                label: 'Ethnicity Major Chinese',
                value: 'ethnicity-major-chinese',
              },
            ],
          },
        },
        name: 'characteristic',
      },
      SchoolType: {
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        options: {
          Default: {
            label: 'Default',
            options: [
              {
                label: 'State-funded primary',
                value: 'state-funded-primary',
              },
              {
                label: 'State-funded secondary',
                value: 'state-funded-secondary',
              },
            ],
          },
        },
        name: 'school_type',
      },
    },
    footnotes: [],
    indicators: [
      {
        label: 'Number of authorised absence sessions',
        unit: '',
        value: 'authorised-absence-sessions',
        name: 'sess_authorised',
      },
      {
        label: 'Number of overall absence sessions',
        unit: '',
        value: 'overall-absence-sessions',
        name: 'sess_overall',
      },
    ],
    locations: [
      { level: 'localAuthority', label: 'Barnet', value: 'barnet' },
      { level: 'localAuthority', label: 'Barnsley', value: 'barnsley' },
    ],
    boundaryLevels: [],
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence by characteristic',
    timePeriodRange: [
      { code: 'AY', label: '2014/15', year: 2014 },
      { code: 'AY', label: '2015/16', year: 2015 },
    ],
    geoJsonAvailable: true,
  },
  results: [
    {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      geographicLevel: 'localAuthority',
      location: {
        localAuthority: { code: 'barnet', name: 'Barnet' },
      },
      measures: {
        'authorised-absence-sessions': '2613',
        'overall-absence-sessions': '3134',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      location: {
        localAuthority: { code: 'barnsley', name: 'Barnsley' },
      },
      measures: {
        'authorised-absence-sessions': 'x',
        'overall-absence-sessions': 'x',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      location: {
        localAuthority: { code: 'barnet', name: 'Barnet' },
      },
      measures: {
        'authorised-absence-sessions': '1939',
        'overall-absence-sessions': '2269',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      geographicLevel: 'localAuthority',
      location: {
        localAuthority: { code: 'barnsley', name: 'Barnsley' },
      },
      measures: {
        'authorised-absence-sessions': '39',
        'overall-absence-sessions': '83',
      },
      timePeriod: '2014_AY',
    },
  ],
};

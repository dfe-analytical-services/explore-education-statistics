import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';

export const testTableData: TableDataResponse = {
  subjectMeta: {
    filters: {
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        options: {
          EthnicGroupMajor: {
            id: 'ethnic-group-major',
            label: 'Ethnic group major',
            options: [
              {
                label: 'Ethnicity Major Chinese',
                value: 'ethnicity-major-chinese',
              },
              {
                label: 'Ethnicity Major Black Total',
                value: 'ethnicity-major-black-total',
              },
            ],
            order: 0,
          },
        },
        order: 0,
        name: 'characteristic',
      },
      SchoolType: {
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        options: {
          Default: {
            id: 'default',
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
            order: 0,
          },
        },
        order: 1,
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
    locations: {
      localAuthority: [
        { id: 'barnet', label: 'Barnet', value: 'barnet' },
        { id: 'barnsley', label: 'Barnsley', value: 'barnsley' },
      ],
    },
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
      locationId: 'barnet',
      measures: {
        'authorised-absence-sessions': '2613',
        'overall-absence-sessions': '3134',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-black-total', 'state-funded-primary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnet',
      measures: {
        'authorised-absence-sessions': '30364',
        'overall-absence-sessions': '40327',
      },
      timePeriod: '2015_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnsley',
      measures: {
        'authorised-absence-sessions': 'x',
        'overall-absence-sessions': 'x',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnsley',
      measures: {
        'authorised-absence-sessions': '479',
        'overall-absence-sessions': '843',
      },
      timePeriod: '2015_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnet',
      measures: {
        'authorised-absence-sessions': '1939',
        'overall-absence-sessions': '2269',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnet',
      measures: {
        'authorised-absence-sessions': '26594',
        'overall-absence-sessions': '37084',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnsley',
      measures: {
        'authorised-absence-sessions': '19',
        'overall-absence-sessions': '35',
      },
      timePeriod: '2015_AY',
    },
    {
      filters: ['ethnicity-major-black-total', 'state-funded-primary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnsley',
      measures: {
        'authorised-absence-sessions': '939',
        'overall-absence-sessions': '1268',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnet',
      measures: {
        'authorised-absence-sessions': '27833',
        'overall-absence-sessions': '38130',
      },
      timePeriod: '2015_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnsley',
      measures: {
        'authorised-absence-sessions': '39',
        'overall-absence-sessions': '83',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-black-total', 'state-funded-primary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnet',
      measures: {
        'authorised-absence-sessions': '31322',
        'overall-absence-sessions': '41228',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-primary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnet',
      measures: {
        'authorised-absence-sessions': '2652',
        'overall-absence-sessions': '3093',
      },
      timePeriod: '2015_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnet',
      measures: {
        'authorised-absence-sessions': '1856',
        'overall-absence-sessions': '2125',
      },
      timePeriod: '2015_AY',
    },
    {
      filters: ['ethnicity-major-black-total', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnsley',
      measures: {
        'authorised-absence-sessions': '745',
        'overall-absence-sessions': '1105',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['ethnicity-major-black-total', 'state-funded-primary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnsley',
      measures: {
        'authorised-absence-sessions': '825',
        'overall-absence-sessions': '1003',
      },
      timePeriod: '2015_AY',
    },
    {
      filters: ['ethnicity-major-chinese', 'state-funded-secondary'],
      geographicLevel: 'localAuthority',
      locationId: 'barnsley',
      measures: {
        'authorised-absence-sessions': '4',
        'overall-absence-sessions': '4',
      },
      timePeriod: '2015_AY',
    },
  ],
};

export const testFullTable = mapFullTable(testTableData);

import { UnmappedTableHeadersConfig } from '@common/services/permalinkService';
import { TableDataResponse } from '@common/services/tableBuilderService';

export const testData1Table: TableDataResponse = {
  subjectMeta: {
    filters: {
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        name: 'characteristic',
        options: {
          EthnicGroupMajor: {
            id: 'ethnic-group-major',
            label: 'Ethnic group major',
            options: [
              {
                label: 'Ethnicity Major Asian Total',
                value: 'filter-1',
              },
              {
                label: 'Ethnicity Major Black Total',
                value: 'filter-2',
              },
            ],
            order: 0,
          },
        },
        order: 0,
      },
      SchoolType: {
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        name: 'school_type',
        options: {
          Default: {
            id: 'default',
            label: 'Default',
            options: [
              {
                label: 'State-funded primary',
                value: 'filter-3',
              },
              {
                label: 'State-funded secondary',
                value: 'filter-4',
              },
            ],
            order: 0,
          },
        },
        order: 1,
      },
    },
    footnotes: [],
    geoJsonAvailable: false,
    indicators: [
      {
        value: 'indicator-1',
        label: 'Number of authorised absence sessions',
        unit: '',
        name: 'sess_authorised',
        decimalPlaces: 0,
      },
      {
        value: 'indicator-2',
        label: 'Number of overall absence sessions',
        unit: '',
        name: 'sess_overall',
        decimalPlaces: 0,
      },
    ],
    locations: {
      localAuthority: [
        {
          id: 'location-1',
          value: 'E09000003',
          label: 'Barnet',
        },
        {
          id: 'location-2',
          value: 'E08000016',
          label: 'Barnsley',
        },
      ],
    },
    boundaryLevels: [],
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence by characteristic',
    timePeriodRange: [
      { label: '2013/14', code: 'AY', year: 2013 },
      { label: '2014/15', code: 'AY', year: 2014 },
    ],
  },
  results: [
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: {
        'indicator-1': '33725',
        'indicator-2': '41239',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: {
        'indicator-1': '31241',
        'indicator-2': '41945',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: {
        'indicator-1': '442',
        'indicator-2': '788',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: {
        'indicator-1': '1582',
        'indicator-2': '2122',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: {
        'indicator-1': '481',
        'indicator-2': '752',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: {
        'indicator-1': '904',
        'indicator-2': '1215',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: {
        'indicator-1': '32125',
        'indicator-2': '39697',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: {
        'indicator-1': '31244',
        'indicator-2': '36083',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: {
        'indicator-1': '26594',
        'indicator-2': '37084',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: {
        'indicator-1': '30389',
        'indicator-2': '34689',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: {
        'indicator-1': '939',
        'indicator-2': '1268',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: {
        'indicator-1': '31322',
        'indicator-2': '41228',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: {
        'indicator-1': '1135',
        'indicator-2': '1512',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: {
        'indicator-1': '25741',
        'indicator-2': '33422',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: {
        'indicator-1': '745',
        'indicator-2': '1105',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: {
        'indicator-1': '274',
        'indicator-2': '571',
      },
      timePeriod: '2014_AY',
    },
  ],
};

export const testData1TableHeadersConfig: UnmappedTableHeadersConfig = {
  columnGroups: [
    [
      {
        value: 'filter-1',
        type: 'Filter',
      },
      {
        value: 'filter-2',
        type: 'Filter',
      },
    ],
  ],
  rowGroups: [
    [
      {
        value: 'filter-3',
        type: 'Filter',
      },
      {
        value: 'filter-4',
        type: 'Filter',
      },
    ],
    [
      {
        value: 'location-1',
        type: 'Location',
        level: 'localAuthority',
      },
      {
        value: 'location-2',
        type: 'Location',
        level: 'localAuthority',
      },
    ],
  ],
  columns: [
    { value: '2013_AY', type: 'TimePeriod' },
    { value: '2014_AY', type: 'TimePeriod' },
  ],
  rows: [
    {
      value: 'indicator-1',
      type: 'Indicator',
    },
    {
      value: 'indicator-2',
      type: 'Indicator',
    },
  ],
};

export const testData2Table: TableDataResponse = {
  subjectMeta: {
    filters: {
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        name: 'characteristic',
        options: {
          EthnicGroupMajor: {
            id: 'ethnic-group-major',
            label: 'Ethnic group major',
            options: [
              {
                label: 'Ethnicity Major Asian Total',
                value: 'filter-1',
              },
              {
                label: 'Ethnicity Major Black Total',
                value: 'filter-2',
              },
            ],
            order: 0,
          },
        },
        order: 0,
      },
      SchoolType: {
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        name: 'school_type',
        options: {
          Default: {
            id: 'default',
            label: 'Default',
            options: [
              {
                label: 'State-funded primary',
                value: 'filter-3',
              },
              {
                label: 'State-funded secondary',
                value: 'filter-4',
              },
            ],
            order: 0,
          },
        },
        order: 1,
      },
    },
    footnotes: [],
    geoJsonAvailable: false,
    indicators: [
      {
        value: 'indicator-3',
        label: 'Authorised absence rate',
        unit: '%',
        name: 'sess_authorised_percent',
        decimalPlaces: 1,
      },
    ],
    locations: {
      localAuthority: [
        {
          id: 'location-1',
          value: 'E09000003',
          label: 'Barnet',
        },
        {
          id: 'location-2',
          value: 'E08000016',
          label: 'Barnsley',
        },
      ],
    },
    boundaryLevels: [],
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence by characteristic',
    timePeriodRange: [
      { label: '2013/14', code: 'AY', year: 2013 },
      { label: '2014/15', code: 'AY', year: 2014 },
    ],
  },
  results: [
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '3.4' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '2.9' },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '1.8' },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '4.3' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '3.3' },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '2.5' },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '3.4' },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '3.3' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '3' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '3.3' },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '2.4' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '2.8' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '3.7' },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '2.9' },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '2.6' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '2.1' },
      timePeriod: '2014_AY',
    },
  ],
};

export const testData2TableHeadersConfig: UnmappedTableHeadersConfig = {
  columnGroups: [
    [
      {
        value: 'filter-1',
        type: 'Filter',
      },
      {
        value: 'filter-2',
        type: 'Filter',
      },
    ],
  ],
  rowGroups: [
    [
      {
        value: 'filter-3',
        type: 'Filter',
      },
      {
        value: 'filter-4',
        type: 'Filter',
      },
    ],
    [
      {
        value: 'location-1',
        type: 'Location',
        level: 'localAuthority',
      },
      {
        value: 'location-2',
        type: 'Location',
        level: 'localAuthority',
      },
    ],
  ],
  columns: [
    { value: '2013_AY', type: 'TimePeriod' },
    { value: '2014_AY', type: 'TimePeriod' },
  ],
  rows: [
    {
      value: 'indicator-3',
      type: 'Indicator',
    },
  ],
};

export const testData3Table: TableDataResponse = {
  subjectMeta: {
    filters: {
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        name: 'characteristic',
        options: {
          EthnicGroupMajor: {
            id: 'ethnic-group-major',
            label: 'Ethnic group major',
            options: [
              {
                label: 'Ethnicity Major Asian Total',
                value: 'filter-1',
              },
              {
                label: 'Ethnicity Major Black Total',
                value: 'filter-2',
              },
            ],
            order: 0,
          },
        },
        order: 0,
      },
      SchoolType: {
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        name: 'school_type',
        options: {
          Default: {
            id: 'default',
            label: 'Default',
            options: [
              {
                label: 'State-funded primary',
                value: 'filter-3',
              },
              {
                label: 'State-funded secondary',
                value: 'filter-4',
              },
            ],
            order: 0,
          },
        },
        order: 1,
      },
    },
    footnotes: [],
    geoJsonAvailable: false,
    indicators: [
      {
        value: 'indicator-3',
        label: 'Authorised absence rate',
        unit: '%',
        name: 'sess_authorised_percent',
        decimalPlaces: 1,
      },
    ],
    locations: {
      localAuthority: [
        {
          id: 'location-1',
          value: 'E09000003',
          label: 'Barnet',
        },
        {
          id: 'location-2',
          value: 'E08000016',
          label: 'Barnsley',
        },
      ],
    },
    boundaryLevels: [],
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence by characteristic',
    timePeriodRange: [{ label: '2014/15', code: 'AY', year: 2014 }],
  },
  results: [
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '3.4' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '4.3' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '3.3' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '3' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '2.4' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-3'],
      geographicLevel: 'localAuthority',
      locationId: 'location-1',
      measures: { 'indicator-3': '2.8' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-2', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '2.6' },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-1', 'filter-4'],
      geographicLevel: 'localAuthority',
      locationId: 'location-2',
      measures: { 'indicator-3': '2.1' },
      timePeriod: '2014_AY',
    },
  ],
};

export const testData3TableHeadersConfig: UnmappedTableHeadersConfig = {
  columnGroups: [
    [
      {
        value: 'filter-1',
        type: 'Filter',
      },
      {
        value: 'filter-2',
        type: 'Filter',
      },
    ],
  ],
  rowGroups: [
    [
      {
        value: 'filter-3',
        type: 'Filter',
      },
      {
        value: 'filter-4',
        type: 'Filter',
      },
    ],
    [
      {
        value: 'location-1',
        type: 'Location',
        level: 'localAuthority',
      },
      {
        value: 'location-2',
        type: 'Location',
        level: 'localAuthority',
      },
    ],
  ],
  columns: [{ value: '2014_AY', type: 'TimePeriod' }],
  rows: [
    {
      value: 'indicator-3',
      type: 'Indicator',
    },
  ],
};

export const testDataNoFiltersTable: TableDataResponse = {
  subjectMeta: {
    geoJsonAvailable: false,
    filters: {},
    footnotes: [],
    indicators: [
      {
        value: 'indicator-4',
        label: 'Number of overall absence sessions',
        unit: '',
        name: 'sess_overall',
        decimalPlaces: 0,
      },
      {
        value: 'indicator-5',
        label: 'Number of authorised absence sessions',
        unit: '',
        name: 'sess_overall',
        decimalPlaces: 0,
      },
      {
        value: 'indicator-6',
        label: 'Authorised absence rate',
        unit: '%',
        name: 'sess_authorised_percent',
        decimalPlaces: 1,
      },
    ],
    locations: {
      country: [
        {
          id: 'location-5',
          value: 'E92000001',
          label: 'England',
        },
      ],
    },
    boundaryLevels: [],
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence in prus',
    timePeriodRange: [
      { label: '2014/15', code: 'AY', year: 2014 },
      { label: '2015/16', code: 'AY', year: 2015 },
      { label: '2016/17', code: 'AY', year: 2016 },
    ],
  },
  results: [
    {
      filters: [],
      geographicLevel: 'country',
      locationId: 'location-5',
      measures: {
        'indicator-4': '2453340',
        'indicator-5': '1397521',
        'indicator-6': '18.6',
      },
      timePeriod: '2015_AY',
    },
    {
      filters: [],
      geographicLevel: 'country',
      locationId: 'location-5',
      measures: {
        'indicator-4': '2212399',
        'indicator-5': '1280964',
        'indicator-6': '18.3',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: [],
      geographicLevel: 'country',
      locationId: 'location-5',
      measures: {
        'indicator-4': '2637752',
        'indicator-5': '1488865',
        'indicator-6': '19.2',
      },
      timePeriod: '2016_AY',
    },
  ],
};

export const testDataNoFiltersTableHeadersConfig: UnmappedTableHeadersConfig = {
  columns: [
    { value: '2014_AY', type: 'TimePeriod' },
    { value: '2015_AY', type: 'TimePeriod' },
    { value: '2016_AY', type: 'TimePeriod' },
  ],
  columnGroups: [],
  rows: [
    {
      value: 'indicator-6',
      type: 'Indicator',
    },
    {
      value: 'indicator-5',
      type: 'Indicator',
    },
    {
      value: 'indicator-4',
      type: 'Indicator',
    },
  ],
  rowGroups: [
    [
      {
        value: 'location-5',
        type: 'Location',
        level: 'country',
      },
    ],
  ],
};

export const testDataFiltersWithNoResults: TableDataResponse = {
  subjectMeta: {
    geoJsonAvailable: false,
    filters: {
      Characteristic: {
        totalValue: '',
        hint: 'Filter by pupil characteristic',
        legend: 'Characteristic',
        name: 'characteristic',
        options: {
          FirstLanguage: {
            id: 'first-language',
            label: 'First language',
            options: [
              {
                label:
                  'First language Known or believed to be other than English',
                value: 'filter-5',
              },
              {
                label: 'First language Unclassified',
                value: 'filter-6',
              },
            ],
            order: 0,
          },
        },
        order: 0,
      },
      SchoolType: {
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        name: 'school_type',
        options: {
          Default: {
            id: 'default',
            label: 'Default',
            options: [
              {
                label: 'Special',
                value: 'filter-7',
              },
            ],
            order: 0,
          },
        },
        order: 1,
      },
    },
    footnotes: [],
    indicators: [
      {
        value: 'indicator-1',
        label: 'Number of authorised absence sessions',
        unit: '',
        name: 'sess_authorised',
        decimalPlaces: 0,
      },
      {
        value: 'indicator-2',
        label: 'Number of overall absence sessions',
        unit: '',
        name: 'sess_overall',
        decimalPlaces: 0,
      },
    ],
    locations: {
      localAuthority: [
        {
          id: 'location-3',
          value: 'E08000026',
          label: 'Coventry',
        },
        {
          id: 'location-4',
          value: 'E09000008',
          label: 'Croydon',
        },
      ],
    },
    boundaryLevels: [],
    publicationName: 'Pupil absence in schools in England',
    subjectName: 'Absence by characteristic',
    timePeriodRange: [
      { label: '2013/14', year: 2013, code: 'AY' },
      { label: '2014/15', year: 2014, code: 'AY' },
      { label: '2015/16', year: 2015, code: 'AY' },
    ],
  },
  results: [
    {
      filters: ['filter-5', 'filter-7'],
      geographicLevel: 'localAuthority',
      locationId: 'location-3',
      measures: {
        'indicator-1': '4185',
        'indicator-2': '5142',
      },
      timePeriod: '2015_AY',
    },
    {
      filters: ['filter-6', 'filter-7'],
      geographicLevel: 'localAuthority',
      locationId: 'location-4',
      measures: {
        'indicator-1': 'x',
        'indicator-2': 'x',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-5', 'filter-7'],
      geographicLevel: 'localAuthority',
      locationId: 'location-3',
      measures: {
        'indicator-1': '6492',
        'indicator-2': '7280',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-5', 'filter-7'],
      geographicLevel: 'localAuthority',
      locationId: 'location-4',
      measures: {
        'indicator-1': '4809',
        'indicator-2': '5076',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-5', 'filter-7'],
      geographicLevel: 'localAuthority',
      locationId: 'location-4',
      measures: {
        'indicator-1': '4179',
        'indicator-2': '4390',
      },
      timePeriod: '2013_AY',
    },
    {
      filters: ['filter-5', 'filter-7'],
      geographicLevel: 'localAuthority',
      locationId: 'location-3',
      measures: {
        'indicator-1': '5542',
        'indicator-2': '6493',
      },
      timePeriod: '2014_AY',
    },
    {
      filters: ['filter-5', 'filter-7'],
      geographicLevel: 'localAuthority',
      locationId: 'location-4',
      measures: {
        'indicator-1': '5322',
        'indicator-2': '5483',
      },
      timePeriod: '2015_AY',
    },
  ],
};

import Features from '@common/modules/charts/components/__tests__/__data__/testLocationData';

import { ChartProps } from '@common/modules/charts/types/chart';
import {
  TableDataResponse,
  TableDataResult,
  TableDataSubjectMeta,
} from '@common/services/tableBuilderService';

const subjectMeta: TableDataSubjectMeta = {
  geoJsonAvailable: true,
  publicationName: 'test',
  subjectName: 'test',
  footnotes: [],
  filters: {
    test: {
      totalValue: '',
      legend: '',
      hint: '',
      name: '',
      options: {
        test: {
          label: 'test',
          options: [
            {
              label: 'All Schools',
              value: '1',
            },
            {
              label: 'All Pupils',
              value: '1',
            },
          ],
        },
      },
    },
  },
  indicators: [
    {
      label: 'Unauthorised absence rate',
      unit: '%',
      value: '23',
      name: 'sess_unauthorised_percent',
    },
    {
      label: 'Overall absence rate',
      unit: '%',
      value: '26',
      name: 'sess_overall_percent',
    },
    {
      label: 'Authorised absence rate',
      unit: '%',
      value: '28',
      name: 'sess_authorised_percent',
    },
  ],
  timePeriodRange: [
    {
      label: '2014/15',
      year: 2014,
      code: 'HT6',
    },
    {
      label: '2015/16',
      year: 2015,
      code: 'HT6',
    },
  ],
  boundaryLevels: [],
  locations: [
    {
      level: 'country',
      value: 'E92000001',
      label: 'England',
      geoJson: [Features.E92000001],
    },
    {
      level: 'country',
      value: 'S92000001',
      label: 'Scotland',
      geoJson: [Features.S92000001],
    },
  ],
};

const data: TableDataResult[] = [
  {
    geographicLevel: 'Country',
    filters: ['1', '2'],
    location: {
      country: {
        code: 'E92000001',
        name: 'England',
      },
    },
    measures: {
      '28': '5',
      '26': '10',
      '23': '3',
    },
    timePeriod: '2014_HT6',
  },
  {
    geographicLevel: 'Country',
    filters: ['1', '2'],
    location: {
      country: {
        code: 'E92000001',
        name: 'England',
      },
    },
    measures: {
      '28': '1',
      '26': '4',
      '23': '-3',
    },
    timePeriod: '2015_HT6',
  },
];

const multipleData: TableDataResult[] = [
  {
    geographicLevel: 'Country',
    filters: ['1', '2'],
    location: {
      country: {
        code: 'E92000001',
        name: 'England',
      },
    },
    measures: {
      '28': '5',
      '26': '10',
      '23': '3',
    },
    timePeriod: '2015_HT6',
  },
  {
    geographicLevel: 'Country',
    filters: ['1', '2'],
    location: {
      country: {
        code: 'S92000001',
        name: 'Scotland',
      },
    },
    measures: {
      '28': '10',
      '26': '20',
      '23': '4',
    },
    timePeriod: '2015_HT6',
  },
];

export const testChartPropsWithData1: ChartProps = {
  data,
  meta: subjectMeta,
  width: 900,
  height: 300,
  legend: 'top',
  labels: {
    '23_1_2_____': {
      label: 'Unauthorised absence rate',
      unit: '%',
      value: '23_1_2_____',
      name: '23_1_2_____',
      colour: '#ff0000',
    },
    '26_1_2_____': {
      label: 'Overall absence rate',
      unit: '%',
      value: '26_1_2_____',
      name: '26_1_2_____',
      colour: '#00ff00',
    },
    '28_1_2_____': {
      label: 'Authorised absence rate',
      unit: '%',
      value: '28_1_2_____',
      name: '28_1_2_____',
      colour: '#0000ff',
    },
  },
  axes: {
    major: {
      type: 'major',
      groupBy: 'timePeriod',
      dataSets: [
        {
          indicator: '23',
          filters: ['1', '2'],
        },
        {
          indicator: '26',
          filters: ['1', '2'],
        },
        {
          indicator: '28',
          filters: ['1', '2'],
        },
      ],
      visible: true,
    },
    minor: {
      type: 'minor',
      visible: true,
      dataSets: [],
    },
  },
};

export const testChartPropsWithMultipleData: ChartProps = {
  data: multipleData,
  meta: subjectMeta,
  width: 900,
  height: 300,
  labels: {
    '23_1_2_____': {
      label: 'Unauthorised absence rate',
      name: '23_1_2_____',
      unit: '%',
      value: '23_1_2_____',
    },
    '26_1_2_____': {
      label: 'Overall absence rate',
      name: '26_1_2_____',
      unit: '%',
      value: '26_1_2_____',
    },
    '28_1_2_____': {
      label: 'Authorised absence rate',
      name: '28_1_2_____',
      unit: '%',
      value: '28_1_2_____',
    },
  },
  axes: {
    major: {
      type: 'major',
      groupBy: 'timePeriod',
      visible: true,
      dataSets: [
        {
          indicator: '23',
          filters: ['1', '2'],
        },
        {
          indicator: '26',
          filters: ['1', '2'],
        },
        {
          indicator: '28',
          filters: ['1', '2'],
        },
      ],
    },
    minor: {
      type: 'minor',
      visible: true,
      dataSets: [],
    },
  },
};

export const testDataBlockResponse: TableDataResponse = {
  results: data,
  subjectMeta,
};

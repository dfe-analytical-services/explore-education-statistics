/* eslint-disable @typescript-eslint/camelcase */
import { DataBlockProps } from '@common/modules/find-statistics/components/DataBlock';
import { ChartProps } from 'explore-education-statistics-common/src/modules/find-statistics/components/charts/ChartFunctions';
import { GeographicLevel } from 'explore-education-statistics-common/src/services/dataBlockService';
import TimePeriod from 'explore-education-statistics-common/src/services/types/TimePeriod';

export const newApiTest: DataBlockProps = {
  type: 'datablock',
  id: 'test',
  dataBlockRequest: {
    subjectId: 1,
    regions: [],
    localAuthorityDistricts: [],
    localAuthorities: [],
    countries: [],
    geographicLevel: GeographicLevel.National,
    startYear: '2014',
    endYear: '2017',
    indicators: ['23', '26', '28'],
    filters: ['1', '2'],
  },
};

export const newApiHorizontalData: ChartProps = {
  data: {
    publicationId: 'test',
    releaseDate: new Date(),
    releaseId: '1',
    subjectId: 1,
    geographicLevel: GeographicLevel.National,
    result: [
      {
        filters: ['1'],
        location: {
          country: {
            country_code: 'E92000001',
            country_name: 'England',
          },
          region: {
            region_code: '',
            region_name: '',
          },
          localAuthority: {
            new_la_code: '',
            old_la_code: '',
            la_name: '',
          },
          localAuthorityDistrict: {
            sch_lad_code: '',
            sch_lad_name: '',
          },
        },
        measures: {
          '28': '5',
          '26': '10',
          '23': '3',
        },
        timeIdentifier: 'HT6',
        year: 2014,
      },
      {
        filters: ['1'],
        location: {
          country: {
            country_code: 'E92000001',
            country_name: 'England',
          },
          region: {
            region_code: '',
            region_name: '',
          },
          localAuthority: {
            new_la_code: '',
            old_la_code: '',
            la_name: '',
          },
          localAuthorityDistrict: {
            sch_lad_code: '',
            sch_lad_name: '',
          },
        },
        measures: {
          '28': '5',
          '26': '10',
          '23': '3',
        },
        timeIdentifier: 'HT6',
        year: 2015,
      },
    ],
  },

  meta: {
    filters: {
      '1': {
        label: 'All Schools',
        value: '1',
      },
    },

    indicators: {
      '23': {
        label: 'Unauthorised absence rate',
        unit: '%',
        value: '23',
      },
      '26': {
        label: 'Overall absence rate',
        unit: '%',
        value: '26',
      },
      '28': {
        label: 'Authorised absence rate',
        unit: '%',
        value: '28',
      },
    },

    timePeriods: {
      '2014': new TimePeriod(2014, 'HT6'),
      '2015': new TimePeriod(2015, 'HT6'),
    },

    locations: {},
  },

  dataSets: [
    { indicator: '23', filters: ['1'] },
    { indicator: '26', filters: ['1'] },
    { indicator: '28', filters: ['1'] },
  ],

  dataLabels: {
    '23_1': {
      value: '23_1',
      unit: '%',
      name: '23_1',
      label: 'Un auth rate',
    },
    '26_1': {
      value: '26_1',
      unit: '%',
      name: '26_1',
      label: 'All rate',
    },
    '28_1': {
      value: '28_1',
      unit: '%',
      name: '28_1',
      label: 'Auth rate',
    },
  },

  xAxis: { title: 'test x axis' },
  yAxis: { title: 'test y axis' },
};

export const newChartsApiDataBlock: DataBlockProps = {
  type: 'datablock',
  id: 'test',
  dataBlockRequest: {
    subjectId: 1,
    regions: [],
    localAuthorityDistricts: [],
    localAuthorities: [],
    countries: [],
    geographicLevel: GeographicLevel.National,
    startYear: '2012',
    endYear: '2017',
    indicators: ['23', '26', '28', '27'],
    filters: ['1', '2'],
  },
  showTables: false,
  charts: [
    {
      type: 'verticalbar',
      xAxis: { title: '' },
      yAxis: { title: '' },

      dataLabels: {
        '23_1_2': {
          value: '23_1_2',
          unit: '%',
          name: '23_1_2',
          label: 'Unauthorised absence rate',
        },
        '26_1_2': {
          value: '26_1_2',
          unit: '%',
          name: '26_1_2',
          label: 'Overall absence rate',
        },
        '28_1_2': {
          value: '28_1_2',
          unit: '%',
          name: '28_1_2',
          label: 'Authorised absence rate',
        },
        '27_1_2': {
          value: '27_1_2',
          unit: '',
          name: '27_1_2',
          label: 'Absence Rate',
        },
      },

      dataSets: [
        {
          indicator: '23',
          filters: ['1', '2'],
        },
        { indicator: '26', filters: ['1', '2'] },
        { indicator: '28', filters: ['1', '2'] },
        // { indicator: '27', filters: ['1', '2'] },
      ],
    },
  ],
};

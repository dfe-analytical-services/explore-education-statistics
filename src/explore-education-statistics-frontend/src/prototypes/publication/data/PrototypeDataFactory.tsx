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
    geographicLevel: GeographicLevel.Country,
    timePeriod: {
      startYear: '2014',
      startCode: 'HT6',
      endYear: '2017',
      endCode: 'HT6',
    },
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
    geographicLevel: GeographicLevel.Country,
    result: [
      {
        filters: ['1'],
        location: {
          country: {
            code: 'E92000001',
            name: 'England',
          },
          region: {
            code: '',
            name: '',
          },
          localAuthority: {
            code: '',
            old_code: '',
            name: '',
          },
          localAuthorityDistrict: {
            code: '',
            name: '',
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
        filters: ['1'],
        location: {
          country: {
            code: 'E92000001',
            name: 'England',
          },
          region: {
            code: '',
            name: '',
          },
          localAuthority: {
            code: '',
            old_code: '',
            name: '',
          },
          localAuthorityDistrict: {
            code: '',
            name: '',
          },
        },
        measures: {
          '28': '5',
          '26': '10',
          '23': '3',
        },
        timePeriod: '2015_HT6',
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

  labels: {
    '23_1_____': {
      value: '23_1',
      unit: '%',
      name: '23_1',
      label: 'Un auth rate',
    },
    '26_1_____': {
      value: '26_1',
      unit: '%',
      name: '26_1',
      label: 'All rate',
    },
    '28_1_____': {
      value: '28_1',
      unit: '%',
      name: '28_1',
      label: 'Auth rate',
    },
  },

  axes: {
    major: {
      name: 'major',
      type: 'major',
      groupBy: 'timePeriods',
      dataSets: [
        { indicator: '23', filters: ['1'] },
        { indicator: '26', filters: ['1'] },
        { indicator: '28', filters: ['1'] },
      ],
    },
    minor: {
      name: 'minor',
      type: 'minor',
      dataSets: [],
    },
  },
};

export const newChartsApiDataBlock: DataBlockProps = {
  type: 'datablock',
  id: 'test',
  dataBlockRequest: {
    subjectId: 1,
    geographicLevel: GeographicLevel.Country,
    timePeriod: {
      startYear: '2012',
      startCode: 'HT6',
      endYear: '2017',
      endCode: 'HT6',
    },
    indicators: ['23', '26', '28', '27'],
    filters: ['1', '2'],
  },
  showTables: false,
  charts: [
    {
      type: 'verticalbar',
      axes: {
        major: {
          name: 'major',
          type: 'major',
          groupBy: 'timePeriods',
          dataSets: [
            { indicator: '23', filters: ['1', '2'] },
            { indicator: '26', filters: ['1', '2'] },
            { indicator: '27', filters: ['1', '2'] },
            { indicator: '28', filters: ['1', '2'] },
          ],
        },
        minor: {
          name: 'minor',
          type: 'minor',
          dataSets: [],
        },
      },
      labels: {
        '23_1_2_____': {
          value: '23_1_2',
          unit: '%',
          name: '23_1_2',
          label: 'Unauthorised absence rate',
        },
        '26_1_2_____': {
          value: '26_1_2',
          unit: '%',
          name: '26_1_2',
          label: 'Overall absence rate',
        },
        '28_1_2_____': {
          value: '28_1_2',
          unit: '%',
          name: '28_1_2',
          label: 'Authorised absence rate',
        },
        '27_1_2_____': {
          value: '27_1_2',
          unit: '',
          name: '27_1_2',
          label: 'Absence Rate',
        },
      },
    },
  ],
};

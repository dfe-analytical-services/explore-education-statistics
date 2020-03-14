/* eslint-disable @typescript-eslint/camelcase */
import Features from '@common/modules/charts/components/__tests__/__data__/testLocationData';

import { ChartMetaData, ChartProps } from '@common/modules/charts/types/chart';
import { parseMetaData } from '@common/modules/charts/util/chartUtils';
import {
  DataBlockData,
  DataBlockMetadata,
  DataBlockResponse,
  GeographicLevel,
} from '@common/services/dataBlockService';

const data: DataBlockData = {
  publicationId: 'test',
  releaseDate: new Date(),
  releaseId: '1',
  subjectId: 1,
  geographicLevel: GeographicLevel.Country,
  result: [
    {
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
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
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
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
        '28': '1',
        '26': '4',
        '23': '-3',
      },
      timePeriod: '2015_HT6',
    },
  ],
};

const data2: DataBlockData = {
  publicationId: 'test',
  releaseDate: new Date(),
  releaseId: '1',
  subjectId: 1,
  geographicLevel: GeographicLevel.Country,
  result: [
    {
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
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
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
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
        '28': '1',
        '26': '4',
        '23': '-3',
      },
      timePeriod: '2015_HT6',
    },
    {
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
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
        '28': '0',
        '26': '5',
        '23': '-2',
      },
      timePeriod: '2016_HT6',
    },
  ],
};

const missingData: DataBlockData = {
  publicationId: 'test',
  releaseDate: new Date(),
  releaseId: '1',
  subjectId: 1,
  geographicLevel: GeographicLevel.Country,
  result: [
    {
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
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
        '23': '3',
      },
      timePeriod: '2013_HT6',
    },
    {
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
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
        '28': '1',
        '26': '10',
        '23': '-3',
      },
      timePeriod: '2015_HT6',
    },
    {
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
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
        '28': '6',
        '26': '4',
        '23': '-2',
      },
      timePeriod: '2016_HT6',
    },
  ],
};

const multipleData: DataBlockData = {
  publicationId: 'test',
  releaseDate: new Date(),
  releaseId: '1',
  subjectId: 1,
  geographicLevel: GeographicLevel.Country,
  result: [
    {
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
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
    {
      geographicLevel: GeographicLevel.Country,
      filters: ['1', '2'],
      location: {
        country: {
          code: 'S92000001',
          name: 'Scotland',
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
        '28': '10',
        '26': '20',
        '23': '4',
      },
      timePeriod: '2015_HT6',
    },
  ],
};

const labels = {
  '28': 'Authorised absence rate',
  '26': 'Overall absence rate',
  '23': 'Unauthorised absence rate',
};

const metaData: DataBlockMetadata = {
  geoJsonAvailable: true,
  publicationName: 'test',
  subjectName: 'test',
  footnotes: [],
  filters: {
    test: {
      totalValue: '',
      legend: '',
      hint: '',
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
  timePeriod: {
    '2014_HT6': {
      label: '2014/15',
      year: 2014,
      code: 'HT6',
    },
    '2015_HT6': {
      label: '2015/16',
      year: 2015,
      code: 'HT6',
    },
  },
  locations: {
    E92000001: {
      level: GeographicLevel.Country,
      value: 'E92000001',
      label: 'England',
      geoJson: [Features.E92000001],
    },
    S92000001: {
      level: GeographicLevel.Country,
      value: 'S92000001',
      label: 'Scotland',
      geoJson: [Features.S92000001],
    },
  },
};

const chartMetaData: ChartMetaData = parseMetaData(metaData) as ChartMetaData;

export const testChartPropsWithData1: ChartProps = {
  data,
  meta: chartMetaData,
  width: 900,
  height: 300,
  legend: 'top',
  labels: {
    '23_1_2_____': {
      label: metaData.indicators['23'].label,
      unit: '%',
      value: '23_1_2_____',
      name: '23_1_2_____',
      colour: '#ff0000',
    },
    '26_1_2_____': {
      label: metaData.indicators['26'].label,
      unit: '%',
      value: '26_1_2_____',
      name: '26_1_2_____',
      colour: '#00ff00',
    },
    '28_1_2_____': {
      label: metaData.indicators['28'].label,
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

export const testChartPropsWithData2: ChartProps = {
  data: data2,
  meta: {
    ...chartMetaData,
    timePeriod: {
      ...chartMetaData.timePeriod,
      '2016_HT6': {
        label: '2016/17',
        value: '2016_HT6',
      },
    },
  },
  width: 900,
  height: 300,
  labels: {
    '23_1_2_____': {
      label: metaData.indicators['23'].label,
      unit: '%',
      value: '23_1_2',
      name: '23_1_2',
    },
    '26_1_2_____': {
      label: metaData.indicators['26'].label,
      unit: '%',
      value: '26_1_2',
      name: '26_1_2',
    },
    '28_1_2_____': {
      label: metaData.indicators['28'].label,
      unit: '%',
      value: '28_1_2',
      name: '28_1_2',
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
  meta: chartMetaData,
  width: 900,
  height: 300,
  labels: {
    '23_1_2_____': {
      label: chartMetaData.indicators['23'].label,
      name: '23_1_2_____',
      unit: '%',
      value: '23_1_2_____',
    },
    '26_1_2_____': {
      label: chartMetaData.indicators['26'].label,
      name: '26_1_2_____',
      unit: '%',
      value: '26_1_2_____',
    },
    '28_1_2_____': {
      label: chartMetaData.indicators['28'].label,
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

export const testChartPropsWithMissingData: ChartProps = {
  data: missingData,
  width: 900,
  height: 300,
  meta: {
    ...chartMetaData,
    timePeriod: {
      '2013_HT6': {
        label: '2013/14',
        value: '2013_HT6',
      },
      '2014_HT6': {
        label: '2014/15',
        value: '2014_HT6',
      },
      '2015_HT6': {
        label: '2015/16',
        value: '2015_HT6',
      },
      '2016_HT6': {
        label: '2016/17',
        value: '2016_HT6',
      },
    },
  },
  labels: {
    '23_1_2_____': {
      label: metaData.indicators['23'].label,
      unit: '%',
      value: '23_1_2_____',
      name: '23_1_2_____',
    },
    '26_1_2_____': {
      label: metaData.indicators['26'].label,
      unit: '%',
      value: '26_1_2_____',
      name: '26_1_2_____',
    },
    '28_1_2_____': {
      label: metaData.indicators['28'].label,
      unit: '%',
      value: '28_1_2_____',
      name: '28_1_2_____',
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

export const testDataBlockResponse: DataBlockResponse = {
  ...data,
  metaData,
};

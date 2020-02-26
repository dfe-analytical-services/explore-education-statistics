/* eslint-disable @typescript-eslint/camelcase */
import {
  ChartMetaData,
  ChartProps,
  parseMetaData,
} from '@common/modules/find-statistics/components/charts/util/chartUtils';
import {
  DataBlockData,
  DataBlockMetadata,
  DataBlockResponse,
  GeographicLevel,
} from '@common/services/dataBlockService';

import Features from './testLocationData';

import testResponseData_23_26_28__1_2_LA_JSON from './testResponseData_23_26_28__1_2_LA.json';

const testResponseData_23_26_28__1_2_LA: DataBlockResponse = (testResponseData_23_26_28__1_2_LA_JSON as unknown) as DataBlockResponse;

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

const AbstractChartProps: ChartProps = {
  data,
  meta: chartMetaData,

  width: 900,
  height: 300,

  legend: 'top',
  legendHeight: '50',

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
      name: '23',
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
      name: 'minor',
      type: 'minor',
      title: '',
      visible: true,
      dataSets: [],
    },
  },
};

const AbstractChartProps2: ChartProps = {
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
      name: '23',
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
      name: 'minor',
      type: 'minor',
      title: '',
      visible: true,
      dataSets: [],
    },
  },
};

const AbstractMultipleChartProps: ChartProps = {
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
      name: '23',
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
    },
    minor: {
      name: 'minor',
      type: 'minor',
      title: '',
      visible: true,
      dataSets: [],
    },
  },
};

const testResponseData_23_26__1_2_LA: DataBlockResponse = {
  ...testResponseData_23_26_28__1_2_LA,

  result: testResponseData_23_26_28__1_2_LA.result.map(r => {
    return {
      ...r,
      measures: {
        '23': r.measures['23'],
        '26': r.measures['26'],
      },
    };
  }),

  metaData: {
    ...testResponseData_23_26_28__1_2_LA.metaData,
    indicators: {
      '23': testResponseData_23_26_28__1_2_LA.metaData.indicators['23'],
      '26': testResponseData_23_26_28__1_2_LA.metaData.indicators['26'],
    },
  },
};

const AbstractLargeDataChartPropsMeta = parseMetaData(
  testResponseData_23_26_28__1_2_LA.metaData,
) as ChartMetaData;
const AbstractLargeDataChartProps: ChartProps = {
  data: testResponseData_23_26_28__1_2_LA,
  meta: AbstractLargeDataChartPropsMeta,
  width: 900,
  height: 300,

  labels: {
    '2014_AY': {
      label: AbstractLargeDataChartPropsMeta.timePeriod['2014_AY'].label,
      value: '2014_AY',
    },
    '2015_AY': {
      label: AbstractLargeDataChartPropsMeta.timePeriod['2015_AY'].label,
      value: '2015_AY',
    },
    '23_1_2_____': {
      label: AbstractLargeDataChartPropsMeta.indicators['23'].label,
      unit: '%',
      value: '23_1_2_____',
      name: '23_1_2_____',
    },
    '26_1_2_____': {
      label: AbstractLargeDataChartPropsMeta.indicators['26'].label,
      unit: '%',
      value: '26_1_2_____',
      name: '26_1_2_____',
    },
    '28_1_2_____': {
      label: AbstractLargeDataChartPropsMeta.indicators['28'].label,
      unit: '%',
      value: '26_1_2_____',
      name: '26_1_2_____',
    },
  },

  axes: {
    major: {
      name: '23',
      type: 'major',
      groupBy: 'locations',
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
      name: 'minor',
      type: 'minor',
      title: '',
      visible: true,
      dataSets: [],
    },
  },
};

const AbstractLargeDataChartProps_smaller_datasetsMeta = parseMetaData(
  testResponseData_23_26_28__1_2_LA.metaData,
) as ChartMetaData;

const AbstractLargeDataChartProps_smaller_datasets: ChartProps = {
  data: testResponseData_23_26__1_2_LA,
  meta: AbstractLargeDataChartProps_smaller_datasetsMeta,

  width: 900,
  height: 300,

  labels: {
    '2014_AY': {
      label:
        AbstractLargeDataChartProps_smaller_datasetsMeta.timePeriod['2014_AY']
          .label,
      value: '2014_AY',
    },
    '2015_AY': {
      label:
        AbstractLargeDataChartProps_smaller_datasetsMeta.timePeriod['2015_AY']
          .label,
      value: '2015_AY',
    },
    '23_1_2_____': {
      label:
        AbstractLargeDataChartProps_smaller_datasetsMeta.indicators['23'].label,
      unit: '%',
      value: '23_1_2_____',
      name: '23_1_2_____',
      colour: '#285252',
    },
    '26_1_2_____': {
      label:
        AbstractLargeDataChartProps_smaller_datasetsMeta.indicators['26'].label,
      unit: '%',
      value: '26_1_2_____',
      name: '26_1_2_____',
      colour: '#572957',
    },
    '28_1_2_____': {
      label:
        AbstractLargeDataChartProps_smaller_datasetsMeta.indicators['28'].label,
      unit: '%',
      value: '28_1_2_____',
      name: '28_1_2_____',
      colour: '#454520',
    },
  },

  axes: {
    major: {
      name: '23',
      type: 'major',
      groupBy: 'locations',
      dataSets: [
        {
          indicator: '23',
          filters: ['1', '2'],
        },
        {
          indicator: '26',
          filters: ['1', '2'],
        },
      ],
      visible: true,
    },
    minor: {
      name: 'minor',
      type: 'minor',
      title: '',
      visible: true,
      dataSets: [],
    },
  },
};

const AbstractMissingDataChartProps: ChartProps = {
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
      name: '23',
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
      name: 'minor',
      type: 'minor',
      title: '',
      visible: true,
      dataSets: [],
    },
  },
};

const response: DataBlockResponse = {
  ...data,
  metaData,
};

export default {
  AbstractChartProps,
  AbstractChartProps2,
  AbstractMultipleChartProps,
  AbstractMissingDataChartProps,
  AbstractLargeDataChartProps,
  AbstractLargeDataChartProps_smaller_datasets,
  testBlockData: data,
  labels,
  response,
};

// eslint-disable-next-line @typescript-eslint/camelcase
import testResponseData_23_26_28__1_2_LA_JSON from '@common/modules/charts/components/__tests__/__data__/testResponseData_23_26_28__1_2_LA.json';
import { MapBlockProps } from '@common/modules/charts/components/MapBlock';
import { ChartMetaData } from '@common/modules/charts/types/chart';
import { parseMetaData } from '@common/modules/charts/util/chartUtils';
import { DataBlockResponse } from '@common/services/dataBlockService';

// eslint-disable-next-line @typescript-eslint/camelcase
const testDataBlockResponse: DataBlockResponse = (testResponseData_23_26_28__1_2_LA_JSON as unknown) as DataBlockResponse;

const largeMetaData = parseMetaData(
  testDataBlockResponse.metaData,
) as ChartMetaData;

export const testMapBlockProps: MapBlockProps = {
  data: testDataBlockResponse,
  meta: largeMetaData,
  width: 900,
  height: 300,
  labels: {
    '2014_AY': {
      label: largeMetaData.timePeriod['2014_AY'].label,
      value: '2014_AY',
    },
    '2015_AY': {
      label: largeMetaData.timePeriod['2015_AY'].label,
      value: '2015_AY',
    },
    '23_1_2_____': {
      label: largeMetaData.indicators['23'].label,
      unit: '%',
      value: '23_1_2_____',
      name: '23_1_2_____',
    },
    '26_1_2_____': {
      label: largeMetaData.indicators['26'].label,
      unit: '%',
      value: '26_1_2_____',
      name: '26_1_2_____',
    },
    '28_1_2_____': {
      label: largeMetaData.indicators['28'].label,
      unit: '%',
      value: '26_1_2_____',
      name: '26_1_2_____',
    },
  },
  axes: {
    major: {
      type: 'major',
      groupBy: 'locations',
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
  },
};

const largeMetaDataWithSamllerDataSets = parseMetaData(
  testDataBlockResponse.metaData,
) as ChartMetaData;

export const testMapBlockPropsWithSmallerDataSets: MapBlockProps = {
  data: {
    ...testDataBlockResponse,
    result: testDataBlockResponse.result.map(r => {
      return {
        ...r,
        measures: {
          '23': r.measures['23'],
          '26': r.measures['26'],
        },
      };
    }),
  },
  meta: largeMetaDataWithSamllerDataSets,
  width: 900,
  height: 300,
  labels: {
    '2014_AY': {
      label: largeMetaDataWithSamllerDataSets.timePeriod['2014_AY'].label,
      value: '2014_AY',
    },
    '2015_AY': {
      label: largeMetaDataWithSamllerDataSets.timePeriod['2015_AY'].label,
      value: '2015_AY',
    },
    '23_1_2_____': {
      label: largeMetaDataWithSamllerDataSets.indicators['23'].label,
      unit: '%',
      value: '23_1_2_____',
      name: '23_1_2_____',
      colour: '#285252',
    },
    '26_1_2_____': {
      label: largeMetaDataWithSamllerDataSets.indicators['26'].label,
      unit: '%',
      value: '26_1_2_____',
      name: '26_1_2_____',
      colour: '#572957',
    },
    '28_1_2_____': {
      label: largeMetaDataWithSamllerDataSets.indicators['28'].label,
      unit: '%',
      value: '28_1_2_____',
      name: '28_1_2_____',
      colour: '#454520',
    },
  },
  axes: {
    major: {
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
  },
};

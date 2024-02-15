import generateFeaturesAndDataGroups from '@common/modules/charts/components/utils/generateFeaturesAndDataGroups';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { MapDataSetCategory } from '@common/modules/charts/components/utils/createMapDataSetCategories';
import { DataSetCategoryConfig } from '@common/modules/charts/util/getDataSetCategoryConfigs';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import { MapFeatureCollection } from '@common/modules/charts/components/MapBlockInternal';
import { LegendDataGroup } from '@common/modules/charts/components/utils/generateLegendDataGroups';
import {
  testLocation1,
  testLocation2,
  testLocation3,
  testTimePeriod1,
  testDataGrouping,
  testDataSet1,
  testGeoJsonFeature1,
  testGeoJsonFeature2,
  testGeoJsonFeature3,
  testDataSet2,
  testDataSet3,
  testSubjectMeta,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import { Indicator } from '@common/modules/table-tool/types/filters';

describe('generateFeaturesAndDataGroups', () => {
  test('generates the correct features and data groups', () => {
    const testDataSetCategories: MapDataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 3.5,
          },
        },
        filter: testLocation1,
        geoJson: testGeoJsonFeature1,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 3,
          },
        },
        filter: testLocation2,
        geoJson: testGeoJsonFeature2,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 4,
          },
        },
        filter: testLocation3,
        geoJson: testGeoJsonFeature3,
      },
    ];
    const testSelectedDataSetConfig: DataSetCategoryConfig = {
      config: {
        label: 'Indicator 1 (Time period 1)',
        colour: '#12436D',
      },
      dataGrouping: testDataGrouping,
      dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
      dataSet: expandDataSet(testDataSet1, testSubjectMeta),
      rawDataSet: JSON.parse(generateDataSetKey(testDataSet1, testTimePeriod1)),
    };

    const result = generateFeaturesAndDataGroups({
      dataSetCategories: testDataSetCategories,
      selectedDataSetConfig: testSelectedDataSetConfig,
    });

    const expectedFeatures: MapFeatureCollection = {
      type: 'FeatureCollection',
      features: [
        {
          id: testLocation1.id,
          geometry: testGeoJsonFeature1.geometry,
          properties: {
            ...testGeoJsonFeature1.properties,
            colour: 'rgba(113, 142, 167, 1)',
            data: 3.5,
            dataSets: testDataSetCategories[0].dataSets,
          },
          type: 'Feature',
        },
        {
          id: testLocation2.id,
          geometry: testGeoJsonFeature2.geometry,
          properties: {
            ...testGeoJsonFeature2.properties,
            colour: 'rgba(208, 217, 226, 1)',
            data: 3,
            dataSets: testDataSetCategories[1].dataSets,
          },
          type: 'Feature',
        },
        {
          id: testLocation3.id,
          geometry: testGeoJsonFeature3.geometry,
          properties: {
            ...testGeoJsonFeature3.properties,
            colour: 'rgba(18, 67, 109, 1)',
            data: 4,
            dataSets: testDataSetCategories[2].dataSets,
          },
          type: 'Feature',
        },
      ],
    };
    expect(result.features).toEqual(expectedFeatures);

    const expectedDataGroups: LegendDataGroup[] = [
      {
        colour: 'rgba(208, 217, 226, 1)',
        decimalPlaces: 1,
        min: '3.0',
        max: '3.1',
        minRaw: 3,
        maxRaw: 3.1,
      },
      {
        colour: 'rgba(160, 180, 197, 1)',
        decimalPlaces: 1,
        min: '3.2',
        max: '3.3',
        minRaw: 3.2,
        maxRaw: 3.3,
      },
      {
        colour: 'rgba(113, 142, 167, 1)',
        decimalPlaces: 1,
        min: '3.4',
        max: '3.5',
        minRaw: 3.4,
        maxRaw: 3.5,
      },
      {
        colour: 'rgba(65, 105, 138, 1)',
        decimalPlaces: 1,
        min: '3.6',
        max: '3.7',
        minRaw: 3.6,
        maxRaw: 3.7,
      },
      {
        colour: 'rgba(18, 67, 109, 1)',
        decimalPlaces: 1,
        min: '3.8',
        max: '4.0',
        minRaw: 3.8,
        maxRaw: 4,
      },
    ];
    expect(result.dataGroups).toEqual(expectedDataGroups);
  });

  test('generates dataGroups correctly with 1 decimal place', async () => {
    const testDataSetCategories: MapDataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 3.5123,
          },
        },
        filter: testLocation1,
        geoJson: testGeoJsonFeature1,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 3.012,
          },
        },
        filter: testLocation2,
        geoJson: testGeoJsonFeature2,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 4.009,
          },
        },
        filter: testLocation3,
        geoJson: testGeoJsonFeature3,
      },
    ];

    const testSubjectMeta1DP = {
      ...testSubjectMeta,
      indicators: [
        new Indicator({
          label: 'Indicator 1',
          name: 'indicator-1-name',
          unit: '%',
          value: 'indicator-1',
          decimalPlaces: 1,
        }),
      ],
    };

    const testSelectedDataSetConfig: DataSetCategoryConfig = {
      config: {
        label: 'Indicator 1 (Time period 1)',
        colour: '#12436D',
      },
      dataGrouping: testDataGrouping,
      dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
      dataSet: expandDataSet(testDataSet1, testSubjectMeta1DP),
      rawDataSet: JSON.parse(generateDataSetKey(testDataSet1, testTimePeriod1)),
    };

    const result = generateFeaturesAndDataGroups({
      dataSetCategories: testDataSetCategories,
      selectedDataSetConfig: testSelectedDataSetConfig,
    });

    const expectedDataGroups: LegendDataGroup[] = [
      {
        colour: 'rgba(208, 217, 226, 1)',
        decimalPlaces: 1,
        min: '3.0%',
        max: '3.1%',
        minRaw: 3,
        maxRaw: 3.1,
      },
      {
        colour: 'rgba(160, 180, 197, 1)',
        decimalPlaces: 1,
        min: '3.2%',
        max: '3.3%',
        minRaw: 3.2,
        maxRaw: 3.3,
      },
      {
        colour: 'rgba(113, 142, 167, 1)',
        decimalPlaces: 1,
        min: '3.4%',
        max: '3.5%',
        minRaw: 3.4,
        maxRaw: 3.5,
      },
      {
        colour: 'rgba(65, 105, 138, 1)',
        decimalPlaces: 1,
        min: '3.6%',
        max: '3.7%',
        minRaw: 3.6,
        maxRaw: 3.7,
      },
      {
        colour: 'rgba(18, 67, 109, 1)',
        decimalPlaces: 1,
        min: '3.8%',
        max: '4.0%',
        minRaw: 3.8,
        maxRaw: 4,
      },
    ];

    expect(result.dataGroups).toEqual(expectedDataGroups);
  });

  test('generates dataGroups correctly with 3 decimal places', async () => {
    const testDataSetCategories: MapDataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 3.5123,
          },
        },
        filter: testLocation1,
        geoJson: testGeoJsonFeature1,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 3.012,
          },
        },
        filter: testLocation2,
        geoJson: testGeoJsonFeature2,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 4.009,
          },
        },
        filter: testLocation3,
        geoJson: testGeoJsonFeature3,
      },
    ];

    const testSubjectMeta3DP = {
      ...testSubjectMeta,
      indicators: [
        new Indicator({
          label: 'Indicator 1',
          name: 'indicator-1-name',
          unit: '%',
          value: 'indicator-1',
          decimalPlaces: 3,
        }),
      ],
    };

    const testSelectedDataSetConfig: DataSetCategoryConfig = {
      config: {
        label: 'Indicator 1 (Time period 1)',
        colour: '#12436D',
      },
      dataGrouping: testDataGrouping,
      dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
      dataSet: expandDataSet(testDataSet1, testSubjectMeta3DP),
      rawDataSet: JSON.parse(generateDataSetKey(testDataSet1, testTimePeriod1)),
    };

    const result = generateFeaturesAndDataGroups({
      dataSetCategories: testDataSetCategories,
      selectedDataSetConfig: testSelectedDataSetConfig,
    });

    const expectedDataGroups: LegendDataGroup[] = [
      {
        colour: 'rgba(208, 217, 226, 1)',
        decimalPlaces: 3,
        min: '3.012%',
        max: '3.211%',
        minRaw: 3.012,
        maxRaw: 3.211,
      },
      {
        colour: 'rgba(160, 180, 197, 1)',
        decimalPlaces: 3,
        min: '3.212%',
        max: '3.411%',
        minRaw: 3.212,
        maxRaw: 3.411,
      },
      {
        colour: 'rgba(113, 142, 167, 1)',
        decimalPlaces: 3,
        min: '3.412%',
        max: '3.611%',
        minRaw: 3.412,
        maxRaw: 3.611,
      },
      {
        colour: 'rgba(65, 105, 138, 1)',
        decimalPlaces: 3,
        min: '3.612%',
        max: '3.811%',
        minRaw: 3.612,
        maxRaw: 3.811,
      },
      {
        colour: 'rgba(18, 67, 109, 1)',
        decimalPlaces: 3,
        min: '3.812%',
        max: '4.009%',
        minRaw: 3.812,
        maxRaw: 4.009,
      },
    ];

    expect(result.dataGroups).toEqual(expectedDataGroups);
  });

  test('ensure values with decimal places are assigned the correct colour when decimal places is set to 0', async () => {
    const testDataSetCategories: MapDataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 16388.4329758565,
          },
        },
        filter: testLocation1,
        geoJson: testGeoJsonFeature1,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: -1395.33948144574,
          },
        },
        filter: testLocation2,
        geoJson: testGeoJsonFeature2,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 6059.30533950346,
          },
        },
        filter: testLocation3,
        geoJson: testGeoJsonFeature3,
      },
    ];

    const testSubjectMeta0DP = {
      ...testSubjectMeta,
      indicators: [
        new Indicator({
          label: 'Indicator 1',
          name: 'indicator-1-name',
          unit: '£',
          value: 'indicator-1',
          decimalPlaces: 0,
        }),
      ],
    };

    const testSelectedDataSetConfig: DataSetCategoryConfig = {
      config: {
        label: 'Indicator 1 (Time period 1)',
        colour: '#4763a5',
      },
      dataGrouping: testDataGrouping,
      dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
      dataSet: expandDataSet(testDataSet1, testSubjectMeta0DP),
      rawDataSet: JSON.parse(generateDataSetKey(testDataSet1, testTimePeriod1)),
    };

    const result = generateFeaturesAndDataGroups({
      dataSetCategories: testDataSetCategories,
      selectedDataSetConfig: testSelectedDataSetConfig,
    });

    const expectedDataGroups: LegendDataGroup[] = [
      {
        colour: 'rgba(218, 224, 237, 1)',
        decimalPlaces: 0,
        min: '-£1,395',
        max: '£2,161',
        minRaw: -1395,
        maxRaw: 2161,
      },
      {
        colour: 'rgba(181, 193, 219, 1)',
        decimalPlaces: 0,
        min: '£2,162',
        max: '£5,718',
        minRaw: 2162,
        maxRaw: 5718,
      },
      {
        colour: 'rgba(145, 161, 201, 1)',
        decimalPlaces: 0,
        min: '£5,719',
        max: '£9,275',
        minRaw: 5719,
        maxRaw: 9275,
      },
      {
        colour: 'rgba(108, 130, 183, 1)',
        decimalPlaces: 0,
        min: '£9,276',
        max: '£12,832',
        minRaw: 9276,
        maxRaw: 12832,
      },
      {
        colour: 'rgba(71, 99, 165, 1)',
        decimalPlaces: 0,
        min: '£12,833',
        max: '£16,388',
        minRaw: 12833,
        maxRaw: 16388,
      },
    ];

    expect(result.dataGroups).toEqual(expectedDataGroups);

    // expect(legendColours[0].style.backgroundColor).toBe(
    //   'rgba(218, 224, 237, 1)',
    // );
    // expect(legendColours[1].style.backgroundColor).toBe(
    //   'rgba(181, 193, 219, 1)',
    // );
    // expect(legendColours[2].style.backgroundColor).toBe(
    //   'rgba(145, 161, 201, 1)',
    // );
    // expect(legendColours[3].style.backgroundColor).toBe(
    //   'rgba(108, 130, 183, 1)',
    // );
    // expect(legendColours[4].style.backgroundColor).toBe('rgba(71, 99, 165, 1)');
  });
});

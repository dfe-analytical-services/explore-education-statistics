import {
  testDataGrouping,
  testDataSet1,
  testDataSet2,
  testDataSet3,
  testGeoJsonFeature1,
  testGeoJsonFeature2,
  testGeoJsonFeature3,
  testLocation1,
  testLocation2,
  testLocation3,
  testSubjectMeta,
  testTimePeriod1,
} from '@common/modules/charts/components/__tests__/__data__/testMapBlockData';
import { MapFeatureCollection } from '@common/modules/charts/components/MapBlock';
import { MapDataSetCategory } from '@common/modules/charts/components/utils/createMapDataSetCategories';
import generateFeaturesAndDataGroups from '@common/modules/charts/components/utils/generateFeaturesAndDataGroups';
import {
  MapCategoricalData,
  MapLegendItem,
} from '@common/modules/charts/types/chart';
import expandDataSet from '@common/modules/charts/util/expandDataSet';
import generateDataSetKey from '@common/modules/charts/util/generateDataSetKey';
import { MapDataSetCategoryConfig } from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import { Indicator } from '@common/modules/table-tool/types/filters';

describe('generateFeaturesAndDataGroups', () => {
  test('generates the correct features and legend items', () => {
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
    const testSelectedDataSetConfig: MapDataSetCategoryConfig = {
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

    const expectedLegendItems: MapLegendItem[] = [
      {
        colour: 'rgba(208, 217, 226, 1)',
        value: '3.0 to 3.1',
      },
      {
        colour: 'rgba(160, 180, 197, 1)',
        value: '3.2 to 3.3',
      },
      {
        colour: 'rgba(113, 142, 167, 1)',
        value: '3.4 to 3.5',
      },
      {
        colour: 'rgba(65, 105, 138, 1)',
        value: '3.6 to 3.7',
      },
      {
        colour: 'rgba(18, 67, 109, 1)',
        value: '3.8 to 4.0',
      },
    ];
    expect(result.legendItems).toEqual(expectedLegendItems);
  });

  test('generates legendItems correctly with 1 decimal place', async () => {
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

    const testSelectedDataSetConfig: MapDataSetCategoryConfig = {
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

    const expectedLegendItems: MapLegendItem[] = [
      {
        colour: 'rgba(208, 217, 226, 1)',
        value: '3.0% to 3.1%',
      },
      {
        colour: 'rgba(160, 180, 197, 1)',
        value: '3.2% to 3.3%',
      },
      {
        colour: 'rgba(113, 142, 167, 1)',
        value: '3.4% to 3.5%',
      },
      {
        colour: 'rgba(65, 105, 138, 1)',
        value: '3.6% to 3.7%',
      },
      {
        colour: 'rgba(18, 67, 109, 1)',
        value: '3.8% to 4.0%',
      },
    ];

    expect(result.legendItems).toEqual(expectedLegendItems);
  });

  test('generates legendItems correctly with 3 decimal places', async () => {
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

    const testSelectedDataSetConfig: MapDataSetCategoryConfig = {
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

    const expectedLegendItems: MapLegendItem[] = [
      {
        colour: 'rgba(208, 217, 226, 1)',
        value: '3.012% to 3.211%',
      },
      {
        colour: 'rgba(160, 180, 197, 1)',
        value: '3.212% to 3.411%',
      },
      {
        colour: 'rgba(113, 142, 167, 1)',
        value: '3.412% to 3.611%',
      },
      {
        colour: 'rgba(65, 105, 138, 1)',
        value: '3.612% to 3.811%',
      },
      {
        colour: 'rgba(18, 67, 109, 1)',
        value: '3.812% to 4.009%',
      },
    ];

    expect(result.legendItems).toEqual(expectedLegendItems);
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

    const testSelectedDataSetConfig: MapDataSetCategoryConfig = {
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

    const expectedLegendItems: MapLegendItem[] = [
      {
        colour: 'rgba(218, 224, 237, 1)',
        value: '-£1,395 to £2,161',
      },
      {
        colour: 'rgba(181, 193, 219, 1)',
        value: '£2,162 to £5,718',
      },
      {
        colour: 'rgba(145, 161, 201, 1)',
        value: '£5,719 to £9,275',
      },
      {
        colour: 'rgba(108, 130, 183, 1)',
        value: '£9,276 to £12,832',
      },
      {
        colour: 'rgba(71, 99, 165, 1)',
        value: '£12,833 to £16,388',
      },
    ];

    expect(result.legendItems).toEqual(expectedLegendItems);
  });

  describe('Categorical data', () => {
    const testCategoricalDataSetCategories: MapDataSetCategory[] = [
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet1,
            value: 'large',
          },
        },
        filter: testLocation1,
        geoJson: testGeoJsonFeature1,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet2,
            value: 'small',
          },
        },
        filter: testLocation2,
        geoJson: testGeoJsonFeature2,
      },
      {
        dataSets: {
          [generateDataSetKey(testDataSet1, testTimePeriod1)]: {
            dataSet: testDataSet3,
            value: 'medium',
          },
        },
        filter: testLocation3,
        geoJson: testGeoJsonFeature3,
      },
    ];

    test('generates the correct features and legend items for categorical data', () => {
      const testSelectedDataSetConfig: MapDataSetCategoryConfig = {
        config: {
          label: 'Indicator 1 (Time period 1)',
          colour: '#12436D',
        },
        categoricalDataConfig: [
          { colour: '#12436D', value: 'large' },
          { colour: '#28A197', value: 'small' },
          { colour: '#801650', value: 'medium' },
        ],
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      };

      const result = generateFeaturesAndDataGroups({
        dataSetCategories: testCategoricalDataSetCategories,
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
              colour: '#12436D',
              data: 'large',
              dataSets: testCategoricalDataSetCategories[0].dataSets,
            },
            type: 'Feature',
          },
          {
            id: testLocation2.id,
            geometry: testGeoJsonFeature2.geometry,
            properties: {
              ...testGeoJsonFeature2.properties,
              colour: '#28A197',
              data: 'small',
              dataSets: testCategoricalDataSetCategories[1].dataSets,
            },
            type: 'Feature',
          },
          {
            id: testLocation3.id,
            geometry: testGeoJsonFeature3.geometry,
            properties: {
              ...testGeoJsonFeature3.properties,
              colour: '#801650',
              data: 'medium',
              dataSets: testCategoricalDataSetCategories[2].dataSets,
            },
            type: 'Feature',
          },
        ],
      };
      expect(result.features).toEqual(expectedFeatures);

      const expectedLegendItems: MapLegendItem[] = [
        {
          colour: '#12436D',
          value: 'large',
        },
        {
          colour: '#28A197',
          value: 'small',
        },
        {
          colour: '#801650',
          value: 'medium',
        },
      ];
      expect(result.legendItems).toEqual(expectedLegendItems);
    });

    test('generates the correct features and legend items for categorical data when sequential category colours is selected', () => {
      const testSelectedDataSetConfig: MapDataSetCategoryConfig = {
        config: {
          label: 'Indicator 1 (Time period 1)',
          colour: '#12436D',
          sequentialCategoryColours: true,
        },
        categoricalDataConfig: [
          { colour: '#12436D', value: 'small' },
          { colour: '#28A197', value: 'medium' },
          { colour: '#801650', value: 'large' },
        ],
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      };

      const result = generateFeaturesAndDataGroups({
        dataSetCategories: testCategoricalDataSetCategories,
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
              colour: 'rgba(18, 67, 109, 1)',
              data: 'large',
              dataSets: testCategoricalDataSetCategories[0].dataSets,
            },
            type: 'Feature',
          },
          {
            id: testLocation2.id,
            geometry: testGeoJsonFeature2.geometry,
            properties: {
              ...testGeoJsonFeature2.properties,
              colour: 'rgba(176, 192, 206, 1)',
              data: 'small',
              dataSets: testCategoricalDataSetCategories[1].dataSets,
            },
            type: 'Feature',
          },
          {
            id: testLocation3.id,
            geometry: testGeoJsonFeature3.geometry,
            properties: {
              ...testGeoJsonFeature3.properties,
              colour: 'rgba(97, 130, 158, 1)',
              data: 'medium',
              dataSets: testCategoricalDataSetCategories[2].dataSets,
            },
            type: 'Feature',
          },
        ],
      };
      expect(result.features).toEqual(expectedFeatures);

      const expectedLegendItems: MapLegendItem[] = [
        {
          colour: 'rgba(176, 192, 206, 1)',
          value: 'small',
        },
        {
          colour: 'rgba(97, 130, 158, 1)',
          value: 'medium',
        },
        {
          colour: 'rgba(18, 67, 109, 1)',
          value: 'large',
        },
      ];
      expect(result.legendItems).toEqual(expectedLegendItems);
    });

    test('generates the correct features and legend items for categorical data using deprecatedCategoricalDataConfig', () => {
      const testSelectedDataSetConfig: MapDataSetCategoryConfig = {
        config: {
          label: 'Indicator 1 (Time period 1)',
          colour: '#12436D',
        },
        categoricalDataConfig: [],
        dataGrouping: testDataGrouping,
        dataKey: generateDataSetKey(testDataSet1, testTimePeriod1),
        dataSet: expandDataSet(testDataSet1, testSubjectMeta),
        rawDataSet: JSON.parse(
          generateDataSetKey(testDataSet1, testTimePeriod1),
        ),
      };

      const testDeprecatedCategoricalDataConfig: MapCategoricalData[] = [
        {
          value: 'large',
          colour: '#12436D',
        },
        {
          value: 'small',
          colour: '#28A197',
        },

        {
          value: 'medium',
          colour: '#801650',
        },
      ];

      const result = generateFeaturesAndDataGroups({
        deprecatedCategoricalDataConfig: testDeprecatedCategoricalDataConfig,
        dataSetCategories: testCategoricalDataSetCategories,
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
              colour: '#12436D',
              data: 'large',
              dataSets: testCategoricalDataSetCategories[0].dataSets,
            },
            type: 'Feature',
          },
          {
            id: testLocation2.id,
            geometry: testGeoJsonFeature2.geometry,
            properties: {
              ...testGeoJsonFeature2.properties,
              colour: '#28A197',
              data: 'small',
              dataSets: testCategoricalDataSetCategories[1].dataSets,
            },
            type: 'Feature',
          },
          {
            id: testLocation3.id,
            geometry: testGeoJsonFeature3.geometry,
            properties: {
              ...testGeoJsonFeature3.properties,
              colour: '#801650',
              data: 'medium',
              dataSets: testCategoricalDataSetCategories[2].dataSets,
            },
            type: 'Feature',
          },
        ],
      };
      expect(result.features).toEqual(expectedFeatures);

      const expectedLegendItems: MapLegendItem[] = [
        {
          colour: '#12436D',
          value: 'large',
        },
        {
          colour: '#28A197',
          value: 'small',
        },
        {
          colour: '#801650',
          value: 'medium',
        },
      ];
      expect(result.legendItems).toEqual(expectedLegendItems);
    });
  });
});

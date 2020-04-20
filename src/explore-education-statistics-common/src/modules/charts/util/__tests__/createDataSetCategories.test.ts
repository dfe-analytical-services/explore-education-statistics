import { AxisConfiguration } from '@common/modules/charts/types/chart';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import { TableDataResponse } from '@common/services/tableBuilderService';

describe('createDataSetCategories', () => {
  const testTable: TableDataResponse = {
    subjectMeta: {
      geoJsonAvailable: false,
      publicationName: '',
      subjectName: '',
      filters: {
        Characteristic: {
          totalValue: '',
          hint: 'Filter by pupil characteristic',
          legend: 'Characteristic',
          name: 'characteristic',
          options: {
            Gender: {
              label: 'Gender',
              options: [
                {
                  label: 'Gender female',
                  value: 'gender-female',
                },
                {
                  label: 'Gender male',
                  value: 'gender-male',
                },
              ],
            },
          },
        },
        SchoolType: {
          totalValue: '',
          hint: 'Filter by school type',
          legend: 'School type',
          name: 'school_type',
          options: {
            Default: {
              label: 'Default',
              options: [
                {
                  label: 'State-funded primary',
                  value: 'state-funded-primary',
                },
              ],
            },
          },
        },
      },
      footnotes: [],
      indicators: [
        {
          label: 'Fixed period exclusion rate',
          unit: '%',
          value: 'exclusion-rate',
          name: 'exclusion_rate',
        },
      ],
      locations: [
        {
          level: 'country',
          geoJson: [],
          label: 'England',
          value: 'E92000001',
        },
      ],
      boundaryLevels: [],
      timePeriodRange: [
        {
          code: 'AY',
          label: '2012/13',
          year: 2012,
        },
        {
          code: 'AY',
          label: '2013/14',
          year: 2013,
        },
        {
          code: 'AY',
          label: '2014/15',
          year: 2014,
        },
        {
          code: 'AY',
          label: '2015/16',
          year: 2015,
        },
      ],
    },
    results: [
      {
        filters: ['gender-female', 'state-funded-primary'],
        geographicLevel: 'Country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: { 'exclusion-rate': '0.29' },
        timePeriod: '2015_AY',
      },
      {
        filters: ['gender-female', 'state-funded-primary'],
        geographicLevel: 'Country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: { 'exclusion-rate': '0.25' },
        timePeriod: '2014_AY',
      },
      {
        filters: ['state-funded-primary', 'gender-male'],
        geographicLevel: 'Country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: { 'exclusion-rate': '1.54' },
        timePeriod: '2012_AY',
      },
      {
        filters: ['state-funded-primary', 'gender-male'],
        geographicLevel: 'Country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: { 'exclusion-rate': '1.78' },
        timePeriod: '2013_AY',
      },
      {
        filters: ['gender-female', 'state-funded-primary'],
        geographicLevel: 'Country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: { 'exclusion-rate': '0.22' },
        timePeriod: '2013_AY',
      },
      {
        filters: ['gender-female', 'state-funded-primary'],
        geographicLevel: 'Country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: { 'exclusion-rate': '0.19' },
        timePeriod: '2012_AY',
      },
      {
        filters: ['state-funded-primary', 'gender-male'],
        geographicLevel: 'Country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: { 'exclusion-rate': '1.92' },
        timePeriod: '2014_AY',
      },
      {
        filters: ['state-funded-primary', 'gender-male'],
        geographicLevel: 'Country',
        location: { country: { code: 'E92000001', name: 'England' } },
        measures: { 'exclusion-rate': '2.09' },
        timePeriod: '2015_AY',
      },
    ],
  };

  test('returns chart data categorised by time periods', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'timePeriod',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'exclusion-rate',
          filters: ['gender-female', 'state-funded-primary'],
          config: {
            label: 'Test label 1',
          },
        },
        {
          indicator: 'exclusion-rate',
          filters: ['gender-male', 'state-funded-primary'],
          config: {
            label: 'Test label 2',
          },
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(4);

    expect(dataSetCategories[0].filter.label).toBe('2012/13');

    const dataSet1 = Object.values(dataSetCategories[0].dataSets);
    expect(dataSet1).toHaveLength(2);
    expect(dataSet1[0].value).toBe(0.19);
    expect(dataSet1[1].value).toBe(1.54);

    expect(dataSetCategories[1].filter.label).toBe('2013/14');

    const dataSet2 = Object.values(dataSetCategories[1].dataSets);
    expect(dataSet2).toHaveLength(2);
    expect(dataSet2[0].value).toBe(0.22);
    expect(dataSet2[1].value).toBe(1.78);

    expect(dataSetCategories[2].filter.label).toBe('2014/15');

    const dataSet3 = Object.values(dataSetCategories[2].dataSets);
    expect(dataSet3).toHaveLength(2);
    expect(dataSet3[0].value).toBe(0.25);
    expect(dataSet3[1].value).toBe(1.92);

    expect(dataSetCategories[3].filter.label).toBe('2015/16');

    const dataSet4 = Object.values(dataSetCategories[3].dataSets);
    expect(dataSet4).toHaveLength(2);
    expect(dataSet4[0].value).toBe(0.29);
    expect(dataSet4[1].value).toBe(2.09);

    expect(dataSetCategories).toMatchSnapshot();
  });

  test('returns chart data categorised by locations', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'locations',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'exclusion-rate',
          filters: ['gender-female', 'state-funded-primary'],
          config: {
            label: 'Test label 1',
          },
        },
        {
          indicator: 'exclusion-rate',
          filters: ['gender-male', 'state-funded-primary'],
          config: {
            label: 'Test label 2',
          },
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(1);

    expect(dataSetCategories[0].filter.label).toBe('England');

    const dataSet1 = Object.values(dataSetCategories[0].dataSets);
    expect(dataSet1).toHaveLength(8);
    expect(dataSet1[0].value).toBe(0.19);
    expect(dataSet1[1].value).toBe(0.22);
    expect(dataSet1[2].value).toBe(0.25);
    expect(dataSet1[3].value).toBe(0.29);
    expect(dataSet1[4].value).toBe(1.54);
    expect(dataSet1[5].value).toBe(1.78);
    expect(dataSet1[6].value).toBe(1.92);
    expect(dataSet1[7].value).toBe(2.09);

    expect(dataSetCategories).toMatchSnapshot();
  });

  test('returns chart data categorised by indicators', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'indicators',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'exclusion-rate',
          filters: ['gender-female', 'state-funded-primary'],
          config: {
            label: 'Test label 1',
          },
        },
        {
          indicator: 'exclusion-rate',
          filters: ['gender-male', 'state-funded-primary'],
          config: {
            label: 'Test label 2',
          },
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(1);

    expect(dataSetCategories[0].filter.label).toBe(
      'Fixed period exclusion rate',
    );

    const dataSet1 = Object.values(dataSetCategories[0].dataSets);
    expect(dataSet1).toHaveLength(8);
    expect(dataSet1[0].value).toBe(0.19);
    expect(dataSet1[1].value).toBe(0.22);
    expect(dataSet1[2].value).toBe(0.25);
    expect(dataSet1[3].value).toBe(0.29);
    expect(dataSet1[4].value).toBe(1.54);
    expect(dataSet1[5].value).toBe(1.78);
    expect(dataSet1[6].value).toBe(1.92);
    expect(dataSet1[7].value).toBe(2.09);

    expect(dataSetCategories).toMatchSnapshot();
  });

  test('returns chart data categorised by filters', () => {
    const axisConfiguration: AxisConfiguration = {
      type: 'major',
      groupBy: 'filters',
      sortBy: 'name',
      sortAsc: true,
      dataSets: [
        {
          indicator: 'exclusion-rate',
          filters: ['gender-female', 'state-funded-primary'],
          config: {
            label: 'Test label 1',
          },
        },
        {
          indicator: 'exclusion-rate',
          filters: ['gender-male', 'state-funded-primary'],
          config: {
            label: 'Test label 2',
          },
        },
      ],
      referenceLines: [],
      visible: true,
      unit: '',
      min: 0,
    };

    const fullTable = mapFullTable(testTable);

    const dataSetCategories = createDataSetCategories(
      axisConfiguration,
      fullTable.results,
      fullTable.subjectMeta,
    );

    expect(dataSetCategories).toHaveLength(3);

    expect(dataSetCategories[0].filter.label).toBe('Gender female');

    const dataSet1 = Object.values(dataSetCategories[0].dataSets);
    expect(dataSet1).toHaveLength(4);
    expect(dataSet1[0].value).toBe(0.19);
    expect(dataSet1[1].value).toBe(0.22);
    expect(dataSet1[2].value).toBe(0.25);
    expect(dataSet1[3].value).toBe(0.29);

    expect(dataSetCategories[1].filter.label).toBe('Gender male');

    const dataSet2 = Object.values(dataSetCategories[1].dataSets);
    expect(dataSet2).toHaveLength(4);
    expect(dataSet2[0].value).toBe(1.54);
    expect(dataSet2[1].value).toBe(1.78);
    expect(dataSet2[2].value).toBe(1.92);
    expect(dataSet2[3].value).toBe(2.09);

    expect(dataSetCategories[2].filter.label).toBe('State-funded primary');

    const dataSet3 = Object.values(dataSetCategories[2].dataSets);
    expect(dataSet3).toHaveLength(8);
    expect(dataSet3[0].value).toBe(0.19);
    expect(dataSet3[1].value).toBe(0.22);
    expect(dataSet3[2].value).toBe(0.25);
    expect(dataSet3[3].value).toBe(0.29);
    expect(dataSet3[4].value).toBe(1.54);
    expect(dataSet3[5].value).toBe(1.78);
    expect(dataSet3[6].value).toBe(1.92);
    expect(dataSet3[7].value).toBe(2.09);

    expect(dataSetCategories).toMatchSnapshot();
  });
});

import {
  AxisConfiguration,
  AxisType,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import { DataSetConfiguration } from '@common/modules/charts/types/dataSet';
import produce from 'immer';
import {
  ChartBuilderActions,
  chartBuilderReducer,
  ChartBuilderState,
  ChartOptions,
} from '../chartBuilderReducer';

describe('chartBuilderReducer', () => {
  const testChartDefinition: ChartDefinition = {
    type: 'line',
    name: 'Line',
    capabilities: {
      dataSymbols: true,
      stackable: false,
      lineStyle: true,
      gridLines: true,
      canSize: true,
      fixedAxisGroupBy: false,
      hasAxes: true,
      hasReferenceLines: true,
      hasLegend: true,
      requiresGeoJson: false,
    },
    options: {
      defaults: {
        height: 300,
        legend: 'top',
      },
    },
    data: [
      {
        type: 'line',
        title: 'Line',
        entryCount: 'multiple',
        targetAxis: 'xaxis',
      },
    ],
    axes: {
      major: {
        id: 'xaxis',
        title: 'X Axis',
        type: 'major',
        defaults: {
          groupBy: 'timePeriod',
          min: 0,
          showGrid: true,
          size: 50,
          sortAsc: true,
          sortBy: 'name',
          tickConfig: 'default',
          tickSpacing: 1,
          visible: true,
          unit: '',
        },
      },
      minor: {
        id: 'yaxis',
        title: 'Y Axis',
        type: 'minor',
      },
    },
  };

  describe('UPDATE_CHART_DEFINITION', () => {
    const initialState: ChartBuilderState = {
      axes: {},
      options: {
        height: 300,
      },
    };

    test('sets the `definition` to the payload', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      } as ChartBuilderActions);

      expect(nextState.definition).toEqual(testChartDefinition);
    });

    test('sets `options` with defaults from the definition', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      } as ChartBuilderActions);

      expect(nextState.options).toEqual<ChartOptions>({
        height: 300,
        legend: 'top',
        title: '',
      });
    });

    test('does not override `options` that already exist ', () => {
      const initialStateWithOptions: ChartBuilderState = {
        ...initialState,
        options: {
          height: 400,
          title: 'Some title',
          legend: 'bottom',
        },
      };

      const nextState = produce(chartBuilderReducer)(initialStateWithOptions, {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      } as ChartBuilderActions);

      expect(nextState.options).toEqual<ChartOptions>({
        height: 400,
        title: 'Some title',
        legend: 'bottom',
      });
    });

    test('overrides `options` if definition has constants', () => {
      const initialStateWithOptions: ChartBuilderState = {
        ...initialState,
        options: {
          height: 400,
          title: 'Some title',
          legend: 'bottom',
        },
      };

      const testChartDefinitionWithConstants = {
        ...testChartDefinition,
        options: {
          defaults: testChartDefinition.options.defaults,
          constants: {
            height: 600,
            legend: 'top',
          },
        },
      };

      const nextState = produce(chartBuilderReducer)(initialStateWithOptions, {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinitionWithConstants,
      } as ChartBuilderActions);

      expect(nextState.options).toEqual<ChartOptions>({
        height: 600,
        title: 'Some title',
        legend: 'top',
      });
    });

    test('sets `axes` with defaults', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      } as ChartBuilderActions);

      expect(nextState.axes.major).toEqual<AxisConfiguration>({
        dataSets: [],
        groupBy: 'timePeriod',
        min: 0,
        referenceLines: [],
        showGrid: true,
        size: 50,
        sortAsc: true,
        sortBy: 'name',
        tickConfig: 'default',
        tickSpacing: 1,
        type: 'major',
        visible: true,
        unit: '',
      });

      expect(nextState.axes.minor).toEqual<AxisConfiguration>({
        dataSets: [],
        referenceLines: [],
        type: 'minor',
        visible: true,
      });
    });

    test('does not override `axes` options that already exist', () => {
      const initialStateWithAxes: ChartBuilderState = {
        ...initialState,
        axes: {
          major: {
            type: 'major',
            dataSets: [],
            referenceLines: [],
            groupBy: 'filters',
            visible: false,
            sortBy: 'something',
          },
          minor: {
            type: 'minor',
            dataSets: [],
            referenceLines: [],
            visible: false,
            sortBy: 'something else',
          },
        },
      };

      const nextState = produce(chartBuilderReducer)(initialStateWithAxes, {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      } as ChartBuilderActions);

      expect(nextState.axes.major).toMatchObject<Partial<AxisConfiguration>>({
        dataSets: [],
        groupBy: 'filters',
        sortBy: 'something',
        visible: false,
      });

      expect(nextState.axes.minor).toMatchObject<Partial<AxisConfiguration>>({
        sortBy: 'something else',
        visible: false,
      });
    });

    test('overrides `axes` options if definition has constants', () => {
      const testChartDefinitionWithConstants: ChartDefinition = {
        ...testChartDefinition,
        axes: {
          ...testChartDefinition.axes,
          major: {
            ...testChartDefinition.axes.major,
            constants: {
              visible: true,
              sortBy: 'overriding value',
            },
          } as ChartDefinition['axes']['major'],
        },
      };

      const initialStateWithAxes: ChartBuilderState = {
        ...initialState,
        axes: {
          major: {
            type: 'major',
            dataSets: [],
            referenceLines: [],
            visible: false,
            sortBy: 'override me',
          },
        },
      };

      const nextState = produce(chartBuilderReducer)(initialStateWithAxes, {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinitionWithConstants,
      } as ChartBuilderActions);

      expect(nextState.axes.major).toMatchObject<Partial<AxisConfiguration>>({
        sortBy: 'overriding value',
        visible: true,
      });
    });
  });

  describe('UPDATE_CHART_AXIS', () => {
    const initialState: ChartBuilderState = {
      axes: {
        major: {
          dataSets: [],
          referenceLines: [],
          type: 'major',
          visible: true,
        },
      },
      definition: testChartDefinition,
      options: {
        height: 300,
      },
    };

    test('overrides chart definition defaults', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'UPDATE_CHART_AXIS',
        payload: {
          type: 'major',
          groupBy: 'indicators',
          visible: false,
        },
      } as ChartBuilderActions);

      expect(nextState.axes.major).toEqual<AxisConfiguration>({
        dataSets: [],
        groupBy: 'indicators',
        min: 0,
        referenceLines: [],
        showGrid: true,
        size: 50,
        sortAsc: true,
        sortBy: 'name',
        tickConfig: 'default',
        tickSpacing: 1,
        type: 'major',
        unit: '',
        visible: false,
      });
    });

    test('does not override chart definition constants', () => {
      const initialStateWithConstants: ChartBuilderState = {
        ...initialState,
        definition: {
          ...testChartDefinition,
          axes: {
            ...testChartDefinition.axes,
            major: {
              ...testChartDefinition.axes.major,
              constants: {
                groupBy: 'filters',
                visible: true,
              },
            },
          },
        } as ChartDefinition,
      };

      const nextState = produce(chartBuilderReducer)(
        initialStateWithConstants,
        {
          type: 'UPDATE_CHART_AXIS',
          payload: {
            type: 'major',
            groupBy: 'indicators',
            visible: false,
          },
        } as ChartBuilderActions,
      );

      expect(nextState.axes.major).toMatchObject<Partial<AxisConfiguration>>({
        groupBy: 'filters',
        visible: true,
      });
    });

    test('throws if an invalid axis definition `type` is provided', () => {
      expect(() => {
        produce(chartBuilderReducer)(initialState, {
          type: 'UPDATE_CHART_AXIS',
          payload: {
            type: 'not valid' as AxisType,
            dataSets: [
              {
                indicator: 'some thing',
              },
            ],
          },
        } as ChartBuilderActions);
      }).toThrow("Could not find chart axis definition with type 'not valid'");
    });
  });

  describe('UPDATE_CHART_OPTIONS', () => {
    const initialState: ChartBuilderState = {
      axes: {},
      definition: testChartDefinition,
      options: {
        height: 300,
      },
    };

    test('sets `options` with defaults not in payload', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'UPDATE_CHART_OPTIONS',
        payload: {
          legend: 'top',
        },
      } as ChartBuilderActions);

      expect(nextState.options).toEqual<ChartOptions>({
        height: 300,
        legend: 'top',
        title: '',
      });
    });

    test('does not unset existing `options`', () => {
      const initialStateWithExistingOptions: ChartBuilderState = {
        ...initialState,
        options: {
          height: 300,
          width: 400,
        },
      };

      const nextState = produce(chartBuilderReducer)(
        initialStateWithExistingOptions,
        {
          type: 'UPDATE_CHART_OPTIONS',
          payload: {
            legend: 'top',
          },
        } as ChartBuilderActions,
      );

      expect(nextState.options).toEqual<ChartOptions>({
        height: 300,
        legend: 'top',
        title: '',
        width: 400,
      });
    });

    test('unsets existing `options` if they are set to undefined', () => {
      const initialStateWithExistingOptions: ChartBuilderState = {
        ...initialState,
        options: {
          height: 300,
          width: 400,
        },
      };

      const nextState = produce(chartBuilderReducer)(
        initialStateWithExistingOptions,
        {
          type: 'UPDATE_CHART_OPTIONS',
          payload: {
            legend: 'top',
            width: undefined,
          },
        } as ChartBuilderActions,
      );

      expect(nextState.options).toEqual<ChartOptions>({
        height: 300,
        legend: 'top',
        title: '',
      });
    });

    test('overrides `options` with chart definition constants', () => {
      const initialStateWithConstants: ChartBuilderState = {
        ...initialState,
        definition: {
          ...testChartDefinition,
          options: {
            ...testChartDefinition.options,
            constants: {
              height: 400,
              title: 'overrides title',
              legend: 'bottom',
            },
          },
        } as ChartDefinition,
      };

      const nextState = produce(chartBuilderReducer)(
        initialStateWithConstants,
        {
          type: 'UPDATE_CHART_OPTIONS',
          payload: {
            height: 500,
            legend: 'top',
            title: 'override me',
          },
        } as ChartBuilderActions,
      );

      expect(nextState.options).toEqual<ChartOptions>({
        height: 400,
        title: 'overrides title',
        legend: 'bottom',
      });
    });
  });

  describe('ADD_DATA_SET', () => {
    const initialState: ChartBuilderState = {
      axes: {
        major: {
          type: 'major',
          visible: true,
          referenceLines: [],
          dataSets: [
            {
              indicator: 'indicator-1',
              filters: ['filter-1'],
              config: {
                label: 'Label 1',
              },
            },
          ],
        },
      },
      definition: testChartDefinition,
      options: {
        height: 300,
      },
    };

    test('adds payload to data sets', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'ADD_DATA_SET',
        payload: {
          indicator: 'indicator-2',
          filters: ['filter-2'],
          config: {
            label: 'Label 2',
          },
        },
      } as ChartBuilderActions);

      expect(nextState.axes.major?.dataSets).toEqual<DataSetConfiguration[]>([
        {
          indicator: 'indicator-1',
          filters: ['filter-1'],
          config: {
            label: 'Label 1',
          },
        },
        {
          indicator: 'indicator-2',
          filters: ['filter-2'],
          config: {
            label: 'Label 2',
          },
        },
      ]);
    });
  });

  describe('REMOVE_DATA_SET', () => {
    const initialState: ChartBuilderState = {
      axes: {
        major: {
          type: 'major',
          visible: true,
          referenceLines: [],
          dataSets: [
            {
              indicator: 'indicator-1',
              filters: ['filter-1'],
              config: {
                label: 'Label 1',
              },
            },
            {
              indicator: 'indicator-2',
              filters: ['filter-2'],
              config: {
                label: 'Label 2',
              },
            },
          ],
        },
      },
      definition: testChartDefinition,
      options: {
        height: 300,
      },
    };

    test('removes data set at the payload index', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'REMOVE_DATA_SET',
        payload: 1,
      } as ChartBuilderActions);

      expect(nextState.axes.major?.dataSets).toEqual<DataSetConfiguration[]>([
        {
          indicator: 'indicator-1',
          filters: ['filter-1'],
          config: {
            label: 'Label 1',
          },
        },
      ]);
    });
  });

  describe('UPDATE_DATA_SETS', () => {
    const initialState: ChartBuilderState = {
      axes: {
        major: {
          type: 'major',
          visible: true,
          referenceLines: [],
          dataSets: [
            {
              indicator: 'indicator-1',
              filters: ['filter-1'],
              config: {
                label: 'Label 1',
              },
            },
            {
              indicator: 'indicator-2',
              filters: ['filter-2'],
              config: {
                label: 'Label 2',
              },
            },
          ],
        },
      },
      definition: testChartDefinition,
      options: {
        height: 300,
      },
    };

    test('replaces data sets with payload', () => {
      const payload: DataSetConfiguration[] = [
        {
          indicator: 'indicator-3',
          filters: ['filter-3'],
          config: {
            label: 'Label 3',
          },
        },
      ];

      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'UPDATE_DATA_SETS',
        payload,
      } as ChartBuilderActions);

      expect(nextState.axes.major?.dataSets).toEqual(payload);
    });
  });
});

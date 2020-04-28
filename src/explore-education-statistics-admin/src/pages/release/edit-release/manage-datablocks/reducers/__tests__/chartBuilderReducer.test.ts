import { lineChartBlockDefinition } from '@common/modules/charts/components/LineChartBlock';
import {
  AxisConfiguration,
  AxisType,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import { DataSetConfiguration } from '@common/modules/charts/types/dataSet';
import { Chart } from '@common/services/types/blocks';
import { renderHook } from '@testing-library/react-hooks';
import produce from 'immer';
import {
  ChartBuilderActions,
  chartBuilderReducer,
  ChartBuilderState,
  ChartOptions,
  useChartBuilderReducer,
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
      canSort: true,
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
      forms: {
        options: {
          isValid: true,
        },
        data: {
          isValid: true,
        },
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
      forms: {
        options: {
          isValid: true,
        },
        data: {
          isValid: true,
        },
        major: {
          isValid: true,
        },
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
      }).toThrow("Could not find chart axis definition for type 'not valid'");
    });
  });

  describe('UPDATE_CHART_OPTIONS', () => {
    const initialState: ChartBuilderState = {
      axes: {},
      definition: testChartDefinition,
      options: {
        height: 300,
      },
      forms: {
        options: {
          isValid: true,
        },
        data: {
          isValid: true,
        },
        major: {
          isValid: true,
        },
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
      forms: {
        options: {
          isValid: true,
        },
        data: {
          isValid: true,
        },
        major: {
          isValid: true,
        },
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

    test('sets `forms.data.isValid` to true', () => {
      const nextState = produce(chartBuilderReducer)(
        {
          ...initialState,
          forms: {
            ...initialState.forms,
            data: {
              isValid: false,
            },
          },
        },
        {
          type: 'ADD_DATA_SET',
          payload: {
            indicator: 'indicator-2',
            filters: ['filter-2'],
            config: {
              label: 'Label 2',
            },
          },
        } as ChartBuilderActions,
      );

      expect(nextState.forms.data.isValid).toBe(true);
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
      forms: {
        options: {
          isValid: true,
        },
        data: {
          isValid: true,
        },
        major: {
          isValid: true,
        },
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

    test('sets `forms.data.isValid` to false if there are no remaining data sets', () => {
      let nextState = produce(chartBuilderReducer)(initialState, {
        type: 'REMOVE_DATA_SET',
        payload: 0,
      } as ChartBuilderActions);

      expect(nextState.forms.data.isValid).toBe(true);

      nextState = produce(chartBuilderReducer)(nextState, {
        type: 'REMOVE_DATA_SET',
        payload: 0,
      } as ChartBuilderActions);

      expect(nextState.axes.major?.dataSets).toEqual([]);
      expect(nextState.forms.data.isValid).toBe(false);
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
      forms: {
        options: {
          isValid: true,
        },
        data: {
          isValid: true,
        },
        major: {
          isValid: true,
        },
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

    test('sets `forms.data.isValid` to false if there are no remaining data sets', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'UPDATE_DATA_SETS',
        payload: [],
      } as ChartBuilderActions);

      expect(nextState.axes.major?.dataSets).toEqual([]);
      expect(nextState.forms.data.isValid).toBe(false);
    });
  });

  describe('UPDATE_FORM', () => {
    const initialState: ChartBuilderState = {
      axes: {},
      definition: testChartDefinition,
      options: {
        height: 300,
      },
      forms: {
        options: {
          isValid: true,
        },
        data: {
          isValid: true,
        },
      },
    };

    test('throws error if invalid `form` key is used', () => {
      expect(() => {
        produce(chartBuilderReducer)(initialState, {
          type: 'UPDATE_FORM',
          payload: {
            form: 'not-correct',
            state: {
              isValid: false,
            },
          },
        } as ChartBuilderActions);
      }).toThrowError("Could not find form 'not-correct' to update");
    });

    test('updates correct `form` with payload', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'UPDATE_FORM',
        payload: {
          form: 'options',
          state: {
            isValid: false,
          },
        },
      } as ChartBuilderActions);

      expect(nextState.forms.options.isValid).toBe(false);
    });
  });

  describe('useChartBuilderReducer', () => {
    test('has correct state when no initial configuration', () => {
      const { result } = renderHook(() => useChartBuilderReducer());

      expect(result.current.state).toEqual<ChartBuilderState>({
        axes: {},
        options: {
          title: '',
          height: 300,
        },
        forms: {
          data: {
            isValid: true,
          },
          options: {
            isValid: true,
          },
        },
      });
    });

    test('has correct with with initial configuration', () => {
      const initialConfiguration: Chart = {
        legend: 'top',
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            sortAsc: true,
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
                config: {
                  label: 'Test label 1',
                },
              },
            ],
            referenceLines: [],
            visible: true,
            size: 100,
            showGrid: true,
            min: 2,
            max: 10,
            tickConfig: 'default',
            unit: '%',
          },
          minor: {
            type: 'minor',
            sortAsc: true,
            dataSets: [],
            referenceLines: [],
            visible: true,
            size: 75,
            showGrid: true,
            min: 500,
            max: 1000,
            tickConfig: 'default',
          },
        },
        type: 'line',
        height: 300,
      };

      const { result } = renderHook(() =>
        useChartBuilderReducer(initialConfiguration),
      );

      expect(result.current.state).toEqual<ChartBuilderState>({
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            sortAsc: true,
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
                config: {
                  label: 'Test label 1',
                },
              },
            ],
            referenceLines: [],
            visible: true,
            showGrid: true,
            size: 100,
            min: 2,
            max: 10,
            tickConfig: 'default',
            tickSpacing: 1,
            unit: '%',
          },
          minor: {
            type: 'minor',
            sortAsc: true,
            dataSets: [],
            referenceLines: [],
            visible: true,
            showGrid: true,
            size: 75,
            min: 500,
            max: 1000,
            tickConfig: 'default',
            tickSpacing: 1,
            unit: '',
          },
        },
        definition: lineChartBlockDefinition,
        options: {
          height: 300,
          legend: 'top',
          title: '',
        },
        forms: {
          data: {
            isValid: true,
          },
          options: {
            isValid: true,
          },
          major: {
            isValid: true,
          },
          minor: {
            isValid: true,
          },
        },
      });
    });

    test('has correct state with minimal initial configuration merged with chart definition defaults', () => {
      const initialConfiguration: Chart = {
        legend: 'top',
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
                config: {
                  label: 'Test label 1',
                },
              },
            ],
            referenceLines: [],
            visible: true,
          },
          minor: {
            type: 'minor',
            dataSets: [],
            referenceLines: [],
            visible: true,
          },
        },
        type: 'line',
        height: 300,
      };

      const { result } = renderHook(() =>
        useChartBuilderReducer(initialConfiguration),
      );

      expect(result.current.state).toEqual<ChartBuilderState>({
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            sortAsc: true,
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
                config: {
                  label: 'Test label 1',
                },
              },
            ],
            referenceLines: [],
            visible: true,
            size: 50,
            showGrid: true,
            min: 0,
            tickConfig: 'default',
            tickSpacing: 1,
            unit: '',
          },
          minor: {
            type: 'minor',
            dataSets: [],
            referenceLines: [],
            visible: true,
            showGrid: true,
            size: 50,
            min: 0,
            tickConfig: 'default',
            tickSpacing: 1,
            unit: '',
          },
        },
        definition: lineChartBlockDefinition,
        options: {
          height: 300,
          legend: 'top',
          title: '',
        },
        forms: {
          data: {
            isValid: true,
          },
          options: {
            isValid: true,
          },
          major: {
            isValid: true,
          },
          minor: {
            isValid: true,
          },
        },
      });
    });

    test('has default `axes.minor` state if initial configuration is missing it', () => {
      const initialConfiguration: Chart = {
        legend: 'top',
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
                config: {
                  label: 'Test label 1',
                },
              },
            ],
            referenceLines: [],
            visible: true,
          },
        },
        type: 'line',
        height: 300,
      };

      const { result } = renderHook(() =>
        useChartBuilderReducer(initialConfiguration),
      );

      expect(result.current.state.axes.minor).toEqual<AxisConfiguration>({
        type: 'minor',
        dataSets: [],
        referenceLines: [],
        visible: true,
        showGrid: true,
        size: 50,
        min: 0,
        tickConfig: 'default',
        tickSpacing: 1,
        unit: '',
      });
    });

    test('merges deprecated `labels` configurations into `axes.major.dataSets`', () => {
      const initialConfiguration: Chart = {
        legend: 'top',
        labels: {
          'indicator-1_filter-1_____': {
            value: 'indicator-1_filter-1_____',
            label: 'Test label 1',
          },
        },
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
              },
            ],
            referenceLines: [],
            visible: true,
          },
          minor: {
            type: 'minor',
            dataSets: [],
            referenceLines: [],
            visible: true,
          },
        },
        type: 'line',
        height: 300,
      };

      const { result } = renderHook(() =>
        useChartBuilderReducer(initialConfiguration),
      );

      expect(result.current.state.axes.major?.dataSets).toEqual([
        {
          indicator: 'indicator-1',
          filters: ['filter-1'],
          config: {
            label: 'Test label 1',
          },
        },
      ]);
    });
  });
});

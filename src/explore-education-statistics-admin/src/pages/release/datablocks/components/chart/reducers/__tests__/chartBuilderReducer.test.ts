import {
  ChartBuilderActions,
  chartBuilderReducer,
  ChartBuilderState,
  ChartOptions,
  useChartBuilderReducer,
} from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import { lineChartBlockDefinition } from '@common/modules/charts/components/LineChartBlock';
import {
  AxisConfiguration,
  AxisType,
  ChartDefinition,
} from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { Chart } from '@common/services/types/blocks';
import { renderHook } from '@testing-library/react-hooks';
import produce from 'immer';

describe('chartBuilderReducer', () => {
  const testChartDefinition: ChartDefinition = {
    type: 'line',
    name: 'Line',
    capabilities: {
      canPositionLegendInline: true,
      canSize: true,
      canSort: true,
      hasGridLines: true,
      hasLegend: true,
      hasLegendPosition: true,
      hasLineStyle: true,
      hasReferenceLines: true,
      hasSymbols: true,
      requiresGeoJson: false,
      stackable: false,
    },
    options: {
      defaults: {
        height: 300,
        titleType: 'default',
      },
    },
    legend: {
      defaults: {
        position: 'top',
      },
    },
    axes: {
      major: {
        id: 'xaxis',
        title: 'X Axis',
        type: 'major',
        capabilities: {
          canRotateLabel: false,
        },
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
        capabilities: {
          canRotateLabel: true,
        },
      },
    },
  };

  describe('UPDATE_CHART_DEFINITION', () => {
    const initialState: ChartBuilderState = {
      axes: {},
      options: {
        height: 300,
        title: '',
        titleType: 'default',
        alt: '',
      },
    };

    test('sets the `definition` to the payload', () => {
      const action: ChartBuilderActions = {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      };

      const nextState = produce(chartBuilderReducer)(initialState, action);

      expect(nextState.definition).toEqual(testChartDefinition);
    });

    test('sets `options` with defaults from the definition', () => {
      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      });

      expect(nextState.options).toEqual<ChartOptions>({
        height: 300,
        title: '',
        titleType: 'default',
        alt: '',
      });
    });

    test('sets `legend` with defaults from the definition', () => {
      const action: ChartBuilderActions = {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      };

      const nextState = produce(chartBuilderReducer)(initialState, action);

      expect(nextState.legend).toEqual<LegendConfiguration>({
        position: 'top',
        items: [],
      });
    });

    test('merges correctly with `options` or `legend` that already exist', () => {
      const initialStateWithOptions: ChartBuilderState = {
        ...initialState,
        options: {
          height: 400,
          title: 'Some title',
          titleType: 'alternative',
          alt: 'Some alt',
        },
        legend: {
          position: 'bottom',
          items: [
            {
              dataSet: {
                indicator: 'indicator-1',
                filters: [],
              },
              label: 'Existing legend item',
              colour: 'blue',
            },
          ],
        },
      };

      const action: ChartBuilderActions = {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      };

      const nextState = produce(chartBuilderReducer)(
        initialStateWithOptions,
        action,
      );

      expect(nextState.options).toEqual<ChartOptions>({
        // Height is set to the definition default
        height: 300,
        title: 'Some title',
        titleType: 'alternative',
        alt: 'Some alt',
      });
      expect(nextState.legend).toEqual<LegendConfiguration>({
        position: 'bottom',
        items: [
          {
            dataSet: {
              indicator: 'indicator-1',
              filters: [],
            },
            label: 'Existing legend item',
            colour: 'blue',
          },
        ],
      });
    });

    test('sets `axes` with defaults', () => {
      const action: ChartBuilderActions = {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      };

      const nextState = produce(chartBuilderReducer)(initialState, action);

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
        label: {
          text: '',
        },
      });

      expect(nextState.axes.minor).toEqual<AxisConfiguration>({
        dataSets: [],
        referenceLines: [],
        type: 'minor',
        visible: true,
        label: {
          text: '',
        },
      });
    });

    test('merges correctly with `axes` options that already exist', () => {
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

      const action: ChartBuilderActions = {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      };

      const nextState = produce(chartBuilderReducer)(
        initialStateWithAxes,
        action,
      );

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
        title: '',
        titleType: 'default',
        alt: '',
      },
    };

    test('overrides chart definition defaults', () => {
      const action: ChartBuilderActions = {
        type: 'UPDATE_CHART_AXIS',
        payload: {
          dataSets: [],
          referenceLines: [],
          type: 'major',
          groupBy: 'indicators',
          visible: false,
          label: {
            text: 'Some label',
          },
        },
      };

      const nextState = produce(chartBuilderReducer)(initialState, action);

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
        label: {
          text: 'Some label',
        },
      });
    });

    test('throws if an invalid axis definition `type` is provided', () => {
      expect(() => {
        const action: ChartBuilderActions = {
          type: 'UPDATE_CHART_AXIS',
          payload: {
            type: 'not valid' as AxisType,
            referenceLines: [],
            visible: true,
            dataSets: [
              {
                indicator: 'some thing',
                filters: [],
              },
            ],
          },
        };

        produce(chartBuilderReducer)(initialState, action);
      }).toThrow("Could not find chart axis definition for type 'not valid'");
    });
  });

  describe('UPDATE_CHART_OPTIONS', () => {
    const initialState: ChartBuilderState = {
      axes: {},
      definition: testChartDefinition,
      options: {
        height: 300,
        title: '',
        titleType: 'default',
        alt: '',
      },
    };

    test('sets `options` to payload', () => {
      const action: ChartBuilderActions = {
        type: 'UPDATE_CHART_OPTIONS',
        payload: {
          height: 500,
          width: 400,
          title: 'Test title',
          titleType: 'alternative',
          alt: 'Test alt',
        },
      };

      const nextState = produce(chartBuilderReducer)(initialState, action);

      expect(nextState.options).toEqual<ChartOptions>({
        height: 500,
        width: 400,
        title: 'Test title',
        titleType: 'alternative',
        alt: 'Test alt',
      });
    });

    test('does not unset existing `options` state that were not included in payload', () => {
      const initialStateWithExistingOptions: ChartBuilderState = {
        ...initialState,
        options: {
          height: 300,
          title: '',
          titleType: 'default',
          alt: '',
          stacked: true,
        },
      };

      const action: ChartBuilderActions = {
        type: 'UPDATE_CHART_OPTIONS',
        payload: {
          height: 500,
          width: 400,
          title: '',
          titleType: 'default',
          alt: '',
        },
      };

      const nextState = produce(chartBuilderReducer)(
        initialStateWithExistingOptions,
        action,
      );

      expect(nextState.options).toEqual<ChartOptions>({
        height: 500,
        width: 400,
        title: '',
        titleType: 'default',
        alt: '',
        stacked: true,
      });
    });

    test('unsets existing `options` if they are set to undefined', () => {
      const initialStateWithExistingOptions: ChartBuilderState = {
        ...initialState,
        options: {
          height: 300,
          width: 400,
          title: '',
          titleType: 'default',
          alt: '',
        },
      };

      const action: ChartBuilderActions = {
        type: 'UPDATE_CHART_OPTIONS',
        payload: {
          height: 300,
          width: undefined,
          title: '',
          titleType: 'default',
          alt: '',
        },
      };

      const nextState = produce(chartBuilderReducer)(
        initialStateWithExistingOptions,
        action,
      );

      expect(nextState.options).toEqual<ChartOptions>({
        height: 300,
        title: '',
        titleType: 'default',
        alt: '',
      });
    });
  });

  describe('UPDATE_DATA_SETS', () => {
    const initialState: ChartBuilderState = {
      axes: {
        major: {
          type: 'major',
          visible: true,
          referenceLines: [],
          dataSets: [],
        },
      },
      definition: testChartDefinition,
      options: {
        height: 300,
        title: '',
        titleType: 'default',
        alt: '',
      },
    };

    test('sets payload to data sets', () => {
      const action: ChartBuilderActions = {
        type: 'UPDATE_DATA_SETS',
        payload: [
          {
            indicator: 'indicator-1',
            filters: ['filter-1'],
          },
        ],
      };

      const nextState = produce(chartBuilderReducer)(initialState, action);

      expect(nextState.axes.major?.dataSets).toEqual<DataSet[]>([
        {
          indicator: 'indicator-1',
          filters: ['filter-1'],
        },
      ]);
    });
  });

  describe('RESET', () => {
    test('resets to correct initial state', () => {
      const initialState: ChartBuilderState = {
        axes: {
          major: {
            type: 'major',
            visible: true,
            referenceLines: [],
            dataSets: [],
          },
        },
        definition: testChartDefinition,
        options: {
          height: 400,
          title: 'Something',
          titleType: 'alternative',
          alt: 'Some alt',
        },
      };

      const nextState = produce(chartBuilderReducer)(initialState, {
        type: 'RESET',
      });

      expect(nextState).toEqual<ChartBuilderState>({
        axes: {},
        titleType: 'default',
      });
    });
  });

  describe('useChartBuilderReducer', () => {
    test('has correct state when no initial configuration', () => {
      const { result } = renderHook(() =>
        useChartBuilderReducer(undefined, 'Table title'),
      );

      expect(result.current.state).toEqual<ChartBuilderState>({
        axes: {},
        titleType: 'default',
      });
    });

    test('has correct state with initial configuration', () => {
      const initialConfiguration: Chart = {
        legend: {
          position: 'top',
          items: [],
        },
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            sortBy: 'something',
            sortAsc: true,
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
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
            label: {
              text: 'Test major axis label',
              width: 300,
            },
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
            label: {
              text: 'Test minor axis label',
              rotated: true,
            },
          },
        },
        type: 'line',
        height: 300,
        title: 'Table title',
        alt: '',
      };

      const { result } = renderHook(() =>
        useChartBuilderReducer(initialConfiguration, 'Table title'),
      );

      expect(result.current.state).toEqual<ChartBuilderState>({
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            sortBy: 'something',
            sortAsc: true,
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
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
            label: {
              text: 'Test major axis label',
              width: 300,
            },
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
            label: {
              text: 'Test minor axis label',
              rotated: true,
              width: 100,
            },
          },
        },
        definition: lineChartBlockDefinition,
        options: {
          height: 300,
          title: 'Table title',
          titleType: 'default',
          alt: '',
        },
        legend: {
          position: 'top',
          items: [],
        },
      });
    });

    test('has correct state with minimal initial configuration merged with chart definition defaults', () => {
      const initialConfiguration: Chart = {
        legend: {
          position: 'top',
          items: [
            {
              dataSet: {
                indicator: 'indicator-1',
                filters: ['filter-1'],
              },
              colour: 'yellow',
              label: 'Legend item 1',
            },
          ],
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
        title: 'Table title',
        alt: '',
      };

      const { result } = renderHook(() =>
        useChartBuilderReducer(initialConfiguration, 'Table title'),
      );

      expect(result.current.state).toEqual<ChartBuilderState>({
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            sortBy: 'name',
            sortAsc: true,
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
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
            label: {
              text: '',
            },
          },
          minor: {
            type: 'minor',
            dataSets: [],
            referenceLines: [],
            visible: true,
            showGrid: true,
            tickConfig: 'default',
            tickSpacing: 1,
            unit: '',
            label: {
              text: '',
              width: 100,
            },
          },
        },
        definition: lineChartBlockDefinition,
        options: {
          height: 300,
          title: 'Table title',
          titleType: 'default',
          alt: '',
        },
        legend: {
          position: 'top',
          items: [
            {
              dataSet: {
                indicator: 'indicator-1',
                filters: ['filter-1'],
              },
              colour: 'yellow',
              label: 'Legend item 1',
            },
          ],
        },
      });
    });

    test('has default `axes.minor` state if initial configuration is missing it', () => {
      const initialConfiguration: Chart = {
        legend: {
          position: 'top',
          items: [],
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
        },
        type: 'line',
        height: 300,
        title: '',
        alt: '',
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
        tickConfig: 'default',
        tickSpacing: 1,
        unit: '',
        label: {
          text: '',
          width: 100,
        },
      });
    });

    test('has correct state with initial configuration with custom chart title', () => {
      const initialConfiguration: Chart = {
        legend: {
          position: 'top',
          items: [],
        },
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            sortBy: 'something',
            sortAsc: true,
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
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
            label: {
              text: 'Test major axis label',
              width: 300,
            },
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
            label: {
              text: 'Test minor axis label',
              rotated: true,
            },
          },
        },
        type: 'line',
        height: 300,
        title: 'Chart title',
        alt: '',
      };

      const { result } = renderHook(() =>
        useChartBuilderReducer(initialConfiguration, 'Table title'),
      );

      expect(result.current.state).toEqual<ChartBuilderState>({
        axes: {
          major: {
            type: 'major',
            groupBy: 'timePeriod',
            sortBy: 'something',
            sortAsc: true,
            dataSets: [
              {
                indicator: 'indicator-1',
                filters: ['filter-1'],
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
            label: {
              text: 'Test major axis label',
              width: 300,
            },
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
            label: {
              text: 'Test minor axis label',
              rotated: true,
              width: 100,
            },
          },
        },
        definition: lineChartBlockDefinition,
        options: {
          height: 300,
          title: 'Chart title',
          titleType: 'alternative',
          alt: '',
        },
        legend: {
          position: 'top',
          items: [],
        },
      });
    });
  });
});

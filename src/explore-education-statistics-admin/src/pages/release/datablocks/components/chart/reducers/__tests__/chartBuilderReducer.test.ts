import {
  ChartBuilderActions,
  chartBuilderReducer,
  ChartBuilderState,
  ChartOptions,
  useChartBuilderReducer,
} from '@admin/pages/release/datablocks/components/chart/reducers/chartBuilderReducer';
import { lineChartBlockDefinition } from '@common/modules/charts/components/LineChartBlock';
import { mapBlockDefinition } from '@common/modules/charts/components/MapBlock';
import {
  AxisConfiguration,
  AxisType,
  ChartDefinition,
  DataGroupingConfig,
} from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { Chart } from '@common/services/types/blocks';
import { renderHook } from '@testing-library/react';
import { produce } from 'immer';
import { testFullTable } from '../../__tests__/__data__/testTableData';

describe('chartBuilderReducer', () => {
  const testChartDefinition: ChartDefinition = {
    type: 'line',
    name: 'Line',
    capabilities: {
      canPositionLegendInline: true,
      canIncludeNonNumericData: true,
      canSetBarThickness: true,
      canSetDataLabelPosition: true,
      canShowDataLabels: true,
      canShowAllMajorAxisTicks: false,
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
        subtitle: '',
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

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, action);

      expect(nextState.definition).toEqual(testChartDefinition);
    });

    test('sets `options` with defaults from the definition', () => {
      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, {
        type: 'UPDATE_CHART_DEFINITION',
        payload: testChartDefinition,
      });

      expect(nextState.options).toEqual<ChartOptions>({
        height: 300,
        subtitle: '',
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

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, action);

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
          subtitle: 'Some subtitle',
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

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialStateWithOptions, action);

      expect(nextState.options).toEqual<ChartOptions>({
        // Height is set to the definition default
        height: 300,
        subtitle: 'Some subtitle',
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

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, action);

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

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialStateWithAxes, action);

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

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, action);

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

        produce(
          chartBuilderReducer({
            data: [],
            meta: testFullTable.subjectMeta,
          }),
        )(initialState, action);
      }).toThrow("Could not find chart axis definition for type 'not valid'");
    });
  });

  describe('UPDATE_CHART_OPTIONS', () => {
    const initialState: ChartBuilderState = {
      axes: {},
      definition: testChartDefinition,
      options: {
        height: 300,
        subtitle: '',
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
          subtitle: 'Test subtitle',
          title: 'Test title',
          titleType: 'alternative',
          alt: 'Test alt',
        },
      };

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, action);

      expect(nextState.options).toEqual<ChartOptions>({
        height: 500,
        width: 400,
        subtitle: 'Test subtitle',
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
          subtitle: '',
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
          subtitle: '',
          title: '',
          titleType: 'default',
          alt: '',
        },
      };

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialStateWithExistingOptions, action);

      expect(nextState.options).toEqual<ChartOptions>({
        height: 500,
        width: 400,
        subtitle: '',
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
          subtitle: '',
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
          subtitle: '',
          title: '',
          titleType: 'default',
          alt: '',
        },
      };

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialStateWithExistingOptions, action);

      expect(nextState.options).toEqual<ChartOptions>({
        height: 300,
        subtitle: '',
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

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, action);

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

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, {
        type: 'RESET',
      });

      expect(nextState).toEqual<ChartBuilderState>({
        axes: {},
        titleType: 'default',
      });
    });
  });

  describe('UPDATE_MAP_BOUNDARY_LEVELS', () => {
    const initialState: ChartBuilderState = {
      axes: {},
      options: {
        height: 300,
        subtitle: '',
        title: '',
        titleType: 'default',
        alt: '',
      },
      map: {
        dataSetConfigs: [
          {
            dataSet: { filters: [] },
            dataGrouping: { type: 'EqualIntervals', customGroups: [] },
          },
        ],
      },
    };

    test('sets the boundary level to the payload boundaryLevel', () => {
      const action: ChartBuilderActions = {
        type: 'UPDATE_MAP_BOUNDARY_LEVELS',
        payload: {
          boundaryLevel: 2,
          dataSetConfigs: [{ dataSet: { filters: [] }, boundaryLevel: 1 }],
        },
      };

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, action);

      expect(nextState.options?.boundaryLevel).toEqual(2);
      expect(nextState.map?.dataSetConfigs[0]?.boundaryLevel).toEqual(1);
    });
  });

  describe('UPDATE_MAP_DATA_GROUPINGS', () => {
    const initialState: ChartBuilderState = {
      axes: {},
      options: {
        height: 300,
        subtitle: '',
        title: '',
        titleType: 'default',
        alt: '',
      },
      map: {
        dataSetConfigs: [
          {
            dataSet: { filters: [] },
            dataGrouping: { type: 'EqualIntervals', customGroups: [] },
          },
        ],
      },
    };
    test('sets the dataGrouping to the payload dataGrouping', () => {
      const testDataGrouping: DataGroupingConfig = {
        type: 'Custom',
        customGroups: [{ min: 0, max: 999 }],
        numberOfGroups: 1,
      };
      const action: ChartBuilderActions = {
        type: 'UPDATE_MAP_DATA_GROUPINGS',
        payload: {
          dataSetConfigs: [
            {
              dataSet: { filters: [] },
              dataGrouping: testDataGrouping,
            },
          ],
        },
      };

      const nextState = produce(
        chartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
        }),
      )(initialState, action);

      expect(nextState.map?.dataSetConfigs[0]?.dataGrouping).toEqual(
        testDataGrouping,
      );
    });
  });

  describe('useChartBuilderReducer', () => {
    test('has correct state when no initial configuration', () => {
      const { result } = renderHook(() =>
        useChartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
          tableTitle: 'Table title',
        }),
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
        useChartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
          tableTitle: 'Table title',
          chart: initialConfiguration,
        }),
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
        map: undefined,

        options: {
          alt: '',
          barThickness: undefined,
          height: 300,
          includeNonNumericData: false,
          showDataLabels: false,
          subtitle: '',
          title: 'Table title',
          titleType: 'default',
          width: undefined,
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
        useChartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
          tableTitle: 'Table title',
          chart: initialConfiguration,
        }),
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
          alt: '',
          barThickness: undefined,
          height: 300,
          includeNonNumericData: false,
          showDataLabels: false,
          subtitle: '',
          title: 'Table title',
          titleType: 'default',
          width: undefined,
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
        useChartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,

          chart: initialConfiguration,
        }),
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
        useChartBuilderReducer({
          data: [],
          meta: testFullTable.subjectMeta,
          tableTitle: 'Table title',
          chart: initialConfiguration,
        }),
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
        map: undefined,
        options: {
          alt: '',
          barThickness: undefined,
          height: 300,
          includeNonNumericData: false,
          showDataLabels: false,
          subtitle: '',
          title: 'Chart title',
          titleType: 'alternative',
          width: undefined,
        },
        legend: {
          position: 'top',
          items: [],
        },
      });
    });

    test('has correct state with initial configuration for a map', () => {
      const initialConfiguration: Chart = {
        type: 'map',
        onBoundaryLevelChange: () => {},
        legend: {
          position: 'top',
          items: [],
        },
        axes: {
          major: {
            type: 'major',
            dataSets: [
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
            ],
            referenceLines: [],
          },
        },

        height: 300,
        title: 'Chart title',
        alt: '',
        boundaryLevel: 1,
      };

      const { result } = renderHook(() =>
        useChartBuilderReducer({
          data: testFullTable.results,
          meta: testFullTable.subjectMeta,
          tableTitle: 'Table title',
          chart: initialConfiguration,
        }),
      );

      expect(result.current.state).toEqual<ChartBuilderState>({
        axes: {
          major: {
            type: 'major',
            groupBy: 'locations',
            dataSets: [
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
            ],
            referenceLines: [],
            visible: true,
            label: {
              text: '',
            },
          },
        },
        definition: mapBlockDefinition,
        map: {
          dataSetConfigs: [
            {
              dataGrouping: {
                customGroups: [],
                numberOfGroups: 5,
                type: 'EqualIntervals',
              },
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
            },
            {
              dataGrouping: {
                customGroups: [],
                numberOfGroups: 5,
                type: 'EqualIntervals',
              },
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
            },
          ],
        },
        options: {
          alt: '',
          height: 300,
          subtitle: '',
          title: 'Chart title',
          titleType: 'alternative',
          boundaryLevel: 1,
        },
        legend: {
          position: 'top',
          items: [],
        },
      });
    });

    test('setting boundary levels does not change data groupings', () => {
      const initialConfiguration: Chart = {
        type: 'map',
        onBoundaryLevelChange: () => {},
        legend: {
          position: 'top',
          items: [],
        },
        axes: {
          major: {
            type: 'major',
            dataSets: [
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
            ],
            referenceLines: [],
          },
        },
        height: 300,
        title: 'Chart title',
        alt: '',
        boundaryLevel: 1,
        map: {
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataGrouping: { type: 'EqualIntervals', customGroups: [] },
              boundaryLevel: 2,
            },
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataGrouping: { type: 'Custom', customGroups: [] },
              boundaryLevel: 3,
            },
          ],
        },
      };

      const { result, rerender } = renderHook(() =>
        useChartBuilderReducer({
          data: testFullTable.results,
          meta: testFullTable.subjectMeta,
          tableTitle: 'Table title',
          chart: initialConfiguration,
        }),
      );

      result.current.actions.updateMapBoundaryLevels({
        boundaryLevel: 10,
        dataSetConfigs: [
          {
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
            boundaryLevel: 20,
          },
          {
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            boundaryLevel: 30,
          },
        ],
      });
      rerender();

      expect(result.current.state.map).toEqual<ChartBuilderState['map']>({
        dataSetConfigs: [
          {
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
            dataGrouping: { type: 'EqualIntervals', customGroups: [] },
            boundaryLevel: 20,
          },
          {
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            dataGrouping: { type: 'Custom', customGroups: [] },
            boundaryLevel: 30,
          },
        ],
      });
    });

    test('setting data groupings does not change boundary levels', () => {
      const initialConfiguration: Chart = {
        type: 'map',
        onBoundaryLevelChange: () => {},
        legend: {
          position: 'top',
          items: [],
        },
        axes: {
          major: {
            type: 'major',
            dataSets: [
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
            ],
            referenceLines: [],
          },
        },
        height: 300,
        title: 'Chart title',
        alt: '',
        boundaryLevel: 1,
        map: {
          dataSetConfigs: [
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2015_AY',
              },
              dataGrouping: { type: 'EqualIntervals', customGroups: [] },
              boundaryLevel: 2,
            },
            {
              dataSet: {
                filters: ['ethnicity-major-chinese', 'state-funded-primary'],
                indicator: 'authorised-absence-sessions',
                timePeriod: '2014_AY',
              },
              dataGrouping: { type: 'Custom', customGroups: [] },
              boundaryLevel: 3,
            },
          ],
        },
      };

      const { result, rerender } = renderHook(() =>
        useChartBuilderReducer({
          data: testFullTable.results,
          meta: testFullTable.subjectMeta,
          tableTitle: 'Table title',
          chart: initialConfiguration,
        }),
      );

      result.current.actions.updateMapDataGroupings({
        dataSetConfigs: [
          {
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
            dataGrouping: { type: 'Quantiles', customGroups: [] },
          },
          {
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            dataGrouping: { type: 'Quantiles', customGroups: [] },
          },
        ],
      });
      rerender();

      expect(result.current.state.map).toEqual<ChartBuilderState['map']>({
        dataSetConfigs: [
          {
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2015_AY',
            },
            dataGrouping: { type: 'Quantiles', customGroups: [] },
            boundaryLevel: 2,
          },
          {
            dataSet: {
              filters: ['ethnicity-major-chinese', 'state-funded-primary'],
              indicator: 'authorised-absence-sessions',
              timePeriod: '2014_AY',
            },
            dataGrouping: { type: 'Quantiles', customGroups: [] },
            boundaryLevel: 3,
          },
        ],
      });
    });
  });
});

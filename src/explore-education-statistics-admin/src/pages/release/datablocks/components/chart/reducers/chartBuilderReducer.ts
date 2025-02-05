import {
  MapBoundaryLevelConfig,
  MapDataGroupingConfig,
} from '@admin/pages/release/datablocks/components/chart/types/mapConfig';
import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import {
  AxesConfiguration,
  AxisConfiguration,
  AxisType,
  Chart,
  ChartDefinition,
  ChartDefinitionAxis,
  ChartDefinitionOptions,
  chartDefinitions,
  MapConfig,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import getMapDataSetCategoryConfigs from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataResult } from '@common/services/tableBuilderService';
import deepMerge from 'deepmerge';
import mapValues from 'lodash/mapValues';
import { Reducer, useCallback, useMemo } from 'react';

export interface ChartBuilderReducerOptions {
  chart?: Chart;
  data: TableDataResult[];
  meta: FullTableMeta;
  tableTitle?: string;
}

export interface ChartOptions extends ChartDefinitionOptions {
  file?: File;
  fileId?: string;
}

export interface ChartBuilderState {
  definition?: ChartDefinition;
  options?: ChartOptions;
  axes: AxesConfiguration;
  legend?: LegendConfiguration;
  map?: MapConfig;
  titleType?: 'default' | 'alternative';
}

export type ChartBuilderActions =
  | {
      type: 'UPDATE_CHART_DEFINITION';
      payload: ChartDefinition;
    }
  | {
      type: 'UPDATE_DATA_SETS';
      payload: DataSet[];
    }
  | {
      type: 'UPDATE_CHART_OPTIONS';
      payload: ChartOptions;
    }
  | {
      type: 'UPDATE_CHART_LEGEND';
      payload: LegendConfiguration;
    }
  | {
      type: 'UPDATE_MAP_BOUNDARY_LEVELS';
      payload: MapBoundaryLevelConfig;
    }
  | {
      type: 'UPDATE_MAP_DATA_GROUPINGS';
      payload: MapDataGroupingConfig;
    }
  | {
      type: 'UPDATE_CHART_AXIS';
      payload: AxisConfiguration;
    }
  | {
      type: 'RESET';
    };

const defaultOptions: ChartOptions = {
  alt: '',
  barThickness: undefined,
  height: 300,
  subtitle: '',
  title: '',
  titleType: 'default',
  width: undefined,
};

const defaultLegend: LegendConfiguration = {
  items: [],
};

const updateAxis = (
  axisDefinition: ChartDefinitionAxis,
  current: Partial<AxisConfiguration> = {},
  next: Partial<AxisConfiguration> = {},
): AxisConfiguration => {
  const defaultAxisOptions: AxisConfiguration = {
    dataSets: [],
    max: undefined,
    min: undefined,
    referenceLines: [],
    size: undefined,
    visible: true,
    label: {
      text: '',
    },
    type: axisDefinition.type,
  };

  return deepMerge.all<AxisConfiguration>(
    [
      defaultAxisOptions,
      (axisDefinition.defaults ?? {}) as Partial<AxisConfiguration>,
      current,
      next,
      {
        // Ensure `type` will not be unset
        // by one of the previous objects.
        type: axisDefinition.type,
      },
    ],
    {
      arrayMerge: (_, source) => source,
    },
  );
};

const getInitialState = ({
  chart,
  data,
  meta,
  tableTitle,
}: ChartBuilderReducerOptions): ChartBuilderState => {
  if (!chart) {
    return {
      titleType: 'default',
      axes: {},
    };
  }

  const {
    type,
    axes: initialAxes,
    legend: initialLegend,
    map,
    ...initialOptions
  } = chart;

  const definition = chartDefinitions.find(
    chartDefinition => chartDefinition.type === type,
  );

  if (!definition) {
    throw new Error(`Could not find chart definition for type: ${type}`);
  }

  const options: ChartOptions = {
    ...defaultOptions,
    ...(definition?.options.defaults ?? {}),
    ...initialOptions,
    titleType: chart.title === tableTitle ? 'default' : 'alternative',
  };

  const axes: AxesConfiguration = mapValues(
    definition?.axes ?? {},
    (axisDefinition: ChartDefinitionAxis, axisType: AxisType) =>
      updateAxis(
        axisDefinition,
        (initialAxes as AxesConfiguration)?.[axisType] ?? {},
      ),
  );

  const legend: LegendConfiguration = {
    ...defaultLegend,
    ...(initialLegend ?? {}),
  };

  return {
    definition,
    options,
    legend,
    axes,
    map: getInitialMapState({
      axes,
      data,
      definition,
      legend,
      map,
      meta,
      options,
    }),
  };
};

function getInitialMapState({
  axes,
  data,
  definition,
  legend,
  map,
  meta,
  options,
}: {
  axes: AxesConfiguration;
  data: TableDataResult[];
  definition: ChartDefinition;
  legend?: LegendConfiguration;
  map?: MapConfig;
  meta: FullTableMeta;
  options: ChartOptions;
}): MapConfig | undefined {
  if (definition.type !== 'map' || !axes.major) {
    return undefined;
  }

  return {
    dataSetConfigs: getMapDataSetConfigs({
      axisMajor: axes.major,
      data,
      legend,
      map,
      meta,
      options,
    }),
  };
}

function getMapDataSetConfigs({
  axisMajor,
  data,
  legend,
  map,
  meta,
  options,
}: {
  axisMajor: AxisConfiguration;
  data: TableDataResult[];
  legend?: LegendConfiguration;
  map?: MapConfig;
  meta: FullTableMeta;
  options?: ChartOptions;
}): MapDataSetConfig[] {
  const dataSetCategories = createDataSetCategories({
    axisConfiguration: {
      ...axisMajor,
      groupBy: 'locations',
    },
    data,
    meta,
  });

  const dataSetCategoryConfigs = getMapDataSetCategoryConfigs({
    dataSetCategories,
    dataSetConfigs: map?.dataSetConfigs,
    legendItems: legend?.items ?? [],
    meta,
    deprecatedDataClassification: options?.dataClassification,
    deprecatedDataGroups: options?.dataGroups,
  });

  return dataSetCategoryConfigs.map(config => {
    return {
      boundaryLevel: config.boundaryLevel,
      dataSet: config.rawDataSet,
      dataGrouping: config.dataGrouping,
    };
  });
}

export function chartBuilderReducer(
  reducerOptions: ChartBuilderReducerOptions,
): Reducer<ChartBuilderState, ChartBuilderActions> {
  const { data, meta } = reducerOptions;

  return (draft, action) => {
    switch (action.type) {
      case 'UPDATE_CHART_DEFINITION': {
        draft.definition = action.payload;

        draft.options = {
          ...defaultOptions,
          ...(action.payload.options.defaults ?? {}),
          ...draft.options,
          // Set height/width to definition defaults
          // as this seems to surprise users the least.
          height:
            action.payload.options.defaults?.height ??
            draft.options?.height ??
            defaultOptions.height,
          width: action.payload.options.defaults?.width ?? draft.options?.width,
        };

        if (action.payload.capabilities.hasLegend) {
          draft.legend = {
            ...defaultLegend,
            ...(action.payload.legend.defaults ?? {}),
            ...(draft.legend ?? {}),
          };
        } else {
          draft.legend = undefined;
        }

        draft.axes = mapValues(
          action.payload.axes,
          (axisDefinition: ChartDefinitionAxis, type: AxisType) => {
            return updateAxis(axisDefinition, draft.axes[type]);
          },
        );

        if (draft.definition.type === 'map' && !draft.map) {
          draft.map = getInitialMapState({
            axes: draft.axes,
            data,
            definition: action.payload,
            legend: draft.legend,
            map: draft.map,
            meta,
            options: draft.options,
          });
        }

        break;
      }
      case 'UPDATE_CHART_AXIS': {
        const axisDefinition = draft?.definition?.axes?.[action.payload.type];

        if (!axisDefinition) {
          throw new Error(
            `Could not find chart axis definition for type '${action.payload.type}'`,
          );
        }

        if (!draft.axes[action.payload.type]) {
          throw new Error(
            `Could not find axis configuration for type '${action.payload.type}'`,
          );
        }

        draft.axes[action.payload.type] = updateAxis(
          axisDefinition,
          draft.axes[action.payload.type] as AxisConfiguration,
          action.payload,
        );

        break;
      }
      case 'UPDATE_CHART_LEGEND': {
        draft.legend = {
          ...defaultLegend,
          ...(draft?.definition?.legend.defaults ?? {}),
          ...draft.legend,
          ...action.payload,
        };

        break;
      }
      case 'UPDATE_MAP_BOUNDARY_LEVELS': {
        if (!draft.map) {
          throw new Error('Map config has not been initialised');
        }

        if (draft.options) {
          draft.options.boundaryLevel = action.payload.boundaryLevel;
        }

        draft.map.dataSetConfigs.forEach((dataSetConfig, index) => {
          // eslint-disable-next-line no-param-reassign
          dataSetConfig.boundaryLevel =
            action.payload.dataSetConfigs[index].boundaryLevel;
        });

        break;
      }
      case 'UPDATE_MAP_DATA_GROUPINGS': {
        if (!draft.map) {
          throw new Error('Map config has not been initialised');
        }

        draft.map.dataSetConfigs.forEach((dataSetConfig, index) => {
          // eslint-disable-next-line no-param-reassign
          dataSetConfig.dataGrouping =
            action.payload.dataSetConfigs[index].dataGrouping;
        });

        break;
      }
      case 'UPDATE_CHART_OPTIONS': {
        draft.options = {
          ...defaultOptions,
          ...(draft?.definition?.options.defaults ?? {}),
          ...draft.options,
          ...action.payload,
        };

        break;
      }
      case 'UPDATE_DATA_SETS': {
        if (draft.axes.major) {
          draft.axes.major.dataSets = action.payload;
        }

        if (draft.map && draft.axes.major) {
          draft.map.dataSetConfigs = getMapDataSetConfigs({
            axisMajor: draft.axes.major,
            data,
            legend: draft.legend,
            map: draft.map,
            meta,
            options: draft.options,
          });
        }

        break;
      }
      case 'RESET':
        return getInitialState(reducerOptions);
      default:
        break;
    }

    return draft;
  };
}

export function useChartBuilderReducer(options: ChartBuilderReducerOptions) {
  const [state, dispatch] = useLoggedImmerReducer<
    ChartBuilderState,
    ChartBuilderActions
  >('Chart builder', chartBuilderReducer(options), getInitialState(options));

  const updateDataSets = useCallback(
    (dataSets: DataSet[]) => {
      dispatch({
        type: 'UPDATE_DATA_SETS',
        payload: dataSets,
      });
    },
    [dispatch],
  );

  const updateChartDefinition = useCallback(
    (nextChartDefinition: ChartDefinition) => {
      dispatch({
        type: 'UPDATE_CHART_DEFINITION',
        payload: nextChartDefinition,
      });
    },
    [dispatch],
  );

  const updateChartOptions = useCallback(
    (chartOptions: ChartOptions) => {
      dispatch({
        type: 'UPDATE_CHART_OPTIONS',
        payload: chartOptions,
      });
    },
    [dispatch],
  );

  const updateChartLegend = useCallback(
    (legend: LegendConfiguration) => {
      dispatch({
        type: 'UPDATE_CHART_LEGEND',
        payload: legend,
      });
    },
    [dispatch],
  );

  const updateMapBoundaryLevels = useCallback(
    (payload: MapBoundaryLevelConfig) => {
      dispatch({
        type: 'UPDATE_MAP_BOUNDARY_LEVELS',
        payload,
      });
    },
    [dispatch],
  );

  const updateMapDataGroupings = useCallback(
    (dataSetConfigs: MapDataGroupingConfig) => {
      dispatch({
        type: 'UPDATE_MAP_DATA_GROUPINGS',
        payload: dataSetConfigs,
      });
    },
    [dispatch],
  );

  const updateChartAxis = useCallback(
    (axisConfiguration: AxisConfiguration) => {
      dispatch({
        type: 'UPDATE_CHART_AXIS',
        payload: axisConfiguration,
      });
    },
    [dispatch],
  );

  const resetState = useCallback(() => {
    dispatch({
      type: 'RESET',
    });
  }, [dispatch]);

  const actions = useMemo(
    () => ({
      updateDataSets,
      updateChartDefinition,
      updateChartLegend,
      updateMapBoundaryLevels,
      updateMapDataGroupings,
      updateChartOptions,
      updateChartAxis,
      resetState,
    }),
    [
      updateDataSets,
      updateChartDefinition,
      updateChartLegend,
      updateMapBoundaryLevels,
      updateMapDataGroupings,
      updateChartOptions,
      updateChartAxis,
      resetState,
    ],
  );

  return {
    state,
    dispatch,
    actions,
  };
}

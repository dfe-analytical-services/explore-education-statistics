import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
import {
  AxesConfiguration,
  AxisConfiguration,
  AxisType,
  ChartDefinition,
  ChartDefinitionAxis,
  ChartDefinitionOptions,
  chartDefinitions,
} from '@common/modules/charts/types/chart';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import { Chart } from '@common/services/types/blocks';
import deepMerge from 'deepmerge';
import mapValues from 'lodash/mapValues';
import { useCallback, useMemo } from 'react';
import { Reducer } from 'use-immer';

export interface ChartOptions extends ChartDefinitionOptions {
  file?: File;
  fileId?: string;
}

export interface ChartBuilderState {
  definition?: ChartDefinition;
  options?: ChartOptions;
  axes: AxesConfiguration;
  legend?: LegendConfiguration;
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
      type: 'UPDATE_CHART_AXIS';
      payload: AxisConfiguration;
    }
  | {
      type: 'RESET';
    };

const defaultOptions: ChartOptions = {
  height: 300,
  title: '',
  titleType: 'default',
  alt: '',
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
    referenceLines: [],
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
      arrayMerge: (target, source) => source,
    },
  );
};

const getInitialState = (
  initialChart?: Chart,
  tableTitle?: string,
): ChartBuilderState => {
  if (!initialChart) {
    return {
      titleType: 'default',
      axes: {},
    };
  }

  const { type, axes, legend, ...options } = initialChart;

  const definition = chartDefinitions.find(
    chartDefinition => chartDefinition.type === type,
  );

  return {
    definition,
    options: {
      ...options,
      titleType: initialChart.title === tableTitle ? 'default' : 'alternative',
    },
    legend: {
      ...defaultLegend,
      ...(legend ?? {}),
    },
    axes: mapValues(
      definition?.axes ?? {},
      (axisDefinition: ChartDefinitionAxis, axisType: AxisType) =>
        updateAxis(
          axisDefinition,
          (axes?.[axisType] ?? {}) as AxisConfiguration,
        ),
    ),
  };
};

export const chartBuilderReducer: Reducer<
  ChartBuilderState,
  ChartBuilderActions
> = (draft, action) => {
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

      break;
    }
    case 'RESET':
      return getInitialState();
    default:
      break;
  }

  return draft;
};

export function useChartBuilderReducer(
  initialChart?: Chart,
  tableTitle?: string,
) {
  const [state, dispatch] = useLoggedImmerReducer<
    ChartBuilderState,
    ChartBuilderActions
  >(
    'Chart builder',
    chartBuilderReducer,
    getInitialState(initialChart, tableTitle),
  );

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
      updateChartOptions,
      updateChartAxis,
      resetState,
    }),
    [
      updateDataSets,
      updateChartAxis,
      updateChartDefinition,
      updateChartLegend,
      updateChartOptions,
      resetState,
    ],
  );

  return {
    state,
    dispatch,
    actions,
  };
}

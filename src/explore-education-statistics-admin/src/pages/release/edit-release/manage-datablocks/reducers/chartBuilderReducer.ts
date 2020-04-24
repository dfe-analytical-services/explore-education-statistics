import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import {
  AxesConfiguration,
  AxisConfiguration,
  AxisType,
  ChartDefinition,
  ChartDefinitionAxis,
  ChartDefinitionOptions,
  chartDefinitions,
} from '@common/modules/charts/types/chart';
import { DataSetConfiguration } from '@common/modules/charts/types/dataSet';
import getLabelDataSetConfigurations from '@common/modules/charts/util/getLabelDataSetConfigurations';
import { Chart } from '@common/services/types/blocks';
import mapValues from 'lodash/mapValues';
import { useCallback, useMemo } from 'react';
import { Reducer, useImmerReducer } from 'use-immer';

export interface ChartOptions extends ChartDefinitionOptions {
  fileId?: string;
  geographicId?: string;
}

export interface ChartBuilderState {
  definition?: ChartDefinition;
  options: ChartOptions;
  axes: AxesConfiguration;
  isValid?: boolean;
  chartProps?: ChartRendererProps;
}

export type ChartBuilderActions =
  | {
      type: 'UPDATE_CHART_DEFINITION';
      payload: ChartDefinition;
    }
  | {
      type: 'ADD_DATA_SET';
      payload: DataSetConfiguration;
    }
  | { type: 'REMOVE_DATA_SET'; payload: number }
  | {
      type: 'UPDATE_DATA_SETS';
      payload: DataSetConfiguration[];
    }
  | {
      type: 'UPDATE_CHART_OPTIONS';
      payload: ChartOptions;
    }
  | {
      type: 'UPDATE_CHART_AXIS';
      payload: AxisConfiguration;
    }
  | {
      type: 'UPDATE_VALID';
      payload: boolean;
    };

const defaultOptions: Partial<ChartOptions> = {
  title: '',
};

const defaultAxisOptions: Partial<AxisConfiguration> = {
  dataSets: [],
  referenceLines: [],
  visible: true,
};

const updateAxis = (
  axisDefinition: ChartDefinitionAxis,
  current: AxisConfiguration,
  next: Partial<AxisConfiguration> = {},
): AxisConfiguration => {
  return {
    ...defaultAxisOptions,
    ...(axisDefinition.defaults ?? {}),
    ...current,
    ...next,
    ...(axisDefinition.constants ?? {}),
    type: axisDefinition.type,
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
        ...(action.payload.options.constants ?? {}),
      };

      draft.axes = mapValues(
        action.payload.axes,
        (axisDefinition: ChartDefinitionAxis, type: AxisType) => {
          return updateAxis(
            axisDefinition,
            draft.axes[type] as AxisConfiguration,
          );
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
    case 'UPDATE_CHART_OPTIONS': {
      draft.options = {
        ...defaultOptions,
        ...(draft?.definition?.options.defaults ?? {}),
        ...draft.options,
        ...action.payload,
        ...(draft?.definition?.options.constants ?? {}),
      };

      break;
    }
    case 'ADD_DATA_SET': {
      if (draft.axes.major) {
        draft.axes.major.dataSets.push(action.payload);
      }

      if (typeof draft.isValid === 'undefined') {
        draft.isValid = true;
      }

      break;
    }
    case 'REMOVE_DATA_SET': {
      if (draft.axes.major) {
        draft.axes.major.dataSets.splice(action.payload, 1);

        if (typeof draft.isValid === 'undefined') {
          draft.isValid = draft.axes.major.dataSets.length > 0;
        }
      }

      break;
    }
    case 'UPDATE_DATA_SETS': {
      if (draft.axes.major) {
        draft.axes.major.dataSets = action.payload;

        if (typeof draft.isValid === 'undefined') {
          draft.isValid = draft.axes.major.dataSets.length > 0;
        }
      }

      break;
    }
    case 'UPDATE_VALID':
      draft.isValid = action.payload;
      break;
    default:
      break;
  }
};

export function useChartBuilderReducer(initialConfiguration?: Chart) {
  const [state, dispatch] = useImmerReducer<
    ChartBuilderState,
    ChartBuilderActions
  >(
    chartBuilderReducer,
    {
      axes: {},
      definition: initialConfiguration
        ? chartDefinitions.find(
            ({ type }) => type === initialConfiguration.type,
          )
        : undefined,
      options: {
        ...(initialConfiguration ?? {}),
        title: initialConfiguration?.title ?? '',
        // Make sure height is never actually 0 or negative
        // as this wouldn't make sense for any chart.
        height:
          typeof initialConfiguration?.height !== 'undefined' &&
          initialConfiguration?.height > 0
            ? initialConfiguration.height
            : 300,
      },
    },
    (initialState: ChartBuilderState) => {
      if (!initialConfiguration) {
        return initialState;
      }

      const axes: AxesConfiguration = mapValues(
        initialState.definition?.axes ?? {},
        (axisDefinition: ChartDefinitionAxis, type: AxisType) => {
          if (!initialConfiguration.axes[type]) {
            throw new Error(
              `Could not find chart axis definition for type '${type}'`,
            );
          }

          return updateAxis(
            axisDefinition,
            initialConfiguration.axes[type] as AxisConfiguration,
          );
        },
      );

      if (
        axes.major?.dataSets?.some(dataSet => !dataSet.config) &&
        initialConfiguration.labels
      ) {
        axes.major.dataSets = getLabelDataSetConfigurations(
          initialConfiguration.labels,
          axes.major.dataSets,
        );
      }

      return {
        ...initialState,
        axes,
      };
    },
  );

  const addDataSet = useCallback(
    (addedData: DataSetConfiguration) => {
      dispatch({
        type: 'ADD_DATA_SET',
        payload: addedData,
      });
    },
    [dispatch],
  );

  const removeDataSet = useCallback(
    (removedData: DataSetConfiguration, index: number) => {
      dispatch({
        type: 'REMOVE_DATA_SET',
        payload: index,
      });
    },
    [dispatch],
  );

  const updateDataSet = useCallback(
    (newData: DataSetConfiguration[]) => {
      dispatch({
        type: 'UPDATE_DATA_SETS',
        payload: newData,
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
    ({ isValid, ...chartOptions }: ChartOptions & { isValid: boolean }) => {
      dispatch({ type: 'UPDATE_VALID', payload: isValid });
      dispatch({
        type: 'UPDATE_CHART_OPTIONS',
        payload: chartOptions,
      });
    },
    [dispatch],
  );

  const updateChartAxis = useCallback(
    ({
      isValid,
      ...axisConfiguration
    }: AxisConfiguration & { isValid: boolean }) => {
      dispatch({ type: 'UPDATE_VALID', payload: isValid });
      dispatch({
        type: 'UPDATE_CHART_AXIS',
        payload: axisConfiguration,
      });
    },
    [dispatch],
  );

  const actions = useMemo(
    () => ({
      addDataSet,
      removeDataSet,
      updateDataSet,
      updateChartDefinition,
      updateChartOptions,
      updateChartAxis,
    }),
    [
      addDataSet,
      removeDataSet,
      updateChartAxis,
      updateChartDefinition,
      updateChartOptions,
      updateDataSet,
    ],
  );

  return {
    state,
    dispatch,
    actions,
  };
}

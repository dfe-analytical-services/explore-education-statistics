import { ChartRendererProps } from '@common/modules/charts/components/ChartRenderer';
import {
  AxesConfiguration,
  AxisConfiguration,
  AxisType,
  ChartDataSet,
  ChartDefinition,
  ChartDefinitionAxis,
  ChartDefinitionOptions,
  chartDefinitions,
  DataSetConfiguration,
} from '@common/modules/charts/types/chart';
import { generateKeyFromDataSet } from '@common/modules/charts/util/chartUtils';
import { Chart } from '@common/services/publicationService';
import mapValues from 'lodash/mapValues';
import { useCallback, useMemo } from 'react';
import { Reducer, useImmerReducer } from 'use-immer';

export interface ChartOptions extends ChartDefinitionOptions {
  fileId?: string;
  geographicId?: string;
}

export interface ChartDataSetAndConfiguration {
  dataSet: ChartDataSet;
  configuration: DataSetConfiguration;
}

export interface ChartBuilderState {
  definition?: ChartDefinition;
  options: ChartOptions;
  dataSetAndConfiguration: ChartDataSetAndConfiguration[];
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
      payload: ChartDataSetAndConfiguration;
    }
  | { type: 'REMOVE_DATA_SET'; payload: number }
  | {
      type: 'UPDATE_DATA_SETS';
      payload: ChartDataSetAndConfiguration[];
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
  min: 0,
  referenceLines: [],
  showGrid: true,
  size: 50,
  sortAsc: true,
  sortBy: 'name',
  tickConfig: 'default',
  tickSpacing: 1,
  visible: true,
  unit: '',
};

export const chartBuilderReducer: Reducer<
  ChartBuilderState,
  ChartBuilderActions
> = (draft, action) => {
  const updateAxis = (
    next: AxisConfiguration,
    axisDefinition: ChartDefinitionAxis,
  ): AxisConfiguration => {
    const current: Partial<AxisConfiguration> = draft.axes[next.type] ?? {};

    return {
      ...defaultAxisOptions,
      ...(axisDefinition.defaults ?? {}),
      ...current,
      ...next,
      ...(axisDefinition.constants ?? {}),
      type: axisDefinition.type,
    };
  };

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
            draft.axes[type] ?? ({} as AxisConfiguration),
            axisDefinition,
          );
        },
      );

      break;
    }
    case 'UPDATE_CHART_AXIS': {
      const axisDefinition = draft?.definition?.axes?.[action.payload.type];

      if (!axisDefinition) {
        throw new Error(
          `Could not find chart axis definition with type '${action.payload.type}'`,
        );
      }

      draft.axes[action.payload.type] = updateAxis(
        action.payload,
        axisDefinition,
      );

      break;
    }
    case 'UPDATE_CHART_OPTIONS': {
      draft.options = {
        ...defaultOptions,
        ...(draft?.definition?.options.defaults ?? {}),
        ...action.payload,
        ...(draft?.definition?.options.constants ?? {}),
      };

      break;
    }
    case 'ADD_DATA_SET': {
      draft.dataSetAndConfiguration.push(action.payload);

      if (typeof draft.isValid === 'undefined') {
        draft.isValid = true;
      }

      break;
    }
    case 'REMOVE_DATA_SET': {
      draft.dataSetAndConfiguration.splice(action.payload, 1);

      if (typeof draft.isValid === 'undefined') {
        draft.isValid = draft.dataSetAndConfiguration.length > 0;
      }

      break;
    }
    case 'UPDATE_DATA_SETS': {
      draft.dataSetAndConfiguration = action.payload;

      if (typeof draft.isValid === 'undefined') {
        draft.isValid = draft.dataSetAndConfiguration.length > 0;
      }

      break;
    }
    case 'UPDATE_VALID':
      draft.isValid = action.payload;
      break;
    default:
      break;
  }

  Object.values(draft.axes as Required<AxesConfiguration>).forEach(axis => {
    // eslint-disable-next-line no-param-reassign
    axis.dataSets =
      axis.type === 'major'
        ? draft.dataSetAndConfiguration.map(dsc => dsc.dataSet)
        : [];
  });
};

export function useChartBuilderReducer(initialConfiguration?: Chart) {
  const [state, dispatch] = useImmerReducer<
    ChartBuilderState,
    ChartBuilderActions
  >(
    chartBuilderReducer,
    {
      axes: initialConfiguration?.axes ?? {},
      definition: initialConfiguration
        ? chartDefinitions.find(
            ({ type }) => type === initialConfiguration.type,
          )
        : undefined,
      options: initialConfiguration ?? {
        height: 300,
        title: '',
      },
      dataSetAndConfiguration: [],
    },
    initial => {
      let dataSetAndConfiguration: ChartDataSetAndConfiguration[] = [];

      if (
        initialConfiguration?.axes?.major?.dataSets &&
        initialConfiguration?.labels
      ) {
        dataSetAndConfiguration = initialConfiguration.axes.major.dataSets
          .map(dataSet => {
            const key = generateKeyFromDataSet(dataSet);
            const configuration =
              initialConfiguration.labels && initialConfiguration.labels[key];
            return { dataSet, configuration };
          })
          .filter(
            dsc => typeof dsc.configuration !== 'undefined',
          ) as ChartDataSetAndConfiguration[];
      }

      return {
        ...initial,
        dataSetAndConfiguration,
      };
    },
  );

  const addDataSet = useCallback(
    (addedData: ChartDataSetAndConfiguration) => {
      dispatch({
        type: 'ADD_DATA_SET',
        payload: addedData,
      });
    },
    [dispatch],
  );

  const removeDataSet = useCallback(
    (removedData: ChartDataSetAndConfiguration, index: number) => {
      dispatch({
        type: 'REMOVE_DATA_SET',
        payload: index,
      });
    },
    [dispatch],
  );

  const updateDataSet = useCallback(
    (newData: ChartDataSetAndConfiguration[]) => {
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

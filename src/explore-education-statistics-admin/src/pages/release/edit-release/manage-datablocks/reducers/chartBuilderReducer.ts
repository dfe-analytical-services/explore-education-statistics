import { useLoggedImmerReducer } from '@common/hooks/useLoggedReducer';
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
import { Dictionary } from '@common/types';
import mapValues from 'lodash/mapValues';
import omit from 'lodash/omit';
import { useCallback, useMemo } from 'react';
import { Reducer } from 'use-immer';

export interface ChartOptions extends ChartDefinitionOptions {
  fileId?: string;
  geographicId?: string;
}

export interface FormState {
  isValid: boolean;
  submitCount: number;
}

export interface ChartBuilderState {
  definition?: ChartDefinition;
  options: ChartOptions;
  axes: AxesConfiguration;
  chartProps?: ChartRendererProps;
  forms: {
    data: FormState;
    options: FormState;
    [form: string]: FormState;
  };
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
      type: 'UPDATE_FORM';
      payload: {
        form: keyof ChartBuilderState['forms'];
        state: FormState;
      };
    }
  | {
      type: 'RESET';
    };

const defaultOptions: Partial<ChartOptions> = {
  title: '',
};

const defaultAxisOptions: Partial<AxisConfiguration> = {
  dataSets: [],
  referenceLines: [],
  visible: true,
  label: {
    text: '',
  },
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

const getInitialState = (initialConfiguration?: Chart): ChartBuilderState => {
  const definition = chartDefinitions.find(
    ({ type }) => type === initialConfiguration?.type,
  );

  // Make sure height is never actually 0 or negative
  // as this wouldn't make sense for any chart.
  const height =
    typeof initialConfiguration?.height !== 'undefined' &&
    initialConfiguration?.height > 0
      ? initialConfiguration.height
      : definition?.options?.defaults?.height ?? 300;

  const initialState: ChartBuilderState = {
    axes: {},
    definition,
    options: {
      ...omit(initialConfiguration ?? {}, ['axes', 'type']),
      title: initialConfiguration?.title ?? '',
      alt: initialConfiguration?.alt ?? '',
      height,
    },
    forms: {
      data: { isValid: true, submitCount: 0 },
      options: { isValid: true, submitCount: 0 },
    },
  };

  if (!initialConfiguration) {
    return initialState;
  }

  const axes: AxesConfiguration = mapValues(
    initialState.definition?.axes ?? {},
    (axisDefinition: ChartDefinitionAxis, type: AxisType) => {
      return updateAxis(
        axisDefinition,
        (initialConfiguration.axes[type] ?? {}) as AxisConfiguration,
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

  const forms: ChartBuilderState['forms'] = {
    ...initialState.forms,
    ...mapValues(axes as Required<AxesConfiguration>, () => {
      const formState: FormState = {
        isValid: true,
        submitCount: 0,
      };

      return formState;
    }),
  };

  return {
    ...initialState,
    axes,
    forms,
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

      const newAxisForms: Dictionary<FormState> = mapValues(
        draft.definition.axes as Required<ChartDefinition['axes']>,
        _ => {
          return {
            isValid: true,
            submitCount: 0,
          };
        },
      );

      draft.forms = {
        ...newAxisForms,
        options: draft.forms.options,
        data: draft.forms.data,
      };

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

      draft.forms.data.isValid = true;

      break;
    }
    case 'REMOVE_DATA_SET': {
      if (draft.axes.major) {
        draft.axes.major.dataSets.splice(action.payload, 1);

        draft.forms.data.isValid = draft.axes.major.dataSets.length > 0;
      }

      break;
    }
    case 'UPDATE_DATA_SETS': {
      if (draft.axes.major) {
        draft.axes.major.dataSets = action.payload;

        draft.forms.data.isValid = draft.axes.major.dataSets.length > 0;
      }

      break;
    }
    case 'UPDATE_FORM':
      if (!draft.forms[action.payload.form]) {
        throw new Error(
          `Could not find form '${action.payload.form}' to update`,
        );
      }

      draft.forms[action.payload.form] = action.payload.state;

      break;
    case 'RESET':
      return getInitialState();
    default:
      break;
  }

  return draft;
};

export function useChartBuilderReducer(initialConfiguration?: Chart) {
  const [state, dispatch] = useLoggedImmerReducer<
    ChartBuilderState,
    ChartBuilderActions
  >(
    'Chart builder',
    chartBuilderReducer,
    getInitialState(initialConfiguration),
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

  const updateDataSets = useCallback(
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
    (chartOptions: ChartOptions) => {
      dispatch({
        type: 'UPDATE_CHART_OPTIONS',
        payload: chartOptions,
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

  const updateFormState = useCallback(
    ({
      form,
      ...formState
    }: {
      form: keyof ChartBuilderState['forms'];
    } & FormState) => {
      dispatch({
        type: 'UPDATE_FORM',
        payload: {
          form,
          state: formState,
        },
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
      addDataSet,
      removeDataSet,
      updateDataSets,
      updateChartDefinition,
      updateChartOptions,
      updateChartAxis,
      updateFormState,
      resetState,
    }),
    [
      addDataSet,
      removeDataSet,
      updateChartAxis,
      updateChartDefinition,
      updateChartOptions,
      updateDataSets,
      updateFormState,
      resetState,
    ],
  );

  return {
    state,
    dispatch,
    actions,
  };
}

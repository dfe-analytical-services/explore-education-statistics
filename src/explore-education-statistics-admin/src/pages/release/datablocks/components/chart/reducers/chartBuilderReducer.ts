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
import { Dictionary } from '@common/types';
import deepMerge from 'deepmerge';
import mapValues from 'lodash/mapValues';
import { useCallback, useMemo } from 'react';
import { Reducer } from 'use-immer';

export interface ChartOptions extends ChartDefinitionOptions {
  file?: File;
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
  legend?: LegendConfiguration;
  forms: {
    data: FormState;
    legend: FormState;
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
      type: 'UPDATE_FORM';
      payload: {
        form: keyof ChartBuilderState['forms'];
        state: FormState;
      };
    }
  | {
      type: 'RESET';
    };

const defaultOptions: ChartOptions = {
  height: 300,
  title: '',
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

const getInitialState = (initialConfiguration?: Chart): ChartBuilderState => {
  const { type, axes, height, legend, ...options } = initialConfiguration ?? {};

  const definition = chartDefinitions.find(
    chartDefinition => chartDefinition.type === type,
  );

  const initialState: ChartBuilderState = {
    axes: {},
    definition,
    options: {
      ...defaultOptions,
      ...(options ?? {}),
      height:
        // Make sure height is never actually 0 or negative
        // as this wouldn't make sense for any chart.
        typeof height !== 'undefined' && height > 0
          ? height
          : definition?.options?.defaults?.height ?? 300,
    },
    forms: {
      data: { isValid: true, submitCount: 0 },
      legend: { isValid: true, submitCount: 0 },
      options: { isValid: true, submitCount: 0 },
    },
  };

  if (!initialConfiguration) {
    return initialState;
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
    legend: {
      ...defaultLegend,
      ...(legend ?? {}),
    },
    axes: mapValues(
      initialState.definition?.axes ?? {},
      (axisDefinition: ChartDefinitionAxis, axisType: AxisType) =>
        updateAxis(
          axisDefinition,
          (axes?.[axisType] ?? {}) as AxisConfiguration,
        ),
    ),
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
        legend: draft.forms.legend,
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

      draft.forms.data.isValid = action.payload.length > 0;

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
      updateDataSets,
      updateChartDefinition,
      updateChartLegend,
      updateChartOptions,
      updateChartAxis,
      updateFormState,
      resetState,
    }),
    [
      updateDataSets,
      updateChartAxis,
      updateChartDefinition,
      updateChartLegend,
      updateChartOptions,
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

import { AxisType, ChartDefinition } from '@common/modules/charts/types/chart';
import { Dictionary } from '@common/types';
import mapValues from 'lodash/mapValues';
import React, {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';

export interface FormState {
  isValid: boolean;
  submitCount: number;
}

export interface ChartBuilderForm extends FormState {
  title: string;
  id: string;
}

export type ChartBuilderFormKeys = 'options' | 'dataSets' | 'legend' | AxisType;

export interface ChartBuilderForms {
  options: ChartBuilderForm;
  dataSets?: ChartBuilderForm;
  legend?: ChartBuilderForm;
  major?: ChartBuilderForm;
  minor?: ChartBuilderForm;
}

interface UpdateFormState extends Partial<FormState> {
  formKey: keyof ChartBuilderForms;
}

export interface ChartBuilderFormsContextValue {
  forms: ChartBuilderForms;
  hasSubmitted: boolean;
  isSubmitting: boolean;
  isValid: boolean;
  submit: () => void;
  updateForm: (nextState: UpdateFormState) => void;
}

const ChartBuilderFormsContext = createContext<
  ChartBuilderFormsContextValue | undefined
>(undefined);

const defaultFormState = (): FormState => {
  return {
    isValid: false,
    submitCount: 0,
  };
};

export interface ChartBuilderFormContextProviderProps {
  children: ReactNode | ((value: ChartBuilderFormsContextValue) => ReactNode);
  definition?: ChartDefinition;
  id?: string;
  initialForms?: ChartBuilderForms;
  onSubmit?: () => void;
}

export const ChartBuilderFormsContextProvider = ({
  children,
  definition,
  id = 'chartBuilder',
  initialForms,
  onSubmit,
}: ChartBuilderFormContextProviderProps) => {
  const [isSubmitting, setSubmitting] = useState(false);
  const [forms, setForms] = useState<ChartBuilderForms>(() => {
    if (initialForms) {
      return initialForms;
    }

    return {
      options: {
        ...defaultFormState(),
        id: `${id}-options`,
        title: 'Chart configuration',
      },
    };
  });

  useEffect(() => {
    if (!definition) {
      return;
    }

    setForms(prevForms => {
      const nextForms: Partial<ChartBuilderForms> = {};

      if (definition.capabilities.hasLegend) {
        nextForms.legend = {
          ...defaultFormState(),
          ...(prevForms.legend ?? {}),
          id: `${id}-legend`,
          title: 'Legend',
        };
      }

      if (definition.axes.major) {
        nextForms.dataSets = {
          ...defaultFormState(),
          ...(prevForms.dataSets ?? {}),
          id: `${id}-dataSets`,
          title: 'Data sets',
        };
      }

      const nextAxisForms: Dictionary<ChartBuilderForm> = mapValues(
        definition.axes as Required<ChartDefinition['axes']>,
        axis => {
          const form: ChartBuilderForm = {
            submitCount: 0,
            isValid: !!axis.hide,
            ...(prevForms[axis.type] ?? {}),
            id: `${id}-${axis.id}`,
            title: axis.title,
          };

          return form;
        },
      );

      return {
        ...nextForms,
        ...nextAxisForms,
        options: prevForms.options,
      };
    });
  }, [definition, id]);

  const updateForm = useCallback(
    ({ formKey, ...nextFormState }: UpdateFormState) => {
      setForms(prevForms => {
        const prevFormState = prevForms[formKey];

        if (!prevFormState) {
          throw new Error(
            `Cannot update form with key '${formKey}' as it does not exist in state`,
          );
        }

        return {
          ...prevForms,
          [formKey]: {
            ...prevFormState,
            ...nextFormState,
          },
        };
      });
    },
    [],
  );

  const submit = useCallback(async () => {
    if (!onSubmit) {
      return;
    }

    if (Object.values(forms).every(form => form.isValid)) {
      setSubmitting(true);
      await onSubmit();
      setSubmitting(false);
    }
  }, [forms, onSubmit]);

  const value = useMemo<ChartBuilderFormsContextValue>(() => {
    const formValues = Object.values(forms);

    return {
      forms,
      updateForm,
      submit,
      isSubmitting,
      isValid: formValues.every(form => form.isValid),
      hasSubmitted: formValues.some(form => form.submitCount > 0),
    };
  }, [forms, isSubmitting, submit, updateForm]);

  return (
    <ChartBuilderFormsContext.Provider value={value}>
      {typeof children === 'function' ? children(value) : children}
    </ChartBuilderFormsContext.Provider>
  );
};

export function useChartBuilderFormsContext(): ChartBuilderFormsContextValue {
  const context = useContext(ChartBuilderFormsContext);

  if (!context) {
    throw new Error('Must have a parent ChartBuilderFormsContextProvider');
  }

  return context;
}

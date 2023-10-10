import { Path } from '@common/types';
import {
  FieldMessageMapper,
  isServerValidationError,
  mapServerFieldErrors,
} from '@common/validation/serverValidations';
import has from 'lodash/has';
import {
  FieldValues,
  FormProvider as RHFormProvider,
  useForm,
  UseFormHandleSubmit,
  UseFormProps,
  UseFormReturn,
} from 'react-hook-form';
import React, { ReactNode, useCallback, useEffect, useRef } from 'react';
import { yupResolver } from '@hookform/resolvers/yup';
import isEqual from 'lodash/isEqual';
import { ObjectSchema, Schema } from 'yup';

interface FormProviderProps<TFormValues extends FieldValues> {
  children: ReactNode | ((form: UseFormReturn<TFormValues>) => ReactNode);
  enableReinitialize?: boolean;
  errorMappers?:
    | FieldMessageMapper<TFormValues>[]
    | ((values: TFormValues) => FieldMessageMapper<TFormValues>[]);
  fallbackErrorMapper?: FieldMessageMapper<TFormValues>;
  initialValues?: UseFormProps<TFormValues>['defaultValues'];
  validationSchema?: ObjectSchema<TFormValues> & Schema<TFormValues>;
}

export default function FormProvider<TFormValues extends FieldValues>({
  children,
  enableReinitialize,
  errorMappers = [],
  fallbackErrorMapper,
  initialValues,
  validationSchema,
}: FormProviderProps<TFormValues>) {
  const form = useForm<TFormValues>({
    defaultValues: initialValues,
    mode: 'onBlur',
    resolver: validationSchema ? yupResolver(validationSchema) : undefined,
    shouldFocusError: false,
  });

  const previousInitialValues = useRef(initialValues);

  /**
   * RHF caches default values and doesn't have a built-in option
   * to reinitialise forms.
   * This use effect provides the equivalent functionality as
   * Formik's `enableReinitialize` by checking for changes to the
   * initialValues and resetting the form with the new values if they've changed.
   */
  useEffect(() => {
    if (
      enableReinitialize &&
      !isEqual(previousInitialValues.current, initialValues)
    ) {
      form.reset(initialValues as TFormValues);
      previousInitialValues.current = initialValues;
    }
  }, [enableReinitialize, form, initialValues]);

  const handleSubmit: UseFormHandleSubmit<TFormValues> = useCallback(
    (onValid, onInvalid) => {
      return form.handleSubmit(async (values, event) => {
        try {
          await onValid(values, event);
        } catch (error) {
          if (isServerValidationError(error) && error.response?.data) {
            const fieldErrors = mapServerFieldErrors(
              error.response.data,
              typeof errorMappers === 'function'
                ? errorMappers(values as TFormValues)
                : errorMappers,
              fallbackErrorMapper,
            );

            fieldErrors.forEach(({ field, message }) => {
              if (has(values, field)) {
                form.setError(field as Path<TFormValues>, { message });
              }
            });

            return;
          }

          throw error;
        }
      }, onInvalid);
    },
    [errorMappers, fallbackErrorMapper, form],
  );

  const providerProps = {
    ...form,
    handleSubmit,
  };

  return (
    <RHFormProvider {...providerProps}>
      {typeof children === 'function' ? children(providerProps) : children}
    </RHFormProvider>
  );
}

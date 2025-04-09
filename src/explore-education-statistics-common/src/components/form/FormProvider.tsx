import SubmitError from '@common/components/form/util/SubmitError';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import logger from '@common/services/logger';
import { Path } from '@common/types';
import isErrorLike from '@common/utils/error/isErrorLike';
import {
  FieldMessageMapper,
  isServerValidationError,
  mapServerFieldErrors,
} from '@common/validation/serverValidations';
import has from 'lodash/has';
import {
  FieldValues,
  FormProvider as RHFFormProvider,
  useForm,
  UseFormHandleSubmit,
  UseFormProps,
  UseFormReturn,
} from 'react-hook-form';
import React, { ReactNode, useCallback, useEffect, useRef } from 'react';
import { yupResolver } from '@hookform/resolvers/yup';
import isEqual from 'lodash/isEqual';
import { ObjectSchema, Schema } from 'yup';

export interface FormProviderProps<TFormValues extends FieldValues> {
  children: ReactNode | ((form: UseFormReturn<TFormValues>) => ReactNode);
  enableReinitialize?: boolean;
  errorMappings?:
    | FieldMessageMapper<TFormValues>[]
    | ((values: TFormValues) => FieldMessageMapper<TFormValues>[]);
  fallbackServerValidationError?: string;
  fallbackErrorMapping?: FieldMessageMapper<TFormValues>;
  fallbackSubmitError?: string;
  initialValues?: UseFormProps<TFormValues>['defaultValues'];
  mode?: 'onChange' | 'onBlur' | 'onSubmit' | 'onTouched' | 'all';
  resetAfterSubmit?: boolean;
  reValidateMode?: 'onChange' | 'onBlur' | 'onSubmit';
  validationSchema?: ObjectSchema<TFormValues> & Schema<TFormValues>;
}

export default function FormProvider<TFormValues extends FieldValues>({
  children,
  enableReinitialize,
  errorMappings = [],
  fallbackErrorMapping,
  fallbackServerValidationError = 'The form submission is invalid and could not be processed',
  fallbackSubmitError = 'Something went wrong whilst submitting the form',
  initialValues,
  mode = 'onSubmit',
  resetAfterSubmit = false,
  reValidateMode = 'onChange',
  validationSchema,
}: FormProviderProps<TFormValues>) {
  const form = useForm<TFormValues>({
    defaultValues: initialValues,
    mode,
    reValidateMode,
    resolver: validationSchema ? yupResolver(validationSchema) : undefined,
    shouldFocusError: false,
  });

  const { handleError } = useErrorControl();

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
          if (resetAfterSubmit) {
            form.reset();
          }
        } catch (error) {
          if (isServerValidationError(error) && error.response?.data) {
            const fieldErrors = mapServerFieldErrors(
              error.response.data,
              typeof errorMappings === 'function'
                ? errorMappings(values as TFormValues)
                : errorMappings,
              fallbackErrorMapping,
            );

            const mappableErrors = fieldErrors.filter(({ field }) =>
              has(values, field),
            );

            if (mappableErrors.length > 0) {
              mappableErrors.forEach(({ field, message }) => {
                form.setError(field as Path<TFormValues>, { message });
              });
            } else {
              form.setError('root', {
                message: fallbackServerValidationError,
              });
            }

            return;
          }

          if (isErrorLike(error)) {
            logger.error(error);

            if (error instanceof SubmitError) {
              form.setError(
                error.field && has(values, error.field)
                  ? (error.field as Path<TFormValues>)
                  : 'root',
                {
                  message: error.message,
                },
              );
            } else {
              form.setError('root', {
                message: fallbackSubmitError,
              });
            }

            return;
          }

          handleError(error);
        }
      }, onInvalid);
    },
    [
      errorMappings,
      fallbackErrorMapping,
      fallbackSubmitError,
      fallbackServerValidationError,
      form,
      handleError,
      resetAfterSubmit,
    ],
  );

  const providerProps = {
    ...form,
    handleSubmit,
  };

  return (
    <RHFFormProvider {...providerProps}>
      {typeof children === 'function' ? children(providerProps) : children}
    </RHFFormProvider>
  );
}

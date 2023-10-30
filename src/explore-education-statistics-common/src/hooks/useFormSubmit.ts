import SubmitError from '@common/components/form/util/SubmitError';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import logger from '@common/services/logger';
import { Path } from '@common/types';
import isErrorLike from '@common/utils/error/isErrorLike';
import {
  convertServerFieldErrors,
  FieldMessageMapper,
  isServerValidationError,
} from '@common/validation/serverValidations';
import { FormikHelpers } from 'formik';
import has from 'lodash/has';
import { useMemo } from 'react';

export type UseFormSubmit<FormValues> = (
  values: FormValues,
  formikActions: FormikHelpers<FormValues>,
) => Promise<void> | void;

export default function useFormSubmit<FormValues>(
  onSubmit: UseFormSubmit<FormValues>,
  errorMappings:
    | FieldMessageMapper<FormValues>[]
    | ((values: FormValues) => FieldMessageMapper<FormValues>[]) = [],
  options: {
    fallbackErrorMapping?: FieldMessageMapper<FormValues>;
    fallbackServerValidationError?: string;
    fallbackSubmitError?: string;
  } = {},
) {
  const {
    fallbackErrorMapping,
    fallbackSubmitError = 'Something went wrong whilst submitting the form',
    fallbackServerValidationError = 'The form submission is invalid and could not be processed',
  } = options;

  const { handleError } = useErrorControl();

  return useMemo(
    () => async (values: FormValues, helpers: FormikHelpers<FormValues>) => {
      try {
        await onSubmit(values, helpers);
      } catch (error) {
        if (isServerValidationError(error) && error.response?.data) {
          const errors = convertServerFieldErrors(
            error.response.data,
            values,
            typeof errorMappings === 'function'
              ? errorMappings(values)
              : errorMappings,
            fallbackErrorMapping,
          );

          if (Object.values(errors).length > 0) {
            helpers.setErrors(errors);
          } else {
            helpers.setFieldError('root', fallbackServerValidationError);
          }

          return;
        }

        if (isErrorLike(error)) {
          logger.error(error);

          if (error instanceof SubmitError) {
            helpers.setFieldError(
              error.field && has(values, error.field)
                ? (error.field as Path<FormValues>)
                : 'root',
              error.message,
            );
          } else {
            helpers.setFieldError('root', fallbackSubmitError);
          }

          return;
        }

        handleError(error);
      }
    },
    [
      onSubmit,
      handleError,
      errorMappings,
      fallbackErrorMapping,
      fallbackSubmitError,
      fallbackServerValidationError,
    ],
  );
}

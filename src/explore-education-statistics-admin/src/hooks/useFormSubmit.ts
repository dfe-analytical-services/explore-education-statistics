import { useErrorControl } from '@common/contexts/ErrorControlContext';
import isAxiosError from '@common/utils/error/isAxiosError';
import {
  convertServerFieldErrors,
  ServerValidationErrorResponse,
  FieldMessageMapper,
} from '@common/validation/serverValidations';
import { AxiosError } from 'axios';
import { FormikHelpers } from 'formik';
import { useMemo } from 'react';

export type UseFormSubmit<FormValues> = (
  values: FormValues,
  formikActions: FormikHelpers<FormValues>,
) => Promise<void> | void;

const isServerValidationError = (
  error: Error,
): error is AxiosError<ServerValidationErrorResponse> => {
  if (!isAxiosError(error) || !error.response?.data) {
    return false;
  }

  const errorDataAsValidationError = error.response
    .data as ServerValidationErrorResponse;

  return (
    errorDataAsValidationError.errors !== undefined &&
    errorDataAsValidationError.status !== undefined &&
    errorDataAsValidationError.title !== undefined
  );
};
function useFormSubmit<FormValues>(
  onSubmit: UseFormSubmit<FormValues>,
  errorMappers: FieldMessageMapper<FormValues>[] = [],
) {
  const { handleApiErrors } = useErrorControl();

  return useMemo(
    () => async (values: FormValues, actions: FormikHelpers<FormValues>) => {
      try {
        await onSubmit(values, actions);
      } catch (error) {
        if (isServerValidationError(error) && error.response?.data) {
          const errors = convertServerFieldErrors(
            error.response?.data,
            errorMappers,
          );

          if (Object.values(errors).length) {
            actions.setErrors(errors);
          } else {
            handleApiErrors(error);
          }
        } else {
          handleApiErrors(error);
        }
      }
    },
    [onSubmit, handleApiErrors, errorMappers],
  );
}

export default useFormSubmit;

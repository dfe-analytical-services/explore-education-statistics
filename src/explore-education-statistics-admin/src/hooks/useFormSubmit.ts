import handleServerSideValidation, {
  ServerValidationErrors,
  ServerValidationMessageMapper,
} from '@common/components/form/util/serverValidationHandler';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import { AxiosError } from 'axios';
import { FormikActions } from 'formik';
import { useMemo } from 'react';

export type UseFormSubmit<FormValues> = (
  values: FormValues,
  formikActions: FormikActions<FormValues>,
) => Promise<void> | void;

const isServerValidationError = (error: AxiosError) => {
  if (!error.isAxiosError || !error.response?.data) {
    return false;
  }

  const errorDataAsValidationError = error.response
    .data as ServerValidationErrors;

  return (
    errorDataAsValidationError.errors !== undefined &&
    errorDataAsValidationError.status !== undefined &&
    errorDataAsValidationError.title !== undefined
  );
};
function useFormSubmit<FormValues>(
  onSubmit: UseFormSubmit<FormValues>,
  errorMappers: ServerValidationMessageMapper[] = [],
) {
  const { handleApiErrors } = useErrorControl();

  return useMemo(
    () => async (values: FormValues, actions: FormikActions<FormValues>) => {
      try {
        await onSubmit(values, actions);
      } catch (error) {
        const typedError: AxiosError = error;

        if (!isServerValidationError(typedError)) {
          handleApiErrors(typedError);
        } else {
          const isErrorHandled = handleServerSideValidation(
            errorMappers,
            typedError.response?.data,
            actions.setFieldError,
            actions.setError,
          );

          if (!isErrorHandled) {
            handleApiErrors(typedError);
          }
        }
      }
    },
    [onSubmit, handleApiErrors, errorMappers],
  );
}

export default useFormSubmit;

import { useErrorControl } from '@common/contexts/ErrorControlContext';
import {
  convertServerFieldErrors,
  FieldMessageMapper,
  isServerValidationError,
} from '@common/validation/serverValidations';
import { FormikHelpers } from 'formik';
import { useMemo } from 'react';

export type UseFormSubmit<FormValues> = (
  values: FormValues,
  formikActions: FormikHelpers<FormValues>,
) => Promise<void> | void;

function useFormSubmit<FormValues>(
  onSubmit: UseFormSubmit<FormValues>,
  errorMappers:
    | FieldMessageMapper<FormValues>[]
    | ((values: FormValues) => FieldMessageMapper<FormValues>[]) = [],
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
            typeof errorMappers === 'function'
              ? errorMappers(values)
              : errorMappers,
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

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
  // eslint-disable-next-line default-param-last
  errorMappers:
    | FieldMessageMapper<FormValues>[]
    | ((values: FormValues) => FieldMessageMapper<FormValues>[]) = [],
  fallbackErrorMapper?: FieldMessageMapper<FormValues>,
) {
  const { handleError } = useErrorControl();

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
            fallbackErrorMapper,
          );

          if (Object.values(errors).length) {
            actions.setErrors(errors);
          } else {
            handleError(error);
          }
        } else {
          handleError(error);
        }
      }
    },
    [onSubmit, handleError, errorMappers, fallbackErrorMapper],
  );
}

export default useFormSubmit;

import handleServerSideValidation, {
  ServerValidationErrors,
  ServerValidationMessageMapper,
} from '@common/components/form/util/serverValidationHandler';
import { AxiosErrorHandler } from '@common/services/api/Client';
import { AxiosError } from 'axios';
import { FormikActions } from 'formik';

export type FormikSubmitHandler<FormValues> = (
  values: FormValues,
  formikActions: FormikActions<FormValues>,
) => Promise<void>;

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

function submitWithFormikValidation<FormValues>(
  submitFn: FormikSubmitHandler<FormValues>,
  handleApiErrors: AxiosErrorHandler,
  ...messageMappers: ServerValidationMessageMapper[]
) {
  return async (values: FormValues, actions: FormikActions<FormValues>) => {
    try {
      await submitFn(values, actions);
    } catch (error) {
      const typedError: AxiosError = error;

      if (!isServerValidationError(typedError)) {
        handleApiErrors(typedError);
      } else {
        const validationHandler = handleServerSideValidation(...messageMappers);

        const errorHandled = validationHandler(
          typedError.response?.data,
          actions.setFieldError,
          actions.setError,
        );

        if (!errorHandled) {
          handleApiErrors(typedError);
        }
      }
    }
  };
}

export default submitWithFormikValidation;

import { FormValues } from '@admin/pages/release/create-release/CreateReleasePage';
import handleServerSideValidation, {
  ServerValidationErrors,
  ServerValidationMessageMapper,
} from '@common/components/form/util/serverValidationHandler';
import { AxiosResponse } from 'axios';
import { FormikActions } from 'formik';

type FormikSubmitHandler<FormValues> = (
  values: FormValues,
  formikActions: FormikActions<FormValues>,
) => Promise<void>;

const isServerValidationError = <T extends {}>(error: T) => {
  const errorWithData = (error as unknown) as { data: unknown };
  const { data } = errorWithData;

  if (!data) {
    return false;
  }

  const errorDataAsValidationError = (data as unknown) as ServerValidationErrors;
  return (
    errorDataAsValidationError.errors !== undefined &&
    errorDataAsValidationError.status !== undefined &&
    errorDataAsValidationError.title !== undefined
  );
};

const submitWithFormikValidation: (
  submitFn: FormikSubmitHandler<FormValues>,
  fallbackErrorHandler: (error: AxiosResponse) => void,
  ...messageMappers: ServerValidationMessageMapper[]
) => FormikSubmitHandler<FormValues> = (
  submitFn,
  fallbackErrorHandler,
  ...messageMappers
) => {
  return async (values: FormValues, actions: FormikActions<FormValues>) => {
    try {
      await submitFn(values, actions);
    } catch (error) {
      if (!isServerValidationError(error)) {
        fallbackErrorHandler(error);
        return;
      }

      const validationHandler = handleServerSideValidation(...messageMappers);

      const errorHandled = validationHandler(
        error.data as ServerValidationErrors,
        actions.setFieldError,
        actions.setError,
      );

      if (!errorHandled) {
        fallbackErrorHandler(error);
      }
    }
  };
};

export default submitWithFormikValidation;

import { ApiErrorHandler } from '@admin/validation/withErrorControl';
import handleServerSideValidation, {
  ServerValidationErrors,
  ServerValidationMessageMapper,
} from '@common/components/form/util/serverValidationHandler';
import { FormikActions } from 'formik';

export type FormikSubmitHandler<FormValues> = (
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

const submitWithFormikValidation = <FormValues>(
  submitFn: FormikSubmitHandler<FormValues>,
  handleApiErrors: ApiErrorHandler,
  ...messageMappers: ServerValidationMessageMapper[]
) => {
  return async (values: FormValues, actions: FormikActions<FormValues>) => {
    try {
      await submitFn(values, actions);
    } catch (error) {
      if (!isServerValidationError(error)) {
        handleApiErrors(error);
      } else {
        const validationHandler = handleServerSideValidation(...messageMappers);

        const errorHandled = validationHandler(
          error.data as ServerValidationErrors,
          actions.setFieldError,
          actions.setError,
        );

        if (!errorHandled) {
          handleApiErrors(error);
        }
      }
    }
  };
};

export default submitWithFormikValidation;

import { Dictionary } from '@common/types';
import { FormikActions } from 'formik';

export interface ServerValidationErrors {
  errors: Dictionary<string[]>;
  title: string;
  status: number;
}

export type FieldErrorSetter = (
  fieldName: string,
  errorMessage: string,
) => void;

/**
 * A class that, given a mapping of backend-to-frontend field name mappings and a mapping of backend validation codes
 * to front end friendly messages, handles binding backend field validation to a frontend form.
 */
class ServerValidationHandler {
  private serverToUiFieldNameMappings: Dictionary<string>;

  private serverToUiErrorCodeMappings: Dictionary<string>;

  public constructor(
    serverToUiFieldNameMappings: Dictionary<string>,
    serverToUiErrorCodeMappings: Dictionary<string>,
  ) {
    this.serverToUiFieldNameMappings = serverToUiFieldNameMappings;
    this.serverToUiErrorCodeMappings = serverToUiErrorCodeMappings;
  }

  public handleServerValidationErrors(
    errors: ServerValidationErrors,
    setFieldError: FieldErrorSetter,
  ) {
    Object.keys(errors.errors).forEach(field => {
      errors.errors[field].forEach(fieldError => {
        const fieldName = this.serverToUiFieldNameMappings[field] || field;
        const errorMessage =
          this.serverToUiErrorCodeMappings[fieldError] || fieldError;
        setFieldError(fieldName, errorMessage);
      });
    });
  }
}

const handleServerSideValidation = <T extends {}>(
  serverToUiFieldNameMappings: Dictionary<string>,
  serverToUiErrorCodeMappings: Dictionary<string>,
) => (errors: ServerValidationErrors, { setFieldError }: FormikActions<T>) => {
  const serverValidationHandler = new ServerValidationHandler(
    serverToUiFieldNameMappings,
    serverToUiErrorCodeMappings,
  );
  serverValidationHandler.handleServerValidationErrors(errors, setFieldError);
};

export default handleServerSideValidation;

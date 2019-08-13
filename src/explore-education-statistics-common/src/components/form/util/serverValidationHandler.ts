import {Dictionary} from '@common/types';

export interface ServerValidationErrors {
  errors: Dictionary<string[]>;
  title: string;
  status: number;
}

export interface ServerValidationError {
  fieldName?: string;
  errorCode: string;
}

export type FieldErrorSetter = (
  fieldName: string,
  errorMessage: string,
) => void;

export type GlobalErrorSetter = (errorMessage: string) => void;

interface ServerValidationMessageMapper {
  canHandleMessage: (message: ServerValidationError) => boolean;
  handleMessage: (
    message: ServerValidationError,
    setFieldError: FieldErrorSetter,
    setGlobalError: GlobalErrorSetter,
  ) => void;
}

/**
 * This handler can map a specific error code to a field with a specific message.
 */
class ErrorCodeToFieldErrorMapper implements ServerValidationMessageMapper {
  private errorCode: string;

  private targetFieldName: string;

  private message: string;

  public constructor(
    errorCode: string,
    targetFieldName: string,
    message: string,
  ) {
    this.errorCode = errorCode;
    this.targetFieldName = targetFieldName;
    this.message = message;
  }

  public canHandleMessage: (
    message: ServerValidationError,
  ) => boolean = message => message.errorCode === this.errorCode;

  public handleMessage: (
    message: ServerValidationError,
    setFieldError: FieldErrorSetter,
  ) => void = (message, setFieldError) =>
    setFieldError(this.targetFieldName, this.message);
}

/**
 * This handler can map a specific combination of error code and field name to a field
 * with a specific message.
 */
class ErrorCodeAndFieldNameToFieldErrorMapper
  implements ServerValidationMessageMapper {
  private errorCode: string;

  private sourceFieldName: string;

  private targetFieldName: string;

  private message: string;

  public constructor(
    errorCode: string,
    sourceFieldName: string,
    targetFieldName: string,
    message: string,
  ) {
    this.errorCode = errorCode;
    this.sourceFieldName = sourceFieldName;
    this.targetFieldName = targetFieldName;
    this.message = message;
  }

  public canHandleMessage: (
    message: ServerValidationError,
  ) => boolean = message =>
    message.errorCode === this.errorCode &&
    message.fieldName === this.sourceFieldName;

  public handleMessage: (
    message: ServerValidationError,
    setFieldError: FieldErrorSetter,
  ) => void = (message, setFieldError) =>
    setFieldError(this.targetFieldName, this.message);
}

/**
 * This handler can map a specific error code to a global error message.
 */
class ErrorCodeToGlobalErrorMapper implements ServerValidationMessageMapper {
  private errorCode: string;

  private message: string;

  public constructor(errorCode: string, message: string) {
    this.errorCode = errorCode;
    this.message = message;
  }

  public canHandleMessage: (
    message: ServerValidationError,
  ) => boolean = message => message.errorCode === this.errorCode;

  public handleMessage: (
    message: ServerValidationError,
    _: FieldErrorSetter,
    setGlobalError: GlobalErrorSetter,
  ) => void = (message, _, setGlobalError) => setGlobalError(this.message);
}

/**
 * A class that, given a mapping of backend-to-frontend field name mappings and a mapping of backend validation codes
 * to front end friendly messages, handles binding backend field validation to a frontend form.
 */
class ServerValidationHandler {
  private messageMappers: ServerValidationMessageMapper[];

  public constructor(messageMappers: ServerValidationMessageMapper[]) {
    this.messageMappers = messageMappers;
  }

  public handleServerValidationErrors(
    errors: ServerValidationErrors,
    setFieldError: FieldErrorSetter,
    setGlobalError: GlobalErrorSetter,
  ) {
    Object.keys(errors.errors).forEach(field => {
      errors.errors[field].forEach(errorCode => {
        const validationError: ServerValidationError = {
          fieldName: field && field.length > 0 ? field : undefined,
          errorCode,
        };

        const validMessageMapper = this.messageMappers.find(mapper =>
          mapper.canHandleMessage(validationError),
        );

        if (validMessageMapper) {
          validMessageMapper.handleMessage(
            validationError,
            setFieldError,
            setGlobalError,
          );
        } else {
          setGlobalError('An unexpected error occurred.');
        }
      });
    });
  }
}

/**
 * Register a mapper that can map an error code to a specific field with a specific message.
 *
 * @param errorCode - the server-side error code to match.
 * @param targetFieldName - the UI field name to relate the message to.
 * @param message - the error message to display to the user.
 */
export const errorCodeToFieldError = (
  errorCode: string,
  targetFieldName: string,
  message: string,
) => new ErrorCodeToFieldErrorMapper(errorCode, targetFieldName, message);

/**
 * Register a mapper that can map an error code along with a specific backend field name to a specific field with a
 * specific message.
 *
 * @param errorCode - the server-side error code to match.
 * @param sourceFieldName - the server-side field name to match.
 * @param targetFieldName - the UI field name to relate the message to.
 * @param message - the error message to display to the user.
 */
export const errorCodeAndFieldNameToFieldError = (
  errorCode: string,
  sourceFieldName: string,
  targetFieldName: string,
  message: string,
) =>
  new ErrorCodeAndFieldNameToFieldErrorMapper(
    errorCode,
    sourceFieldName,
    targetFieldName,
    message,
  );

/**
 * Register a mapper that can map an error code to a global message.
 *
 * @param errorCode - the server-side error code to match.
 * @param message - the error message to display to the user.
 */
export const errorCodeToGlobalError = (errorCode: string, message: string) =>
  new ErrorCodeToGlobalErrorMapper(errorCode, message);

/**
 * Create a validation handler that, given a set of message mappers, will link server-side validation errors to
 * field and global error messages on the UI.
 *
 * @param messageMappers - a set of message mappers that can handle the validaiton messages expected from the back end.
 */
const handleServerSideValidation = <T extends {}>(
  ...messageMappers: ServerValidationMessageMapper[]
) => (
  errors: ServerValidationErrors,
  setFieldError: FieldErrorSetter,
  setGlobalError: GlobalErrorSetter,
) => {
  const serverValidationHandler = new ServerValidationHandler(messageMappers);
  serverValidationHandler.handleServerValidationErrors(
    errors,
    setFieldError,
    setGlobalError,
  );
};

export default handleServerSideValidation;

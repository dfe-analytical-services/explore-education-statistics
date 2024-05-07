import {
  ValidationProblemDetails,
  ValidationProblemError,
} from '@common/services/types/problemDetails';
import { Dictionary, Path } from '@common/types';
import { AxiosError, isAxiosError } from 'axios';
import { FormikErrors } from 'formik';
import camelCase from 'lodash/camelCase';
import has from 'lodash/has';
import set from 'lodash/set';
import toPath from 'lodash/toPath';
import { FieldErrors, FieldValues } from 'react-hook-form';

export interface ServerValidationError {
  sourceField?: string;
  message: string;
  code?: string;
}

export type FieldName<FormValues> = FormValues extends Record<string, unknown>
  ? Path<FormValues>
  : string;

export type FieldMessageMapper<FormValues = unknown> = (
  error: ServerValidationError,
) =>
  | {
      targetField: FieldName<FormValues>;
      message: string;
    }
  | undefined;

export type FieldMessage<FormValues> =
  | {
      field: FieldName<FormValues>;
      message: string;
      mapped: true;
    }
  | {
      field: string;
      message: string;
      mapped: false;
    };

/**
 * Map custom server validation error messages to a
 * `target` form field (in the frontend) based on the
 * `message` contents (e.g. a code), and the `source`
 * field path (in the server's validation problem response).
 */
export function mapFieldErrors<FormValues>(options: {
  target: FieldName<FormValues>;
  source?: string;
  messages?: Dictionary<string>;
}): FieldMessageMapper<FormValues> {
  const { target, source, messages } = options;

  return error => {
    if (source) {
      if (source !== error.sourceField) {
        return undefined;
      }

      // As we know what the source field is, and we
      // have not provided any `messages` to map to,
      // we can assume that it is safe to use the
      // server validation error message instead.
      if (!messages && error.message) {
        return {
          targetField: target,
          message: error.message,
        };
      }
      // Source field may be an empty string in many cases.
      // This should probably not be happening as validation
      // errors should usually be associated with a field.
    } else if (error.sourceField && error.sourceField !== target) {
      return undefined;
    }

    const { code } = error;

    // This error code is unmapped
    if (!code || !messages?.[code]) {
      return undefined;
    }

    return {
      targetField: target,
      message: messages[code],
    };
  };
}

/**
 * Add a generic fallback message that targets the
 * `target` form field if no other error handlers
 * have handled the error.
 */
export function mapFallbackFieldError<FormValues>(options: {
  target: FieldName<FormValues>;
  fallbackMessage: string;
}): FieldMessageMapper<FormValues> {
  const { target, fallbackMessage } = options;

  return _ => {
    return {
      targetField: target,
      message: fallbackMessage,
    };
  };
}

function normalizeField<FormValues = unknown>(
  fieldName: string,
): FieldName<FormValues> {
  const path = toPath(fieldName);

  return path.reduce((acc, item) => {
    const prefix = acc === '' ? '' : '.';

    return (
      acc +
      (Number.isNaN(Number(item)) ? prefix + camelCase(item) : `[${item}]`)
    );
  }, '') as FieldName<FormValues>;
}

/**
 * Convert server validation errors to Formik error messages.
 *
 * @param response The server validation error response.
 * @param formValues The form values that were submitted.
 * @param messageMappers Mappings between server validation errors and field error messages.
 * @param fallbackMapper Optional fallback mapper if no mapping is found.
 */
export function convertServerFieldErrors<FormValues>(
  response: ValidationProblemDetails,
  formValues: FormValues,
  messageMappers: FieldMessageMapper<FormValues>[] = [],
  fallbackMapper?: FieldMessageMapper<FormValues>,
): FormikErrors<FormValues> {
  return mapServerFieldErrors(response, messageMappers, fallbackMapper).reduce<
    FormikErrors<FormValues>
  >((errors, { field, message }) => {
    if (has(formValues, field)) {
      set(errors, field, message);
    }

    return errors;
  }, {});
}

export function rhfConvertServerFieldErrors<FormValues extends FieldValues>(
  response: ValidationProblemDetails,
  formValues: FormValues,
  messageMappers: FieldMessageMapper<FormValues>[] = [],
  fallbackMapper?: FieldMessageMapper<FormValues>,
): FieldErrors<FormValues> {
  return mapServerFieldErrors(response, messageMappers, fallbackMapper).reduce<
    FieldErrors<FormValues>
  >((errors, { field, message }) => {
    if (has(formValues, field)) {
      set(errors, field, { message });
    }

    return errors;
  }, {});
}

/**
 * Map server validation errors to field error messages.
 *
 * @param response The server validation error response.
 * @param messageMappers Mappings between server validation errors and field error messages.
 * @param fallbackMapper Optional fallback mapper if no mapping is found.
 */
export function mapServerFieldErrors<FormValues>(
  response: ValidationProblemDetails,
  messageMappers: FieldMessageMapper<FormValues>[] = [],
  fallbackMapper?: FieldMessageMapper<FormValues>,
): FieldMessage<FormValues>[] {
  return response.errors.reduce<FieldMessage<FormValues>[]>(
    (acc, { code, path, message }) => {
      const sourceField = path ? normalizeField(path) : undefined;

      const error: ServerValidationError = {
        sourceField,
        code,
        message,
      };

      const matchingMappers = messageMappers
        .map(mapper => mapper(error))
        .filter(Boolean);

      if (matchingMappers.length) {
        matchingMappers.forEach(mappedError => {
          if (mappedError) {
            const { targetField, message: mappedMessage } = mappedError;

            acc.push({
              field: targetField,
              message: mappedMessage,
              mapped: true,
            });
          }
        });
      } else if (sourceField) {
        acc.push({
          field: sourceField,
          message,
          mapped: false,
        });
      } else if (fallbackMapper) {
        const mappedFallback = fallbackMapper(error);

        if (mappedFallback) {
          const { targetField, message: mappedMessage } = mappedFallback;

          acc.push({
            field: targetField,
            message: mappedMessage,
            mapped: true,
          });
        }
      }

      return acc;
    },
    [],
  );
}

/**
 * Asserts whether the given error object is an
 * {@link AxiosError<ServerValidationErrorResponse<T>>}.
 *
 * @param error - the error object to check the type of.
 */
export function isServerValidationError<TCode extends string = string>(
  error: unknown,
): error is AxiosError<ValidationProblemDetails<TCode>> {
  if (!isAxiosError(error) || !error.response?.data) {
    return false;
  }

  const errorDataAsValidationError = error.response
    .data as ValidationProblemDetails;

  return (
    errorDataAsValidationError.errors !== undefined &&
    errorDataAsValidationError.status !== undefined &&
    errorDataAsValidationError.title !== undefined
  );
}

/**
 * This method checks whether the error contains any of the error
 * messages provided for a given field.
 *
 * Don't provide a field path if checking global validation errors.
 *
 * If any are included in this error, this method will return true.
 *
 * @param axiosError - the error response to check the messages of.
 * @param errorCodes - array of error codes to check the error response for.
 * @param fieldPath - optional, if omitted, will be treated as checking for global errors.
 */
export function hasErrorMessage<TCode extends string = string>(
  axiosError: AxiosError<ValidationProblemDetails<TCode>>,
  errorCodes: readonly string[],
  fieldPath?: string,
): boolean {
  if (!errorCodes.length) {
    return true;
  }

  return errorCodes.some(code => getErrorCode(axiosError, fieldPath) === code);
}

/**
 * Retrieves the first error message for the given field path, or the
 * first global error message if field path is omitted.
 *
 * @param axiosError - the error containing messages.
 * @param fieldPath - optional, if omitted, will be treated as checking for global errors.
 */
export function getErrorMessage(
  axiosError: AxiosError<ValidationProblemDetails>,
  fieldPath?: string,
): string | undefined {
  return getError(axiosError, fieldPath)?.message;
}

/**
 * Retrieves the first error code for the given field path, or the
 * first global error code if field path is omitted.
 *
 * @param axiosError - the error containing messages.
 * @param fieldPath - optional, if omitted, will be treated as checking for global errors.
 */
export function getErrorCode<TCode extends string = string>(
  axiosError: AxiosError<ValidationProblemDetails<TCode>>,
  fieldPath?: string,
): TCode | undefined {
  return getError(axiosError, fieldPath)?.code;
}

/**
 * Retrieves the first error for the given field path, or the
 * first global error if field path is omitted.
 *
 * @param axiosError
 * @param fieldPath
 */
export function getError<TCode extends string = string>(
  axiosError: AxiosError<ValidationProblemDetails<TCode>>,
  fieldPath?: string,
): ValidationProblemError<TCode> | undefined {
  return axiosError.response?.data?.errors.find(
    error => error.path === fieldPath,
  );
}

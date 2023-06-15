import { Dictionary } from '@common/types';
import isAxiosError from '@common/utils/error/isAxiosError';
import { AxiosError } from 'axios';
import { FormikErrors } from 'formik';
import camelCase from 'lodash/camelCase';
import set from 'lodash/set';
import toPath from 'lodash/toPath';

export interface ServerValidationErrorResponse<T extends string = string> {
  errors: Dictionary<T[]>;
  title: string;
  status: number;
}

export interface ServerValidationError {
  sourceField?: string;
  message: string;
}

export type FieldName<FormValues> = FormValues extends Record<string, unknown>
  ? keyof FormValues
  : string;

export type FieldMessageMapper<FormValues = unknown> = (
  error: ServerValidationError,
) =>
  | {
      targetField: FieldName<FormValues>;
      message: string;
    }
  | undefined;

/**
 * Map custom server validation error messages to a
 * `target` form field (in the frontend) based on the
 * `message` contents (e.g. a code), and the `source`
 * field (in the server validation response).
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
      if (!messages) {
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

    // This error message is unmapped
    if (!messages?.[error.message]) {
      return undefined;
    }

    return {
      targetField: target,
      message: messages[error.message],
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

function normalizeField(fieldName: string): string {
  const path = toPath(fieldName);

  return path.reduce((string, item) => {
    const prefix = string === '' ? '' : '.';

    return (
      string +
      (Number.isNaN(Number(item)) ? prefix + camelCase(item) : `[${item}]`)
    );
  }, '');
}

/**
 * Convert server-side validation error {@param response} to
 * Formik field error messages. The message conversions can be
 * controlled by providing a set of {@param messageMappers}.
 */
export function convertServerFieldErrors<FormValues>(
  response: ServerValidationErrorResponse,
  // eslint-disable-next-line default-param-last
  messageMappers: FieldMessageMapper<FormValues>[] = [],
  fallbackMapper?: FieldMessageMapper<FormValues>,
): FormikErrors<FormValues> {
  return Object.entries(response.errors).reduce<FormikErrors<FormValues>>(
    (acc, [source, messages]) => {
      messages.forEach(message => {
        const sourceField = source ? normalizeField(source) : undefined;

        const error: ServerValidationError = {
          sourceField,
          message,
        };

        const matchingMappers = messageMappers
          .map(mapper => mapper(error))
          .filter(Boolean);

        if (matchingMappers.length) {
          matchingMappers.forEach(mappedError => {
            if (mappedError) {
              const { targetField, message: mappedMessage } = mappedError;
              set(acc, targetField, mappedMessage);
            }
          });
        } else if (sourceField) {
          set(acc, sourceField, message);
        } else if (fallbackMapper) {
          const mappedFallback = fallbackMapper(error);
          if (mappedFallback) {
            const { targetField, message: mappedMessage } = mappedFallback;
            set(acc, targetField, mappedMessage);
          }
        }
      });

      return acc;
    },
    {},
  );
}

/**
 * Asserts whether the given error object is an
 * {@link AxiosError<ServerValidationErrorResponse<T>>}.
 *
 * @param error - the error object to check the type of.
 */
export function isServerValidationError<T extends string = string>(
  error: unknown,
): error is AxiosError<ServerValidationErrorResponse<T>> {
  if (!isAxiosError(error) || !error.response?.data) {
    return false;
  }

  const errorDataAsValidationError = error.response
    .data as ServerValidationErrorResponse<T>;

  return (
    errorDataAsValidationError.errors !== undefined &&
    errorDataAsValidationError.status !== undefined &&
    errorDataAsValidationError.title !== undefined
  );
}

/**
 * This method checks whether or not the error contains any of the error
 * messages provided, using {@param fieldName} to determine field
 * validation, or global validation if {@param fieldName} is omitted.
 *
 * If any are included in this error, this method will return true.
 *
 * @param error - the error to check the messages of.
 * @param errorMessages - array of error messages, any of which can appear
 * in this error in order for this method to return true.
 * @param fieldName - optional fieldName that, if omitted, will be treated
 * as checking for global errors.
 */
export function hasErrorMessage<T extends string = string>(
  error: AxiosError<ServerValidationErrorResponse<T>>,
  errorMessages: readonly T[],
  fieldName = '',
): boolean {
  if (!errorMessages.length) {
    return true;
  }

  return errorMessages.some(errorMessage =>
    error.response?.data.errors[fieldName].includes(errorMessage),
  );
}

/**
 * Retrieves the first error message for the given {@param fieldName}, or the
 * first global error message if {@param fieldName} is omitted.
 *
 * @param error - the error containing messages.
 * @param fieldName - optional fieldName that, if omitted, will be treated as
 * checking for global errors.
 */
export function getErrorMessage<T extends string = string>(
  error: AxiosError<ServerValidationErrorResponse<T>>,
  fieldName = '',
): T | undefined {
  return error.response?.data?.errors[fieldName]?.[0];
}

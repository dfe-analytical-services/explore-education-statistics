import { Dictionary } from '@common/types';
import isAxiosError from '@common/utils/error/isAxiosError';
import { AxiosError } from 'axios';
import { FormikErrors } from 'formik';
import camelCase from 'lodash/camelCase';
import set from 'lodash/set';
import toPath from 'lodash/toPath';

export interface ServerValidationErrorResponse {
  errors: Dictionary<string[]>;
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
  messageMappers: FieldMessageMapper<FormValues>[] = [],
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
        }
      });

      return acc;
    },
    {},
  );
}

export function isServerValidationError(
  error: unknown,
  errorMessage?: string,
): error is AxiosError<ServerValidationErrorResponse> {
  if (!isAxiosError(error) || !error.response?.data) {
    return false;
  }

  const errorDataAsValidationError = error.response
    .data as ServerValidationErrorResponse;

  const isServerError =
    errorDataAsValidationError.errors !== undefined &&
    errorDataAsValidationError.status !== undefined &&
    errorDataAsValidationError.title !== undefined;

  if (!errorMessage) {
    return isServerError;
  }

  if (isServerError && error.response?.data) {
    const errors = Object.values(error.response?.data.errors);
    if (errors.flat().includes(errorMessage)) {
      return true;
    }
  }
  return false;
}

export function hasServerValidationError(
  error: Error,
  ...errorCodes: string[]
): boolean {
  if (isServerValidationError(error) && error.response?.data) {
    const existingCodes = Object.values(error.response?.data.errors).flat();
    return existingCodes.every(existingCode =>
      errorCodes.includes(existingCode),
    );
  }
  return false;
}

export function getServerValidationError(error: Error): string | null {
  if (!isServerValidationError(error) || !error.response?.data) {
    return null;
  }

  const errors = Object.values(error.response?.data.errors);
  return errors.flat()[0];
}

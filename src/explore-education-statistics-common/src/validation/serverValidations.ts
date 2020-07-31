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

export type FieldName<FormValues> = FormValues extends {}
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
  messages: Dictionary<string>;
}): FieldMessageMapper<FormValues> {
  const { target, source, messages } = options;

  return error => {
    if (!messages[error.message]) {
      return undefined;
    }

    if (source) {
      if (source !== error.sourceField) {
        return undefined;
      }
    }
    // Source field may be an empty string in many cases.
    // This should probably not be happening as validation
    // errors should usually be associated with a field.
    else if (error.sourceField && error.sourceField !== target) {
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
  error: Error,
): error is AxiosError<ServerValidationErrorResponse> {
  if (!isAxiosError(error) || !error.response?.data) {
    return false;
  }

  const errorDataAsValidationError = error.response
    .data as ServerValidationErrorResponse;

  return (
    errorDataAsValidationError.errors !== undefined &&
    errorDataAsValidationError.status !== undefined &&
    errorDataAsValidationError.title !== undefined
  );
}

import isObject from 'lodash/isObject';

/**
 * Check if a {@param value} is an {@see Error} or
 * has a data structure like an {@see Error}.
 */
export default function isErrorLike(value: unknown): value is Error {
  if (!isObject(value)) {
    return false;
  }

  if (value instanceof Error) {
    return true;
  }

  const typedError = value as Error;

  return Boolean(typedError.name && typedError.message);
}

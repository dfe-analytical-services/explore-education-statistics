import {
  ValidationProblemDetails,
  ValidationProblemError,
} from '@common/services/types/problemDetails';
import { Dictionary } from '@common/types';
import { AxiosError, AxiosHeaders } from 'axios';

interface CreateAxiosErrorOptions<T> {
  name?: string;
  data: T;
  message?: string;
  headers?: Dictionary<string>;
  status?: number;
  statusText?: string;
}

export default function createAxiosErrorMock<T>(
  options: CreateAxiosErrorOptions<T>,
): AxiosError<T> {
  const status = options.status ?? 500;

  return {
    name: options.name ?? 'AxiosError',
    request: {},
    config: {
      headers: new AxiosHeaders(),
    },
    isAxiosError: true,
    message: options.message ?? `Request failed with status code ${status}`,
    response: {
      config: {
        headers: new AxiosHeaders(),
      },
      headers: options.headers ?? {},
      data: options.data,
      status,
      statusText: options.statusText ?? '',
    },
    toJSON: jest.fn(),
  };
}

export function createServerValidationErrorMock<TCode extends string = string>(
  errors: ValidationProblemError<TCode>[],
): AxiosError<ValidationProblemDetails<TCode>> {
  return createAxiosErrorMock({
    status: 400,
    data: {
      errors,
      status: 400,
      title: 'One or more validation errors occurred.',
      type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
    },
  });
}

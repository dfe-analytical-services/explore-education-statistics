import { Dictionary } from '@common/types';
import { ServerValidationErrorResponse } from '@common/validation/serverValidations';
import { AxiosError } from 'axios';

interface CreateAxiosErrorOptions<T> {
  name?: string;
  data: T;
  message?: string;
  headers?: Dictionary<string>;
  status?: number;
  statusText?: string;
}

// AxiosError with `config` and `config.headers` omitted from `config` and `response`
// as these properties aren't needed in tests
export interface IAxiosError<T = unknown>
  extends Omit<AxiosError<T>, 'config' | 'response'> {
  config: Omit<AxiosError<T>['config'], 'headers'>;
  response: Omit<AxiosError<T>['response'], 'config' | 'headers'>;
}

export default function createAxiosErrorMock<T>(
  options: CreateAxiosErrorOptions<T>,
): IAxiosError<T> {
  const status = options.status ?? 500;

  return {
    name: options.name ?? 'AxiosError',
    request: {},
    config: {
      headers: {},
    },
    isAxiosError: true,
    message: options.message ?? `Request failed with status code ${status}`,
    response: {
      headers: options.headers ?? {},
      data: options.data,
      status,
      statusText: options.statusText ?? '',
    },
    toJSON: jest.fn(),
  };
}

export function createServerValidationErrorMock<T extends string = string>(
  globalErrors: T[],
  fieldErrors: Dictionary<T[]> = {},
): IAxiosError<ServerValidationErrorResponse<T>> {
  return createAxiosErrorMock({
    status: 400,
    data: {
      errors: {
        ...fieldErrors,
        '': globalErrors,
      },
      status: 400,
      title: 'One or more validation errors occurred.',
    },
  });
}

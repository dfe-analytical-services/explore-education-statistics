import isObject from 'lodash/isObject';

export interface HubResult<T> {
  type: 'HubResult';
  status: number;
  message?: string;
  data?: T;
}

export default function isHubResult<T>(value: unknown): value is HubResult<T> {
  if (!isObject(value)) {
    return false;
  }

  const result = value as HubResult<T>;

  return typeof result.status === 'number' && result.type === 'HubResult';
}

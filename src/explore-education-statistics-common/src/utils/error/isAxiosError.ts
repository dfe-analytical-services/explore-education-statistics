import isErrorLike from '@common/utils/error/isErrorLike';
import { AxiosError } from 'axios';

export default function isAxiosError(error: unknown): error is AxiosError {
  if (!isErrorLike(error)) {
    return false;
  }

  const axiosError = error as AxiosError;

  return Boolean(axiosError.isAxiosError);
}

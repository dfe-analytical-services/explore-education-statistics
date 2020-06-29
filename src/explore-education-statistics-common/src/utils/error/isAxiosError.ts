import { AxiosError } from 'axios';

export default function isAxiosError(error: Error): error is AxiosError {
  const axiosError = error as AxiosError;

  return Boolean(axiosError?.isAxiosError);
}

import { FormikErrors, FormikTouched, FormikValues } from 'formik';
import get from 'lodash/get';

const createErrorHelper = <T extends FormikValues>({
  touched,
  errors,
}: {
  touched: FormikTouched<T>;
  errors: FormikErrors<T>;
}) => {
  const getAllErrors = (
    errorGroup: FormikValues,
    keyPrefix?: string,
  ): { [key: string]: string } => {
    return Object.entries(errorGroup).reduce((acc, [key, error]) => {
      const errorKey = keyPrefix ? `${keyPrefix}.${key}` : key;

      const isTouched = get(touched, errorKey, false);

      if (!isTouched || typeof error === 'undefined') {
        return acc;
      }

      if (typeof error === 'string') {
        return {
          ...acc,
          [errorKey]: error,
        };
      }

      return {
        ...acc,
        ...getAllErrors(error, errorKey),
      };
    }, {});
  };

  const getError = (name: keyof T | string): string => {
    const isTouched = get(touched, name, false);

    if (!isTouched) {
      return '';
    }

    const error = get(errors, name);

    return typeof error === 'string' ? error : '';
  };

  const hasError = (value: keyof T | string): boolean => {
    return !!getError(value);
  };

  return {
    getError,
    hasError,
    getAllErrors: () => getAllErrors(errors),
  };
};

export default createErrorHelper;

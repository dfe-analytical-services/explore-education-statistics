import { FormikErrors, FormikTouched, FormikValues } from 'formik';
import get from 'lodash/get';

function createErrorHelper<T extends FormikValues>({
  touched,
  errors,
}: {
  touched: FormikTouched<T>;
  errors: FormikErrors<T>;
}) {
  const getError = (name: keyof T): string => {
    const isTouched = get(touched, name, false);

    if (!isTouched) {
      return '';
    }

    const error = get(errors, name);

    return typeof error === 'string' ? error : '';
  };

  const hasError = (value: keyof T): boolean => {
    return !!getError(value);
  };

  return {
    getError,
    hasError,
  };
}

export default createErrorHelper;

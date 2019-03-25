import { FormikErrors, FormikTouched, FormikValues } from 'formik';

function createErrorHelper<T extends FormikValues>({
  touched,
  errors,
}: {
  touched: FormikTouched<T>;
  errors: FormikErrors<T>;
}) {
  const getError = (name: keyof T): string => {
    if (!touched[name]) {
      return '';
    }

    return typeof errors[name] === 'string' ? (errors[name] as string) : '';
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

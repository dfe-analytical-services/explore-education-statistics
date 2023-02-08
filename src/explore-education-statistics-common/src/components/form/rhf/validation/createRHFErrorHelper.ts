import get from 'lodash/get';
import {
  FieldErrors,
  FieldNamesMarkedBoolean,
  FieldValues,
} from 'react-hook-form';

const createRHFErrorHelper = <T extends FieldValues>({
  touchedFields,
  errors,
  isSubmitted = false,
}: {
  touchedFields: Partial<Readonly<FieldNamesMarkedBoolean<T>>>;
  errors: FieldErrors<T>;
  isSubmitted?: boolean;
}) => {
  const getAllErrors = (
    errorGroup: FieldValues,
    keyPrefix?: string,
  ): { [key: string]: string } => {
    return Object.entries(errorGroup).reduce((acc, [key, error]) => {
      const errorKey = keyPrefix ? `${keyPrefix}.${key}` : key;

      const isTouched = get(touchedFields, errorKey, false);

      // If the form isn't submitted, only show errors for touched fields.
      // Formik touches all fields on submit, but RHF does not so we
      // need to take whether the form has been submitted into account.
      if ((!isSubmitted && !isTouched) || typeof error === 'undefined') {
        return acc;
      }

      if (error.message) {
        return {
          ...acc,
          [errorKey]: error.message,
        };
      }

      return {
        ...acc,
        ...getAllErrors(error, errorKey),
      };
    }, {});
  };

  const getError = (name: keyof T | string): string => {
    const isTouched = get(touchedFields, name, false);

    if (!isSubmitted && !isTouched) {
      return '';
    }

    const error = get(errors, name);

    return error?.message ? (error?.message as string) : '';
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

export default createRHFErrorHelper;

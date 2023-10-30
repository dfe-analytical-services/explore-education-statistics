import { Dictionary } from '@common/types';
import get from 'lodash/get';
import {
  FieldError,
  FieldErrors,
  FieldNamesMarkedBoolean,
  FieldValues,
} from 'react-hook-form';

export default function createRHFErrorHelper<TFormValues extends FieldValues>({
  touchedFields,
  errors,
  isSubmitted = false,
}: {
  touchedFields: Partial<Readonly<FieldNamesMarkedBoolean<TFormValues>>>;
  errors: FieldErrors<TFormValues>;
  isSubmitted?: boolean;
}) {
  const getAllErrors = (
    errorGroup: FieldErrors<TFormValues> | FieldError,
    keyPrefix?: string,
  ): Dictionary<string> => {
    return Object.entries(errorGroup).reduce((acc, [key, error]) => {
      const errorKey = keyPrefix ? `${keyPrefix}.${key}` : key;

      const isTouched = get(touchedFields, errorKey, false);

      // If the form isn't submitted, only show errors for touched fields.
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

  const getError = (name: keyof TFormValues | string): string => {
    const isTouched = get(touchedFields, name, false);

    if (!isSubmitted && !isTouched) {
      return '';
    }

    const error = get(errors, name);

    return error?.message ? (error?.message as string) : '';
  };

  const hasError = (value: keyof TFormValues | string): boolean => {
    return !!getError(value);
  };

  return {
    getError,
    hasError,
    getAllErrors: () => getAllErrors(errors),
  };
}

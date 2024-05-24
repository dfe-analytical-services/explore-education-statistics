import {
  FieldValues,
  FieldPath,
  UseFormRegister,
  UseFormRegisterReturn,
} from 'react-hook-form';
import { useMemo } from 'react';
import parseNumber from '@common/utils/number/parseNumber';

/**
 * Memoize the RHF register call.
 * If we don't do this, register will return a new object and cause excessive rendering due to dependency changes.
 */
export default function useRegister<
  TFieldValues extends FieldValues,
  TFieldName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
>(
  name: TFieldName,
  register: UseFormRegister<TFieldValues>,
  isNumberField = false,
): UseFormRegisterReturn<TFieldName> {
  // RHF changes number input values to strings, this converts them back to numbers.
  const options = isNumberField
    ? // eslint-disable-next-line @typescript-eslint/no-explicit-any
      { setValueAs: (value: any) => parseNumber(value) }
    : undefined;

  // eslint-disable-next-line react-hooks/exhaustive-deps
  return useMemo(() => register(name, options), [name, options]);
}

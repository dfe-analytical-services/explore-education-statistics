import {
  FieldValues,
  FieldPath,
  UseFormRegister,
  UseFormRegisterReturn,
} from 'react-hook-form';
import { useMemo } from 'react';

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
): UseFormRegisterReturn<TFieldName> {
  // eslint-disable-next-line react-hooks/exhaustive-deps
  return useMemo(() => register(name), [name]);
}

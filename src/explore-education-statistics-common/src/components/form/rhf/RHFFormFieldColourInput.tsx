import RHFFormColourInput, {
  RHFFormColourInputProps,
} from '@common/components/form/rhf/RHFFormColourInput';
import RHFFormField, {
  FormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';

import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  RHFFormColourInputProps,
  TFormValues
>;

export default function RHFFormFieldColourInput<
  TFormValues extends FieldValues,
>(props: Props<TFormValues>) {
  return <RHFFormField {...props} as={RHFFormColourInput} />;
}

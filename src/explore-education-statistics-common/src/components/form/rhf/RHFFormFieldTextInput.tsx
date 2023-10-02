import RHFFormField, {
  RHFFormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import FormTextInput, {
  FormTextInputProps,
} from '@common/components/form/FormTextInput';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = RHFFormFieldComponentProps<
  FormTextInputProps,
  TFormValues
>;

export default function RHFFormFieldTextInput<TFormValues extends FieldValues>({
  name,
  ...props
}: Props<TFormValues>) {
  return <RHFFormField {...props} name={name} as={FormTextInput} />;
}

import RHFFormField, {
  RHFFormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import FormTextArea, {
  FormTextAreaProps,
} from '@common/components/form/FormTextArea';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = RHFFormFieldComponentProps<
  FormTextAreaProps,
  TFormValues
>;

export default function RHFFormFieldTextArea<TFormValues extends FieldValues>({
  name,
  ...props
}: Props<TFormValues>) {
  return <RHFFormField {...props} name={name} as={FormTextArea} />;
}

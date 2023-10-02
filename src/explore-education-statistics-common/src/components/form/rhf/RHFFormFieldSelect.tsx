import RHFFormField, {
  RHFFormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import FormSelect, {
  FormSelectProps,
} from '@common/components/form/FormSelect';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = RHFFormFieldComponentProps<
  FormSelectProps,
  TFormValues
>;

export default function RHFFormFieldSelect<TFormValues extends FieldValues>({
  name,
  ...props
}: Props<TFormValues>) {
  return <RHFFormField {...props} name={name} as={FormSelect} />;
}

RHFFormFieldSelect.unordered = [] as [];

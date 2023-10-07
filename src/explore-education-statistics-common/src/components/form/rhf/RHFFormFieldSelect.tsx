import RHFFormField, {
  FormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import FormSelect, {
  FormSelectProps,
} from '@common/components/form/FormSelect';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormSelectProps,
  TFormValues
>;

export default function RHFFormFieldSelect<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  return <RHFFormField {...props} as={FormSelect} />;
}

RHFFormFieldSelect.unordered = [] as [];

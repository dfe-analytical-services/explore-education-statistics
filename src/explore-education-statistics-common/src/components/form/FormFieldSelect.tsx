import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormSelect, {
  FormSelectProps,
} from '@common/components/form/FormSelect';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormSelectProps,
  TFormValues
>;

export default function FormFieldSelect<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  return <FormField {...props} as={FormSelect} />;
}

FormFieldSelect.unordered = [] as [];

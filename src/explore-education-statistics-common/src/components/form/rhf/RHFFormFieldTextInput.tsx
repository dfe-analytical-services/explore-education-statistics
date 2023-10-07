import RHFFormField, {
  FormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import FormTextInput, {
  FormTextInputProps,
} from '@common/components/form/FormTextInput';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormTextInputProps,
  TFormValues
>;

export default function RHFFormFieldTextInput<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  return <RHFFormField {...props} as={FormTextInput} />;
}

import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormTextInput, {
  FormTextInputProps,
} from '@common/components/form/FormTextInput';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormTextInputProps,
  TFormValues
>;

export default function FormFieldTextInput<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  return <FormField {...props} as={FormTextInput} />;
}

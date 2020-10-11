import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import React from 'react';
import FormTextInput, { FormTextInputProps } from './FormTextInput';

type Props<FormValues> = FormFieldComponentProps<
  FormTextInputProps,
  FormValues
>;

function FormFieldTextInput<FormValues>(props: Props<FormValues>) {
  return <FormField {...props} as={FormTextInput} />;
}

export default FormFieldTextInput;

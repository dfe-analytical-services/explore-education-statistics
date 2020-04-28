import FormField from '@common/components/form/FormField';
import React from 'react';
import FormTextInput, { FormTextInputProps } from './FormTextInput';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
  formGroupClass?: string;
} & FormTextInputProps;

const FormFieldTextInput = <T extends {}>(props: Props<T>) => {
  return <FormField {...props} as={FormTextInput} />;
};

export default FormFieldTextInput;

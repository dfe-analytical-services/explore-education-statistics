import FormColourInput, {
  FormColourInputProps,
} from '@common/components/form/FormColourInput';
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import React from 'react';

type Props<FormValues> = FormFieldComponentProps<
  FormColourInputProps,
  FormValues
>;

function FormFieldColourInput<FormValues>(props: Props<FormValues>) {
  return <FormField {...props} as={FormColourInput} />;
}

export default FormFieldColourInput;

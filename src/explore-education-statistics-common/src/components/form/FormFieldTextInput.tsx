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
  const { trimInput } = props;
  return (
    <FormField
      {...props}
      as={FormTextInput}
      trimInput={trimInput ?? true}
      trimInputOnEnter={trimInput ?? true}
    />
  );
}

export default FormFieldTextInput;

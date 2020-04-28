import FormBaseInput, {
  FormBaseInputProps,
} from '@common/components/form/FormBaseInput';
import React from 'react';

export interface FormTextInputProps extends FormBaseInputProps {
  defaultValue?: string;
  pattern?: string;
  value?: string;
}

const FormTextInput = (props: FormTextInputProps) => {
  return <FormBaseInput {...props} />;
};

export default FormTextInput;

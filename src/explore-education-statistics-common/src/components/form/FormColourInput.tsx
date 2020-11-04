import BaseFormInput, {
  FormBaseInputProps,
} from '@common/components/form/FormBaseInput';
import React from 'react';

export interface FormColourInputProps extends FormBaseInputProps {
  list: string;
  value?: string;
}

const FormColourInput = (props: FormColourInputProps) => {
  return <BaseFormInput {...props} type="color" />;
};

export default FormColourInput;

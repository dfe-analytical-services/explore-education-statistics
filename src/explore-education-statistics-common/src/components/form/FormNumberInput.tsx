import BaseFormInput, {
  FormBaseInputProps,
} from '@common/components/form/FormBaseInput';
import React from 'react';

export interface FormNumberInputProps extends FormBaseInputProps {
  defaultValue?: number;
  min?: number;
  max?: number;
  value?: number | null;
}

const FormNumberInput = ({ value, ...props }: FormNumberInputProps) => {
  return (
    <BaseFormInput
      {...props}
      type="number"
      value={!value && value !== 0 ? '' : value}
    />
  );
};

export default FormNumberInput;

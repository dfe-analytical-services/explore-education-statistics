import FormBaseInput, {
  FormBaseInputProps,
} from '@common/components/form/FormBaseInput';
import React from 'react';

export interface FormTextInputProps extends FormBaseInputProps {
  defaultValue?: string;
  pattern?: string;
  value?: string;
}

export default function FormTextInput({
  id,
  value,
  ...props
}: FormTextInputProps) {
  return <FormBaseInput id={id} {...props} value={value} />;
}

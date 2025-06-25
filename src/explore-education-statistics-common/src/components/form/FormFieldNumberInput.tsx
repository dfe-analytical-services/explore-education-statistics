import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormBaseInput, {
  FormBaseInputProps,
} from '@common/components/form/FormBaseInput';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormNumberInputProps,
  TFormValues
>;

export default function FormFieldNumberInput<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  return <FormField {...props} as={FormNumberInput} isNumberField />;
}

interface FormNumberInputProps extends FormBaseInputProps {
  defaultValue?: number;
  min?: number;
  max?: number;
}

function FormNumberInput(props: FormNumberInputProps) {
  return <FormBaseInput {...props} type="text" inputMode="numeric" />;
}

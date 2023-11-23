import RHFFormField, {
  FormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import FormBaseInput, {
  FormBaseInputProps,
} from '@common/components/form/FormBaseInput';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  RHFFormNumberInputProps,
  TFormValues
>;

export default function RHFFormFieldNumberInput<
  TFormValues extends FieldValues,
>(props: Props<TFormValues>) {
  return <RHFFormField {...props} as={RHFFormNumberInput} />;
}

interface RHFFormNumberInputProps extends FormBaseInputProps {
  defaultValue?: number;
  min?: number;
  max?: number;
}

function RHFFormNumberInput(props: RHFFormNumberInputProps) {
  return <FormBaseInput {...props} type="number" />;
}

import FormColourInput, {
  FormColourInputProps,
} from '@common/components/form/FormColourInput';
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';

import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormColourInputProps,
  TFormValues
>;

export default function FormFieldColourInput<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  return <FormField {...props} as={FormColourInput} />;
}

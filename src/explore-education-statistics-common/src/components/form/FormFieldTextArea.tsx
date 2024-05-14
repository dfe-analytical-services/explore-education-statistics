import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormTextArea from '@common/components/form/FormTextArea';
import { FormTextAreaProps } from '@common/components/form/FormBaseTextArea';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormTextAreaProps,
  TFormValues
>;

export default function FormFieldTextArea<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  return <FormField {...props} as={FormTextArea} />;
}

import RHFFormField, {
  FormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import FormTextArea, {
  FormTextAreaProps,
} from '@common/components/form/FormTextArea';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormTextAreaProps,
  TFormValues
>;

export default function RHFFormFieldTextArea<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  return <RHFFormField {...props} as={FormTextArea} />;
}

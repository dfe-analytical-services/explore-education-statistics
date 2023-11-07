import RHFFormField, {
  FormFieldComponentProps,
} from '@common/components/form/rhf/RHFFormField';
import RHFFormTextArea from '@common/components/form/rhf/RHFFormTextArea';
import { FormTextAreaProps } from '@common/components/form/FormBaseTextArea';
import React from 'react';
import { FieldValues } from 'react-hook-form';

type Props<TFormValues extends FieldValues> = FormFieldComponentProps<
  FormTextAreaProps,
  TFormValues
>;

export default function RHFFormFieldTextArea<TFormValues extends FieldValues>(
  props: Props<TFormValues>,
) {
  const { trimInput } = props;
  return (
    <RHFFormField
      {...props}
      as={RHFFormTextArea}
      trimInput={trimInput ?? true}
    />
  );
}

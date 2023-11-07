import FormTextArea from '@common/components/form/FormTextArea';
import { FormTextAreaProps } from '@common/components/form/FormBaseTextArea';
import React from 'react';
import FormField, { FormFieldComponentProps } from './FormField';

type Props<FormValues> = FormFieldComponentProps<FormTextAreaProps, FormValues>;

function FormFieldTextArea<FormValues>(props: Props<FormValues>) {
  return <FormField {...props} as={FormTextArea} />;
}

export default FormFieldTextArea;

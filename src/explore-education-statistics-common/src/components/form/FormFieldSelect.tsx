import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import React from 'react';
import FormSelect, { FormSelectProps } from './FormSelect';

type Props<FormValues> = FormFieldComponentProps<FormSelectProps>;

const FormFieldSelect = <FormValues extends {}>(props: Props<FormValues>) => {
  return <FormField {...props} as={FormSelect} />;
};

export default FormFieldSelect;

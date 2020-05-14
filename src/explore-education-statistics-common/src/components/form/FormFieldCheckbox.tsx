import { FormCheckboxProps } from '@common/components/form/FormCheckbox';
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import { FormCheckbox } from '@common/components/form/index';
import React from 'react';

type Props<FormValues> = FormFieldComponentProps<FormCheckboxProps>;

const FormFieldCheckbox = <FormValues extends {}>(props: Props<FormValues>) => {
  return <FormField {...props} as={FormCheckbox} type="checkbox" />;
};

export default FormFieldCheckbox;

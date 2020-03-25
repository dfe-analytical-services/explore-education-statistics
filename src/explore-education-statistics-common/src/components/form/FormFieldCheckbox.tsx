import { FormCheckboxProps } from '@common/components/form/FormCheckbox';
import FormField from '@common/components/form/FormField';
import { FormCheckbox } from '@common/components/form/index';
import { OmitStrict } from '@common/types';
import React from 'react';

type Props<FormValues> = {
  name: keyof FormValues | string;
} & OmitStrict<FormCheckboxProps, 'name' | 'value'>;

const FormFieldCheckbox = <FormValues extends {}>(props: Props<FormValues>) => {
  return <FormField {...props} as={FormCheckbox} type="checkbox" />;
};

export default FormFieldCheckbox;

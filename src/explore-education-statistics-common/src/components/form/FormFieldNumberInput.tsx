import FormField from '@common/components/form/FormField';
import FormNumberInput, {
  FormNumberInputProps,
} from '@common/components/form/FormNumberInput';
import React from 'react';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
  formGroupClass?: string;
} & FormNumberInputProps;

const FormFieldNumberInput = <T extends {}>(props: Props<T>) => {
  return <FormField {...props} as={FormNumberInput} type="number" />;
};

export default FormFieldNumberInput;

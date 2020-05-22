import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import FormNumberInput, {
  FormNumberInputProps,
} from '@common/components/form/FormNumberInput';
import React from 'react';

type Props<FormValues> = FormFieldComponentProps<
  FormNumberInputProps,
  FormValues
>;

const FormFieldNumberInput = <FormValues extends {}>(
  props: Props<FormValues>,
) => {
  return <FormField {...props} as={FormNumberInput} type="number" />;
};

export default FormFieldNumberInput;

import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import React from 'react';
import FormRadioGroup, { FormRadioGroupProps } from './FormRadioGroup';

type Props<FormValues, Value extends string> = FormFieldComponentProps<
  FormRadioGroupProps<Value>,
  FormValues
>;

const FormFieldRadioGroup = <
  FormValues extends {},
  Value extends string = string
>(
  props: Props<FormValues, Value>,
) => {
  return (
    <FormField<string> {...props}>
      {({ field }) => (
        <FormRadioGroup
          {...props}
          {...field}
          onChange={(event, option) => {
            if (props.onChange) {
              props.onChange(event, option);
            }

            if (!event.isDefaultPrevented()) {
              field.onChange(event);
            }
          }}
        />
      )}
    </FormField>
  );
};

export default FormFieldRadioGroup;

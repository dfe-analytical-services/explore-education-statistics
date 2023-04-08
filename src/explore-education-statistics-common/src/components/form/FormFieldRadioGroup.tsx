/* eslint-disable react/destructuring-assignment */
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import React from 'react';
import FormRadioGroup, { FormRadioGroupProps } from './FormRadioGroup';

type Props<FormValues, Value extends string> = FormFieldComponentProps<
  FormRadioGroupProps<Value>,
  FormValues
>;

function FormFieldRadioGroup<FormValues, Value extends string = string>(
  props: Props<FormValues, Value>,
) {
  const { onChange } = props;

  return (
    <FormField<Value> {...props}>
      {({ id, field }) => (
        <FormRadioGroup<Value>
          {...props}
          {...field}
          id={id}
          onChange={(event, option) => {
            onChange?.(event, option);

            if (!event.isDefaultPrevented()) {
              field.onChange(event);
            }
          }}
        />
      )}
    </FormField>
  );
}

export default FormFieldRadioGroup;

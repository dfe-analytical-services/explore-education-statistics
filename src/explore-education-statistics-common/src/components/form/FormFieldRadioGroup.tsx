import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import React from 'react';
import FormRadioGroup, { FormRadioGroupProps } from './FormRadioGroup';

type Props<FormValues> = FormFieldComponentProps<FormRadioGroupProps>;

const FormFieldRadioGroup = <FormValues extends {}>(
  props: Props<FormValues>,
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

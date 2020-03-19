import createErrorHelper from '@common/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React from 'react';
import FormGroup from './FormGroup';
import FormSelect, { FormSelectProps } from './FormSelect';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & FormSelectProps;

const FormFieldSelect = <T extends {}>(props: Props<T>) => {
  const { error, name, showError = true } = props;

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        const { getError } = createErrorHelper(form);

        let errorMessage = error || getError(name);

        if (!showError) {
          errorMessage = '';
        }

        return (
          <FormGroup hasError={!!errorMessage}>
            <FormSelect
              {...props}
              {...field}
              error={errorMessage}
              onChange={event => {
                if (props.onChange) {
                  props.onChange(event);
                }

                if (!event.isDefaultPrevented()) {
                  field.onChange(event);
                }
              }}
            />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormFieldSelect;

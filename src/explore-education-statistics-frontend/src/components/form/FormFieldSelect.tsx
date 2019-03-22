import { Field, FieldProps } from 'formik';
import React from 'react';
import createErrorHelper from 'src/lib/validation/createErrorHelper';
import FormGroup from './FormGroup';
import FormSelect, { FormSelectProps } from './FormSelect';

type Props<FormValues> = {
  name: keyof FormValues | string;
} & FormSelectProps;

const FormFieldSelect = <T extends {}>(props: Props<T>) => {
  const { error, name } = props;

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        const { getError, hasError } = createErrorHelper(form);

        return (
          <FormGroup hasError={error ? !!error : hasError(name)}>
            <FormSelect
              {...props}
              {...field}
              error={error ? error : getError(name)}
            />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormFieldSelect;

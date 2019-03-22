import { Field, FieldProps } from 'formik';
import React from 'react';
import createErrorHelper from 'src/lib/validation/createErrorHelper';
import { Omit } from 'src/types/util';
import FormGroup from './FormGroup';
import FormSelect, { FormSelectProps } from './FormSelect';

type Props = Omit<FormSelectProps, 'error'>;

const FormFieldSelect = (props: Props) => {
  const { name } = props;

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        const { getError, hasError } = createErrorHelper(form);

        return (
          <FormGroup hasError={hasError(name)}>
            <FormSelect {...props} {...field} error={getError(name)} />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormFieldSelect;

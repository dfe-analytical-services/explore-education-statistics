import { Field, FieldProps } from 'formik';
import React from 'react';
import createErrorHelper from '../../lib/validation/createErrorHelper';
import { Omit } from '../../types/util';
import FormRadioGroup, { FormRadioGroupProps } from './FormRadioGroup';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & Omit<FormRadioGroupProps, 'value'>;

const FormFieldRadioGroup = <T extends {}>(props: Props<T>) => {
  const { error, name, showError = true } = props;

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        const { getError } = createErrorHelper(form);

        let errorMessage = error ? error : getError(name);

        if (!showError) {
          errorMessage = '';
        }

        return <FormRadioGroup {...props} {...field} error={errorMessage} />;
      }}
    </Field>
  );
};

export default FormFieldRadioGroup;

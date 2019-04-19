import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { Omit } from '@common/types/util';
import { Field, FieldProps } from 'formik';
import React from 'react';
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

        let errorMessage = error || getError(name);

        if (!showError) {
          errorMessage = '';
        }

        return <FormRadioGroup {...props} {...field} error={errorMessage} />;
      }}
    </Field>
  );
};

export default FormFieldRadioGroup;

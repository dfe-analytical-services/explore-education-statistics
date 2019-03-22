import { Field, FieldProps } from 'formik';
import React from 'react';
import createErrorHelper from 'src/lib/validation/createErrorHelper';
import FormRadioGroup, { FormRadioGroupProps } from './FormRadioGroup';

type Props<FormValues> = {
  name: keyof FormValues | string;
} & FormRadioGroupProps;

const FormFieldRadioGroup = <T extends {}>(props: Props<T>) => {
  const { error } = props;

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        const { getError } = createErrorHelper(form);

        return (
          <FormRadioGroup
            {...props}
            {...field}
            error={error ? error : getError(name)}
          />
        );
      }}
    </Field>
  );
};

export default FormFieldRadioGroup;

import { Field, FieldProps } from 'formik';
import React, { Component } from 'react';
import createErrorHelper from 'src/lib/validation/createErrorHelper';
import { Omit } from 'src/types/util';
import FormRadioGroup, { FormRadioGroupProps } from './FormRadioGroup';

type Props<FormValues> = {
  name: keyof FormValues | string;
} & Omit<FormRadioGroupProps, 'error'>;

class FormFieldRadioGroup<FormValues> extends Component<Props<FormValues>> {
  public render() {
    return (
      <Field name={name}>
        {({ field, form }: FieldProps) => {
          const { getError } = createErrorHelper(form);

          return (
            <FormRadioGroup {...this.props} {...field} error={getError(name)} />
          );
        }}
      </Field>
    );
  }
}

export default FormFieldRadioGroup;

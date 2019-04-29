import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React from 'react';
import FormGroup from './FormGroup';
import FormTextInput, { FormTextInputProps } from './FormTextInput';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & FormTextInputProps;

const FormFieldTextInput = <T extends {}>(props: Props<T>) => {
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
            <FormTextInput {...props} {...field} error={errorMessage} />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormFieldTextInput;

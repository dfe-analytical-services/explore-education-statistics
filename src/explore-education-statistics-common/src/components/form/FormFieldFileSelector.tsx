import createErrorHelper from '@common/lib/validation/createErrorHelper';
import classNames from 'classnames';
import { FormikProps } from 'formik';
import React, { ChangeEvent, ReactNode } from 'react';
import FormGroup from './FormGroup';
import FormTextInput from './FormTextInput';

interface Props<T> {
  id: string;
  name: keyof T;
  label: string;
  error?: ReactNode | string;
  formGroupClass?: string;
  form: FormikProps<T>;
}

const FormFieldFileSelector = <T extends {}>({
  name,
  form,
  error,
  formGroupClass,
  ...childProps
}: Props<T>) => {
  const { getError } = createErrorHelper(form);

  const errorMessage = error || getError(name);

  return (
    <FormGroup
      hasError={!!errorMessage}
      className={classNames({
        [formGroupClass || '']: formGroupClass,
      })}
    >
      <FormTextInput
        type="file"
        name={name.toString()}
        onChange={(event: ChangeEvent<HTMLInputElement>) => {
          const file =
            event.target.files && event.target.files.length > 0
              ? event.target.files[0]
              : null;
          form.setFieldValue(name.toString(), file);
        }}
        {...childProps}
        error={errorMessage}
      />
    </FormGroup>
  );
};

export default FormFieldFileSelector;

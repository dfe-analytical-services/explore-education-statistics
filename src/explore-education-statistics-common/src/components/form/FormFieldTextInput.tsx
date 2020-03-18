import createErrorHelper from '@common/validation/createErrorHelper';
import classNames from 'classnames';
import { Field, FieldProps } from 'formik';
import React from 'react';
import FormGroup from './FormGroup';
import FormTextInput, { FormTextInputProps } from './FormTextInput';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
  formGroupClass?: string;
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

        const { formGroupClass, ...childProps } = props;

        return (
          <FormGroup
            hasError={!!errorMessage}
            className={classNames({
              [formGroupClass || '']: formGroupClass,
            })}
          >
            <FormTextInput
              {...childProps}
              {...field}
              error={errorMessage}
              onChange={e => {
                if (props.onChange) {
                  props.onChange(e);
                }

                if (!e.isDefaultPrevented()) {
                  field.onChange(e);
                }
              }}
            />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormFieldTextInput;

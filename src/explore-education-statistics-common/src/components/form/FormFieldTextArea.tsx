import FormTextArea, {FormTextAreaProps} from "@common/components/form/FormTextArea";
import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React from 'react';
import classNames from 'classnames';
import FormGroup from './FormGroup';
import FormTextInput, { FormTextInputProps } from './FormTextInput';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
  formGroupClass?: string;
} & FormTextAreaProps;

const FormFieldTextArea = <T extends {}>(props: Props<T>) => {
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
            <FormTextArea {...childProps} {...field} error={errorMessage} />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormFieldTextArea;

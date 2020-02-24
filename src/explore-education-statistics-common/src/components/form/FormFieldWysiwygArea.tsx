import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React from 'react';
import classNames from 'classnames';
import FormWysiwygArea, {
  FormWysiwygAreaProps,
} from '@common/components/form/FormWysiwygArea';
import FormGroup from './FormGroup';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
  formGroupClass?: string;
} & FormWysiwygAreaProps;

const FormFieldWysiwygArea = <T extends {}>(props: Props<T>) => {
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
            <FormWysiwygArea {...childProps} {...field} error={errorMessage} />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormFieldWysiwygArea;

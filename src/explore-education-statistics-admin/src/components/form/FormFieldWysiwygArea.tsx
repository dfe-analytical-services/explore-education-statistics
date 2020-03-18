import FormWysiwygArea, {
  FormWysiwygAreaProps,
} from '@admin/components/form/FormWysiwygArea';
import FormGroup from '@common/components/form/FormGroup';
import createErrorHelper from '@common/validation/createErrorHelper';
import classNames from 'classnames';
import { Field, FieldProps } from 'formik';
import React from 'react';

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

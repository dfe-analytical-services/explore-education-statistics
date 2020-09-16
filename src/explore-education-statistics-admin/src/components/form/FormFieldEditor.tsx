import FormEditor, { FormEditorProps } from '@admin/components/form/FormEditor';
import FormGroup from '@common/components/form/FormGroup';
import { OmitStrict } from '@common/types';
import createErrorHelper from '@common/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React from 'react';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
  formGroupClass?: string;
} & OmitStrict<FormEditorProps, 'value' | 'onChange'>;

const FormFieldEditor = <T extends {}>({
  error,
  name,
  showError = true,
  formGroupClass,
  ...props
}: Props<T>) => {
  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        const { getError } = createErrorHelper(form);

        let errorMessage = error || getError(name);

        if (!showError) {
          errorMessage = '';
        }

        return (
          <FormGroup hasError={!!errorMessage} className={formGroupClass}>
            <FormEditor
              {...props}
              {...field}
              onBlur={() => {
                form.setFieldTouched(name as string, true);
              }}
              onChange={value => {
                form.setFieldValue(name as string, value);
              }}
              error={errorMessage}
            />
          </FormGroup>
        );
      }}
    </Field>
  );
};

export default FormFieldEditor;

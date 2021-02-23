import FormEditor, { FormEditorProps } from '@admin/components/form/FormEditor';
import { useFormContext } from '@common/components/form/contexts/FormContext';
import FormGroup from '@common/components/form/FormGroup';
import { OmitStrict } from '@common/types';
import createErrorHelper from '@common/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React from 'react';

type Props<FormValues> = {
  id?: string;
  name: keyof FormValues | string;
  showError?: boolean;
  formGroupClass?: string;
  testId?: string;
} & OmitStrict<FormEditorProps, 'id' | 'value' | 'onChange'>;

function FormFieldEditor<T>({
  error,
  id,
  name,
  showError = true,
  formGroupClass,
  testId,
  ...props
}: Props<T>) {
  const { prefixFormId, fieldId } = useFormContext();

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
              testId={testId}
              {...props}
              {...field}
              id={id ? prefixFormId(id) : fieldId(name as string)}
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
}

export default FormFieldEditor;

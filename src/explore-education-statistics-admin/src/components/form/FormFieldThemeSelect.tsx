import FormThemeSelect, {
  FormThemeSelectProps,
} from '@admin/components/form/FormThemeSelect';
import { OmitStrict } from '@common/types';
import { useField } from 'formik';
import React from 'react';

interface FormFieldThemeSelectProps<FormValues>
  extends OmitStrict<FormThemeSelectProps, 'error'> {
  name: FormValues extends Record<string, unknown> ? keyof FormValues : string;
}

function FormFieldThemeSelect<FormValues>({
  name,
  onChange,
  ...props
}: FormFieldThemeSelectProps<FormValues>) {
  const [field, meta, helpers] = useField(name as string);

  return (
    <FormThemeSelect
      {...props}
      error={meta.touched && meta.error ? meta.error : undefined}
      themeId={field.value}
      onChange={(themeId) => {
        if (onChange) {
          onChange(themeId);
        }

        helpers.setValue(themeId);
      }}
    />
  );
}

export default FormFieldThemeSelect;

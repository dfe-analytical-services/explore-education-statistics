import FormThemeTopicSelect, {
  FormThemeTopicSelectProps,
} from '@admin/components/form/FormThemeTopicSelect';
import { OmitStrict } from '@common/types';
import { useField } from 'formik';
import React from 'react';

interface FormFieldThemeTopicSelectProps<FormValues>
  extends OmitStrict<FormThemeTopicSelectProps, 'error' | 'topicId'> {
  name: FormValues extends Record<string, unknown> ? keyof FormValues : string;
}

function FormFieldThemeTopicSelect<FormValues>({
  name,
  onChange,
  ...props
}: FormFieldThemeTopicSelectProps<FormValues>) {
  const [field, meta, helpers] = useField(name as string);

  return (
    <FormThemeTopicSelect
      {...props}
      error={meta.touched && meta.error ? meta.error : undefined}
      topicId={field.value}
      onChange={(topicId, themeId) => {
        if (onChange) {
          onChange(topicId, themeId);
        }

        helpers.setValue(topicId);
      }}
    />
  );
}

export default FormFieldThemeTopicSelect;

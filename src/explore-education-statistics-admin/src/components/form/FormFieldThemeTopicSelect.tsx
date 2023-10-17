import FormThemeTopicSelect, {
  FormThemeTopicSelectProps,
} from '@admin/components/form/FormThemeTopicSelect';
import { OmitStrict } from '@common/types';
import getErrorMessage from '@common/components/form/rhf/util/getErrorMessage';
import React from 'react';
import {
  FieldValues,
  Path,
  PathValue,
  useFormContext,
  useWatch,
} from 'react-hook-form';

interface Props<TFormValues extends FieldValues>
  extends OmitStrict<FormThemeTopicSelectProps, 'error' | 'topicId'> {
  name: Path<TFormValues>;
}

export default function FormFieldThemeTopicSelect<
  TFormValues extends FieldValues,
>({ name, onChange, ...props }: Props<TFormValues>) {
  const {
    formState: { errors },
    setValue,
  } = useFormContext<TFormValues>();

  const value = useWatch({ name });

  return (
    <FormThemeTopicSelect
      {...props}
      error={getErrorMessage(errors, name)}
      topicId={value}
      onChange={(topicId, themeId) => {
        if (onChange) {
          onChange(topicId, themeId);
        }

        setValue(name, topicId as PathValue<TFormValues, Path<TFormValues>>);
      }}
    />
  );
}

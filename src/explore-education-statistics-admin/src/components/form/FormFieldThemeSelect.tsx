import FormThemeSelect, {
  FormThemeSelectProps,
} from '@admin/components/form/FormThemeSelect';
import { OmitStrict } from '@common/types';
import getErrorMessage from '@common/components/form/util/getErrorMessage';
import React from 'react';
import {
  FieldValues,
  Path,
  PathValue,
  useFormContext,
  useWatch,
} from 'react-hook-form';

interface Props<TFormValues extends FieldValues>
  extends OmitStrict<FormThemeSelectProps, 'error' | 'themeId'> {
  name: Path<TFormValues>;
}

export default function FormFieldThemeSelect<TFormValues extends FieldValues>({
  name,
  onChange,
  ...props
}: Props<TFormValues>) {
  const {
    formState: { errors },
    setValue,
  } = useFormContext<TFormValues>();

  const value = useWatch({ name });

  return (
    <FormThemeSelect
      {...props}
      error={getErrorMessage(errors, name)}
      themeId={value}
      onChange={themeId => {
        if (onChange) {
          onChange(themeId);
        }

        setValue(name, themeId as PathValue<TFormValues, Path<TFormValues>>);
      }}
    />
  );
}

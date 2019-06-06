import createErrorHelper from '@common/lib/validation/createErrorHelper';
import { OmitStrict } from '@common/types/util';
import { Field, FieldProps } from 'formik';
import React from 'react';
import FormRadioGroup, { FormRadioGroupProps } from './FormRadioGroup';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & OmitStrict<FormRadioGroupProps, 'value'>;

const FormFieldRadioGroup = <T extends {}>(props: Props<T>) => {
  const { error, name, onChange, showError = true } = props;

  return (
    <Field name={name}>
      {({ field, form }: FieldProps) => {
        const { getError } = createErrorHelper(form);

        let errorMessage = error || getError(name);

        if (!showError) {
          errorMessage = '';
        }

        return (
          <FormRadioGroup
            {...props}
            {...field}
            onChange={(event, option) => {
              if (onChange) {
                onChange(event, option);
              }

              if (!event.isDefaultPrevented()) {
                field.onChange(event);
              }
            }}
            error={errorMessage}
          />
        );
      }}
    </Field>
  );
};

export default FormFieldRadioGroup;

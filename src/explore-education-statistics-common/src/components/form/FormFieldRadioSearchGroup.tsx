/* eslint-disable react/destructuring-assignment */
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import { OmitStrict } from '@common/types';
import FormRadioSearchGroup, {
  FormRadioSearchGroupProps,
} from '@common/components/form/FormRadioSearchGroup';
import React from 'react';

export type FormFieldRadioSearchGroupProps<FormValues> = OmitStrict<
  FormFieldComponentProps<FormRadioSearchGroupProps, FormValues>,
  'formGroup'
>;

function FormFieldRadioSearchGroup<FormValues, Value extends string = string>(
  props: FormFieldRadioSearchGroupProps<FormValues>,
) {
  return (
    <FormField<Value> {...props}>
      {({ id, field }) => {
        return (
          <FormRadioSearchGroup
            {...props}
            {...field}
            id={id}
            onChange={(event, option) => {
              if (props.onChange) {
                props.onChange(event, option);
              }

              if (!event.isDefaultPrevented()) {
                field.onChange(event);
              }
            }}
          />
        );
      }}
    </FormField>
  );
}

export default FormFieldRadioSearchGroup;

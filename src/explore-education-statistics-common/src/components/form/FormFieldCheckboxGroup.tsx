/* eslint-disable react/destructuring-assignment */
import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import { OmitStrict } from '@common/types';
import React from 'react';
import FormCheckboxGroup, { FormCheckboxGroupProps } from './FormCheckboxGroup';
import handleAllChange from './util/handleAllCheckboxChange';

type Props<FormValues> = OmitStrict<
  FormFieldComponentProps<FormCheckboxGroupProps, FormValues>,
  'formGroup'
>;

function FormFieldCheckboxGroup<FormValues>(props: Props<FormValues>) {
  const { options } = props;

  return (
    <FormField<string[]> {...props} formGroup={false}>
      {({ id, field, helpers, meta }) => (
        <FormCheckboxGroup
          {...props}
          {...field}
          id={id}
          onAllChange={(event, checked) => {
            if (props.onAllChange) {
              props.onAllChange(event, checked, options);
            }

            if (event.isDefaultPrevented()) {
              return;
            }

            handleAllChange({
              checked,
              options,
              helpers,
              meta,
            });
          }}
          onChange={(event, option) => {
            if (props.onChange) {
              props.onChange(event, option);
            }

            if (event.isDefaultPrevented()) {
              return;
            }

            field.onChange(event);
          }}
        />
      )}
    </FormField>
  );
}

export default FormFieldCheckboxGroup;

import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import { OmitStrict } from '@common/types';
import React from 'react';
import FormCheckboxSearchGroup, {
  FormCheckboxSearchGroupProps,
} from './FormCheckboxSearchGroup';
import handleAllChange from './util/handleAllCheckboxChange';

export type FormFieldCheckboxSearchGroupProps<FormValues> = OmitStrict<
  FormFieldComponentProps<FormCheckboxSearchGroupProps>,
  'formGroup'
>;

const FormFieldCheckboxSearchGroup = <FormValues extends {}>(
  props: FormFieldCheckboxSearchGroupProps<FormValues>,
) => {
  const { options } = props;

  return (
    <FormField<string[]> {...props}>
      {({ field, helpers }) => {
        return (
          <FormCheckboxSearchGroup
            {...props}
            {...field}
            onAllChange={(event, checked) => {
              if (props.onAllChange) {
                props.onAllChange(event, checked);
              }

              handleAllChange({
                checked,
                helpers,
                event,
                options,
                value: field.value,
              });
            }}
            onChange={(event, option) => {
              if (props.onChange) {
                props.onChange(event, option);
              }

              field.onChange(event);
            }}
          />
        );
      }}
    </FormField>
  );
};

export default FormFieldCheckboxSearchGroup;

import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import { OmitStrict } from '@common/types';
import React from 'react';
import FormCheckboxSearchSubGroups, {
  FormCheckboxSearchSubGroupsProps,
} from './FormCheckboxSearchSubGroups';
import FormFieldCheckboxSearchGroup from './FormFieldCheckboxSearchGroup';
import handleAllChange from './util/handleAllCheckboxChange';

export type FormFieldCheckboxSearchSubGroupsProps<FormValues> = OmitStrict<
  FormFieldComponentProps<FormCheckboxSearchSubGroupsProps>,
  'formGroup'
>;

const FormFieldCheckboxSearchSubGroups = <FormValues extends {}>(
  props: FormFieldCheckboxSearchSubGroupsProps<FormValues>,
) => {
  const { options } = props;

  return (
    <>
      {options.length > 1 && (
        <FormField<string[]> {...props}>
          {({ field, helpers }) => {
            return (
              <FormCheckboxSearchSubGroups
                {...props}
                {...field}
                small
                onAllChange={(event, checked, groupOptions) => {
                  if (props.onAllChange) {
                    props.onAllChange(event, checked, groupOptions);
                  }

                  handleAllChange({
                    checked,
                    event,
                    helpers,
                    options: groupOptions,
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
      )}

      {options.length === 1 && (
        <FormFieldCheckboxSearchGroup
          {...props}
          onAllChange={(event, checked) => {
            if (props.onAllChange) {
              props.onAllChange(event, checked, options[0].options);
            }
          }}
          options={options[0].options}
        />
      )}
    </>
  );
};

export default FormFieldCheckboxSearchSubGroups;

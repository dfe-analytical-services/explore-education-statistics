import { OmitStrict } from '@common/types/util';
import React from 'react';
import FieldCheckboxArray from './FieldCheckboxArray';
import FormCheckboxSearchSubGroups, {
  FormCheckboxSearchSubGroupsProps,
} from './FormCheckboxSearchSubGroups';
import FormFieldCheckboxSearchGroup from './FormFieldCheckboxSearchGroup';
import {
  handleAllChange,
  handleChange,
} from './util/checkboxGroupFieldHelpers';

export type FormFieldCheckboxSearchSubGroupsProps<FormValues> = {
  showError?: boolean;
} & OmitStrict<FormCheckboxSearchSubGroupsProps, 'value'>;

const FormFieldCheckboxSearchSubGroups = <T extends {}>(
  props: FormFieldCheckboxSearchSubGroupsProps<T>,
) => {
  const { options } = props;

  return (
    <>
      {options.length > 1 && (
        <FieldCheckboxArray {...props}>
          {fieldArrayProps => {
            return (
              <FormCheckboxSearchSubGroups
                {...props}
                {...fieldArrayProps}
                small
                onAllChange={(event, checked, groupOptions) => {
                  if (props.onAllChange) {
                    props.onAllChange(event, checked, groupOptions);
                  }

                  handleAllChange(fieldArrayProps, groupOptions)(
                    event,
                    checked,
                  );
                }}
                onChange={(event, option) => {
                  if (props.onChange) {
                    props.onChange(event, option);
                  }

                  handleChange(fieldArrayProps)(event);
                }}
              />
            );
          }}
        </FieldCheckboxArray>
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

import FormField, {
  FormFieldComponentProps,
} from '@common/components/form/FormField';
import { OmitStrict } from '@common/types';
import React from 'react';
import FormCheckboxGroup, { FormCheckboxGroupProps } from './FormCheckboxGroup';
import handleAllChange from './util/handleAllCheckboxChange';

type Props<FormValues> = OmitStrict<
  FormFieldComponentProps<FormCheckboxGroupProps>,
  'formGroup'
>;

const FormFieldCheckboxGroup = <FormValues extends {}>(
  props: Props<FormValues>,
) => {
  const { options } = props;

  return (
    <FormField<string[]> {...props} formGroup={false}>
      {({ field, helpers }) => (
        <FormCheckboxGroup
          {...props}
          {...field}
          onAllChange={(event, checked) => {
            if (props.onAllChange) {
              props.onAllChange(event, checked);
            }

            handleAllChange({
              event,
              value: field.value,
              checked,
              options,
              helpers,
            });
          }}
          onChange={(event, option) => {
            if (props.onChange) {
              props.onChange(event, option);
            }

            field.onChange(event);
          }}
        />
      )}
    </FormField>
  );
};

export default FormFieldCheckboxGroup;

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

const FormFieldCheckboxGroup = <FormValues extends {}>(
  props: Props<FormValues>,
) => {
  const { options } = props;

  return (
    <FormField<string[]> {...props} formGroup={false}>
      {({ field, helpers, meta }) => (
        <FormCheckboxGroup
          {...props}
          {...field}
          onAllChange={(event, checked) => {
            if (props.onAllChange) {
              props.onAllChange(event, checked);
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
};

export default FormFieldCheckboxGroup;

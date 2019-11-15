import React from 'react';
import { Field, FieldProps } from 'formik';
import { get } from 'lodash';
import { FormCheckbox } from '@common/components/form';

const FieldSubjectCheckbox = ({
  id,
  name,
  label,
}: {
  name: string;
  id: string;
  label: string;
}) => {
  return (
    <Field name={name}>
      {({ form, field }: FieldProps) => {
        const defaultValue = get(form.values, name);
        return (
          <FormCheckbox
            id={id}
            name={name}
            label={label}
            value={field.value}
            defaultChecked={defaultValue}
          />
        );
      }}
    </Field>
  );
};

export default FieldSubjectCheckbox;

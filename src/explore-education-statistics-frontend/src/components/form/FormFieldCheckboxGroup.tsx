import { FieldArray } from 'formik';
import difference from 'lodash/difference';
import React from 'react';
import createErrorHelper from 'src/lib/validation/createErrorHelper';
import { Omit } from 'src/types/util';
import { FormCheckboxGroupProps } from './FormCheckboxGroup';
import { FormCheckboxGroup } from './index';

type Props<FormValues> = {
  name: keyof FormValues | string;
  showError?: boolean;
} & Omit<FormCheckboxGroupProps, 'value'>;

const FormFieldCheckboxGroup = <T extends {}>(props: Props<T>) => {
  const { error, name, options, showError = true } = props;

  return (
    <FieldArray name={name}>
      {({ form, ...helpers }) => {
        const { getError } = createErrorHelper(form);

        let errorMessage = error ? error : getError(name);

        if (!showError) {
          errorMessage = '';
        }

        return (
          <FormCheckboxGroup
            {...props}
            error={errorMessage}
            options={options}
            value={form.values[name]}
            onAllChange={event => {
              const allOptionValues = options.map(option => option.value);
              const restValues = difference(form.values[name], allOptionValues);

              if (event.target.checked) {
                form.setFieldValue(name, [...restValues, ...allOptionValues]);
              } else {
                form.setFieldValue(name, restValues);
              }
            }}
            onChange={event => {
              if (event.target.checked) {
                helpers.push(event.target.value);
              } else {
                const index = form.values[name].indexOf(event.target.value);

                if (index > -1) {
                  helpers.remove(index);
                }
              }
            }}
          />
        );
      }}
    </FieldArray>
  );
};

export default FormFieldCheckboxGroup;

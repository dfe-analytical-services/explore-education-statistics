import { FieldArray } from 'formik';
import difference from 'lodash/difference';
import get from 'lodash/get';
import React from 'react';
import createErrorHelper from '../../lib/validation/createErrorHelper';
import { Omit } from '../../types/util';
import FormCheckboxGroup, { FormCheckboxGroupProps } from './FormCheckboxGroup';

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

        const currentValues = get(form.values, name);

        return (
          <FormCheckboxGroup
            {...props}
            error={errorMessage}
            options={options}
            value={currentValues}
            onAllChange={event => {
              const allOptionValues = options.map(option => option.value);
              const restValues = difference(currentValues, allOptionValues);

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
                const index = currentValues.indexOf(event.target.value);

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

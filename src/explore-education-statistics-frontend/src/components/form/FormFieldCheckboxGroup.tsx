import { FieldArray } from 'formik';
import difference from 'lodash/difference';
import React, { Component } from 'react';
import createErrorHelper from 'src/lib/validation/createErrorHelper';
import { Omit } from 'src/types/util';
import { FormCheckboxGroupProps } from './FormCheckboxGroup';
import { FormCheckboxGroup } from './index';

type Props<FormValues> = {
  name: keyof FormValues | string;
} & Omit<FormCheckboxGroupProps, 'error'>;

/**
 * Convenience wrapper that integrates {@see FormCheckboxGroup}
 * with Formik using the {@see FieldArray} component.
 */
class FormFieldCheckboxGroup<
  FormValues extends { [key: string]: unknown }
> extends Component<Props<FormValues>> {
  public render() {
    const { name, options, value } = this.props;

    return (
      <FieldArray name={name}>
        {({ form, ...helpers }) => {
          const { getError } = createErrorHelper(form);

          return (
            <FormCheckboxGroup
              {...this.props}
              error={getError(name)}
              options={options}
              onAllChange={event => {
                const allOptionValues = options.map(option => option.value);
                const restValues = difference(
                  form.values[name],
                  allOptionValues,
                );

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
                  const index = value.indexOf(event.target.value);

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
  }
}

export default FormFieldCheckboxGroup;

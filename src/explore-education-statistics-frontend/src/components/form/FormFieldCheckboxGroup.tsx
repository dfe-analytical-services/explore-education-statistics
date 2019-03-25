import { FieldArray } from 'formik';
import difference from 'lodash/difference';
import React, { Component } from 'react';
import { FormCheckboxGroupProps } from './FormCheckboxGroup';
import { FormCheckboxGroup } from './index';

type Props<FormValues> = {
  name: keyof FormValues | string;
} & FormCheckboxGroupProps;

/**
 * Convenience wrapper that integrates {@see FormCheckboxGroup}
 * with Formik using the {@see FieldArray} component.
 */
class FormFieldCheckboxGroup<
  FormValues extends { [key: string]: unknown }
> extends Component<Props<FormValues>> {
  public render() {
    const { id, name, options, value, ...restProps } = this.props;

    return (
      <FieldArray name={name}>
        {({ form, ...helpers }) => (
          <FormCheckboxGroup
            {...restProps}
            value={value}
            name={name}
            id={id}
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
                const index = value.indexOf(event.target.value);

                if (index > -1) {
                  helpers.remove(index);
                }
              }
            }}
            options={options}
          />
        )}
      </FieldArray>
    );
  }
}

export default FormFieldCheckboxGroup;

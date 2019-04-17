import { Omit } from '@common/types/util';
import classNames from 'classnames';
import React, { Component, createRef } from 'react';
import FormCheckbox, {
  CheckboxChangeEventHandler,
  FormCheckboxProps,
} from './FormCheckbox';
import FormFieldset, { FieldSetProps } from './FormFieldset';

export type CheckboxOption = Omit<
  FormCheckboxProps,
  'name' | 'checked' | 'onChange'
>;

export type FormCheckboxGroupProps = {
  name: string;
  onAllChange?: CheckboxChangeEventHandler;
  onChange?: CheckboxChangeEventHandler;
  options: CheckboxOption[];
  selectAll?: boolean;
  small?: boolean;
  value: string[];
} & FieldSetProps;

interface State {
  checkedCount: number;
}

/**
 * Basic checkbox group that should be used as a controlled component.
 * When using Formik, use {@see FormFieldRadioGroup} instead.
 */
class FormCheckboxGroup extends Component<FormCheckboxGroupProps, State> {
  public static defaultProps = {
    legendSize: 'm',
    selectAll: false,
    small: false,
    value: [],
  };

  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    if (this.ref.current) {
      import('govuk-frontend/components/checkboxes/checkboxes').then(
        ({ default: GovUkCheckboxes }) => {
          new GovUkCheckboxes(this.ref.current).init();
        },
      );
    }
  }

  public render() {
    const {
      value,
      onAllChange,
      onChange,
      name,
      id,
      options,
      selectAll,
      small,
    } = this.props;
    const isAllChecked = options.every(
      option => value.indexOf(option.value) > -1,
    );

    return (
      <FormFieldset {...this.props}>
        <div
          className={classNames('govuk-checkboxes', {
            'govuk-checkboxes--small': small,
          })}
          ref={this.ref}
        >
          {options.length > 1 && selectAll && (
            <FormCheckbox
              id={`${id}-all`}
              label="Select all"
              name={name}
              value="select-all"
              checked={isAllChecked}
              onChange={event => {
                if (onAllChange) {
                  onAllChange(event);
                }
              }}
            />
          )}

          {options.map(option => (
            <FormCheckbox
              {...option}
              name={name}
              key={option.id}
              checked={value.indexOf(option.value) > -1}
              onChange={event => {
                if (onChange) {
                  onChange(event);
                }
              }}
            />
          ))}
        </div>
      </FormFieldset>
    );
  }
}

export default FormCheckboxGroup;

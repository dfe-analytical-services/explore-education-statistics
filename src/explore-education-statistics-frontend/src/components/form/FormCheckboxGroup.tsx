import Checkboxes from 'govuk-frontend/components/checkboxes/checkboxes';
import React, { Component, createRef } from 'react';
import FormCheckbox, { CheckboxChangeEventHandler } from './FormCheckbox';
import FormFieldSet, { FieldSetProps } from './FormFieldSet';

export interface CheckboxOption {
  hint?: string;
  id: string;
  label: string;
  value: string;
}

type Props = {
  name: string;
  onAllChange?: CheckboxChangeEventHandler;
  onChange?: CheckboxChangeEventHandler<any>;
  options: CheckboxOption[];
  value: string[];
} & FieldSetProps;

interface State {
  checkedCount: number;
}

class FormCheckboxGroup extends Component<Props, State> {
  public static defaultProps: Partial<Props> = {
    legendSize: 'm',
    value: [],
  };

  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    new Checkboxes(this.ref.current).init();
  }

  public render() {
    const { value, onAllChange, name, options } = this.props;

    const checkedCount = Object.values(value).filter(Boolean).length;

    return (
      <FormFieldSet {...this.props}>
        <div className="govuk-checkboxes" ref={this.ref}>
          {onAllChange && (
            <FormCheckbox
              id={`${name}-all`}
              label="Select all"
              name={name}
              value="select-all"
              checked={checkedCount === options.length}
              onChange={event => {
                if (this.props.onAllChange) {
                  this.props.onAllChange(event);
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
                if (this.props.onChange) {
                  this.props.onChange(event);
                }
              }}
            />
          ))}
        </div>
      </FormFieldSet>
    );
  }
}

export default FormCheckboxGroup;

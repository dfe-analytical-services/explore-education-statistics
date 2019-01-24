import Checkboxes from 'govuk-frontend/components/checkboxes/checkboxes';
import React, { Component, createRef } from 'react';
import FormCheckbox, { CheckboxChangeEventHandler } from './FormCheckbox';
import FormFieldSet, { FieldSetProps } from './FormFieldSet';

export interface CheckboxOption {
  checked?: boolean;
  hint?: string;
  id: string;
  label: string;
  value: string;
}

type Props = {
  checkedValues: {
    [value: string]: boolean;
  };
  name: string;
  onAllChange?: CheckboxChangeEventHandler;
  onChange?: CheckboxChangeEventHandler<any>;
  options: CheckboxOption[];
} & Partial<FieldSetProps>;

interface State {
  checkedCount: number;
}

class FormCheckboxGroup extends Component<Props, State> {
  public static defaultProps: Partial<Props> = {
    checkedValues: {},
    legendSize: 'm',
  };

  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    new Checkboxes(this.ref.current).init();
  }

  private handleChange: CheckboxChangeEventHandler = event => {
    if (this.props.onChange) {
      this.props.onChange(event);
    }
  };

  private handleAllChange: CheckboxChangeEventHandler = event => {
    if (this.props.onAllChange) {
      this.props.onAllChange(event);
    }
  };

  private renderCheckboxes() {
    const { checkedValues, onAllChange, name, options } = this.props;

    const checkedCount = Object.values(checkedValues).filter(Boolean).length;

    return (
      <div className="govuk-checkboxes" ref={this.ref}>
        {onAllChange && (
          <FormCheckbox
            id={`${name}-all`}
            label="Select all"
            name={name}
            value="select-all"
            checked={checkedCount === options.length}
            onChange={this.handleAllChange}
          />
        )}

        {options.map(option => (
          <FormCheckbox
            {...option}
            name={name}
            key={option.id}
            onChange={this.handleChange}
            checked={Boolean(checkedValues[option.value])}
          />
        ))}
      </div>
    );
  }

  public render() {
    const { legend, ...restProps } = this.props;

    return legend ? (
      <FormFieldSet {...restProps} legend={legend}>
        {this.renderCheckboxes()}
      </FormFieldSet>
    ) : (
      this.renderCheckboxes()
    );
  }
}

export default FormCheckboxGroup;

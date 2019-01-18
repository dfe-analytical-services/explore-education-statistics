import Checkboxes from 'govuk-frontend/components/checkboxes/checkboxes';
import React, { Component, createRef } from 'react';
import FormCheckbox from './FormCheckbox';
import FormFieldSet, { FieldSetProps } from './FormFieldSet';

interface CheckboxOption {
  checked?: boolean;
  hint?: string;
  id: string;
  label: string;
  value: string;
}

export type CheckboxGroupChangeEventHandler<
  T = {
    [value: string]: boolean;
  }
> = (state: T) => void;

type Props = {
  name: string;
  onChange?: CheckboxGroupChangeEventHandler<any>;
  options: CheckboxOption[];
} & Partial<FieldSetProps>;

interface State {
  values: {
    [value: string]: boolean;
  };
}

class FormCheckboxGroup extends Component<Props, State> {
  public static defaultProps: Partial<Props> = {
    legendSize: 'm',
  };

  public state: State = {
    values: {},
  };

  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    new Checkboxes(this.ref.current).init();
  }

  private isChecked({ value, checked = false }: CheckboxOption) {
    if (this.state.values[value] !== undefined) {
      return this.state.values[value];
    }

    return checked;
  }

  private handleChange({ value, checked = false }: CheckboxOption) {
    const isChecked =
      this.state.values[value] !== undefined
        ? this.state.values[value]
        : checked;

    this.setState(
      {
        values: {
          ...this.state.values,
          [value]: !isChecked,
        },
      },
      () => {
        if (this.props.onChange) {
          this.props.onChange(this.state.values);
        }
      },
    );
  }

  private renderCheckboxes() {
    return (
      <div className="govuk-checkboxes" ref={this.ref}>
        {this.props.options.map(option => (
          <FormCheckbox
            {...option}
            name={name}
            key={option.id}
            onChange={this.handleChange.bind(this, option)}
            checked={this.isChecked(option)}
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

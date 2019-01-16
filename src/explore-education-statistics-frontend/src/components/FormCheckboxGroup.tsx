import Checkboxes from 'govuk-frontend/components/checkboxes/checkboxes';
import React, { Component, createRef } from 'react';
import FormCheckbox from './FormCheckbox';

interface CheckboxOption {
  id: string;
  label: string;
  value: string;
  checked?: boolean;
}

interface Props {
  name: string;
  onChange?: (
    state: {
      [value: string]: boolean;
    },
  ) => void;
  options: CheckboxOption[];
}

interface State {
  values: {
    [value: string]: boolean;
  };
}

class FormCheckboxGroup extends Component<Props, State> {
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

  private handleChange = ({ value, checked = false }: CheckboxOption) => {
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
  };

  public render() {
    return (
      <div className="govuk-checkboxes" ref={this.ref}>
        {this.props.options.map(option => (
          <FormCheckbox
            {...option}
            name={name}
            key={option.id}
            onChange={this.handleChange.bind(null, option)}
            checked={this.isChecked(option)}
          />
        ))}
      </div>
    );
  }
}

export default FormCheckboxGroup;

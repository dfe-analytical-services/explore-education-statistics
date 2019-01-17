import classNames from 'classnames';
import Radios from 'govuk-frontend/components/radios/radios';
import React, { Component, createRef } from 'react';
import FormRadio from './FormRadio';

interface RadioOption {
  id: string;
  label: string;
  value: string;
}

interface Props {
  inline?: boolean;
  name: string;
  onChange?: (value: string | null) => void;
  options: RadioOption[];
}

interface State {
  selectedValue: string | null;
}

class FormRadioGroup extends Component<Props, State> {
  public static defaultProps: Partial<Props> = {
    inline: false,
  };

  public state: State = {
    selectedValue: null,
  };

  private ref = createRef<HTMLInputElement>();

  public componentDidMount(): void {
    new Radios(this.ref.current).init();
  }

  private isChecked({ value }: RadioOption) {
    return this.state.selectedValue === value;
  }

  private handleChange({ value }: RadioOption) {
    this.setState(
      {
        selectedValue: value,
      },
      () => {
        if (this.props.onChange) {
          this.props.onChange(this.state.selectedValue);
        }
      },
    );
  }

  public render() {
    const { inline, name, options } = this.props;

    return (
      <div
        className={classNames('govuk-radios', {
          'govuk-radios--inline': inline,
        })}
        ref={this.ref}
      >
        {options.map(option => (
          <FormRadio
            {...option}
            checked={this.isChecked(option)}
            key={option.id}
            name={name}
            onChange={this.handleChange.bind(this, option)}
          />
        ))}
      </div>
    );
  }
}

export default FormRadioGroup;

import classNames from 'classnames';
import Radios from 'govuk-frontend/components/radios/radios';
import React, { Component, createRef } from 'react';
import FormFieldSet, { FieldSetProps } from './FormFieldSet';
import FormRadio from './FormRadio';

interface RadioOption {
  id: string;
  label: string;
  value: string;
}

type Props = {
  inline?: boolean;
  name: string;
  onChange?: (value: string | null) => void;
  options: RadioOption[];
} & Partial<FieldSetProps>;

interface State {
  selectedValue: string | null;
}

class FormRadioGroup extends Component<Props, State> {
  public static defaultProps: Partial<Props> = {
    inline: false,
    legendSize: 'm',
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

  private renderRadios() {
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

  public render() {
    const { legend, ...restProps } = this.props;

    return legend ? (
      <FormFieldSet {...restProps} legend={legend}>
        {this.renderRadios()}
      </FormFieldSet>
    ) : (
      this.renderRadios()
    );
  }
}

export default FormRadioGroup;

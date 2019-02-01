import classNames from 'classnames';
import Radios from 'govuk-frontend/components/radios/radios';
import React, { ChangeEventHandler, Component, createRef } from 'react';
import FormFieldSet, { FieldSetProps } from './FormFieldSet';
import FormRadio, { RadioChangeEventHandler } from './FormRadio';

interface RadioOption {
  hint?: string;
  id: string;
  label: string;
  value: string;
}

type Props = {
  checkedValue: string | null;
  inline?: boolean;
  name: string;
  onChange?: RadioChangeEventHandler<any>;
  options: RadioOption[];
} & FieldSetProps;

class FormRadioGroup extends Component<Props> {
  public static defaultProps = {
    inline: false,
    legendSize: 'm',
  };

  private ref = createRef<HTMLInputElement>();

  public componentDidMount(): void {
    new Radios(this.ref.current).init();
  }

  private handleChange: ChangeEventHandler<HTMLInputElement> = event => {
    if (this.props.onChange) {
      this.props.onChange(event);
    }
  };

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
            checked={this.props.checkedValue === option.value}
            key={option.id}
            name={name}
            onChange={this.handleChange}
          />
        ))}
      </div>
    );
  }

  public render() {
    return <FormFieldSet {...this.props}>{this.renderRadios()}</FormFieldSet>;
  }
}

export default FormRadioGroup;

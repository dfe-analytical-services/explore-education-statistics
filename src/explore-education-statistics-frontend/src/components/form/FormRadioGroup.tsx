import classNames from 'classnames';
import React, { ChangeEventHandler, Component, createRef } from 'react';
import FormFieldset, { FieldSetProps } from './FormFieldset';
import FormRadio, { RadioChangeEventHandler } from './FormRadio';

interface RadioOption {
  hint?: string;
  id: string;
  label: string;
  value: string;
}

export type FormRadioGroupProps = {
  value: string | null;
  inline?: boolean;
  name: string;
  onChange?: RadioChangeEventHandler<any>;
  options: RadioOption[];
} & FieldSetProps;

class FormRadioGroup extends Component<FormRadioGroupProps> {
  public static defaultProps = {
    inline: false,
    legendSize: 'm',
  };

  private ref = createRef<HTMLInputElement>();

  public componentDidMount(): void {
    if (this.ref.current) {
      import('govuk-frontend/components/checkboxes/checkboxes').then(
        ({ default: GovUkRadios }) => {
          new GovUkRadios(this.ref.current).init();
        },
      );
    }
  }

  private handleChange: ChangeEventHandler<HTMLInputElement> = event => {
    if (this.props.onChange) {
      this.props.onChange(event);
    }
  };

  public render() {
    const { inline, name, options } = this.props;

    return (
      <FormFieldset {...this.props}>
        <div
          className={classNames('govuk-radios', {
            'govuk-radios--inline': inline,
          })}
          ref={this.ref}
        >
          {options.map(option => (
            <FormRadio
              {...option}
              checked={this.props.value === option.value}
              key={option.id}
              name={name}
              onChange={this.handleChange}
            />
          ))}
        </div>
      </FormFieldset>
    );
  }
}

export default FormRadioGroup;

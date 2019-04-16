import { Omit } from '@common/types/util';
import classNames from 'classnames';
import React, { Component, createRef } from 'react';
import FormFieldset, { FieldSetProps } from './FormFieldset';
import FormRadio, {
  FormRadioProps,
  RadioChangeEventHandler,
} from './FormRadio';

type RadioOption = Omit<FormRadioProps, 'checked' | 'name' | 'onChange'>;

export type FormRadioGroupProps = {
  inline?: boolean;
  name: string;
  onChange?: RadioChangeEventHandler;
  options: RadioOption[];
  value: string | null;
} & FieldSetProps;

class FormRadioGroup extends Component<FormRadioGroupProps> {
  public static defaultProps = {
    inline: false,
    legendSize: 'm',
    value: '',
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

  public render() {
    const { inline, name, onChange, options, value } = this.props;

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
              checked={value === option.value}
              key={option.id}
              name={name}
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

export default FormRadioGroup;

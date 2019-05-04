import { Omit, PartialBy } from '@common/types/util';
import classNames from 'classnames';
import kebabCase from 'lodash/kebabCase';
import React, { ChangeEvent, Component, createRef } from 'react';
import FormFieldset, { FormFieldsetProps } from './FormFieldset';
import FormRadio, { FormRadioProps } from './FormRadio';

type RadioOption = PartialBy<
  Omit<FormRadioProps, 'checked' | 'name' | 'onChange'>,
  'id'
>;

export type RadioGroupChangeEventHandler = (
  event: ChangeEvent<HTMLInputElement>,
  option: RadioOption,
) => void;

export type FormRadioGroupProps = {
  inline?: boolean;
  name: string;
  onChange?: RadioGroupChangeEventHandler;
  options: RadioOption[];
  small?: boolean;
  value: string | null;
} & FormFieldsetProps;

class FormRadioGroup extends Component<FormRadioGroupProps> {
  public static defaultProps = {
    inline: false,
    legendSize: 'm',
    small: false,
    value: '',
  };

  private ref = createRef<HTMLInputElement>();

  public componentDidMount(): void {
    if (this.ref.current) {
      import('govuk-frontend/components/radios/radios').then(
        ({ default: GovUkRadios }) => {
          new GovUkRadios(this.ref.current).init();
        },
      );
    }
  }

  public render() {
    const { id, inline, name, onChange, options, small, value } = this.props;

    return (
      <FormFieldset {...this.props}>
        <div
          className={classNames('govuk-radios', {
            'govuk-radios--inline': inline,
            'govuk-radios--small': small,
          })}
          ref={this.ref}
        >
          {options.map(option => (
            <FormRadio
              {...option}
              id={option.id ? option.id : `${id}-${kebabCase(option.value)}`}
              checked={value === option.value}
              key={option.value}
              name={name}
              onChange={event => {
                if (onChange) {
                  onChange(event, option);
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

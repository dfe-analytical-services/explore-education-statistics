import { OmitStrict, PartialBy } from '@common/types/util';
import classNames from 'classnames';
import kebabCase from 'lodash/kebabCase';
import orderBy from 'lodash/orderBy';
import React, { createRef, PureComponent } from 'react';
import FormFieldset, { FormFieldsetProps } from './FormFieldset';
import FormRadio, {
  FormRadioProps,
  RadioChangeEventHandler,
} from './FormRadio';

export type RadioOption<Value extends string = string> = PartialBy<
  OmitStrict<FormRadioProps, 'checked' | 'name' | 'onChange'>,
  'id'
> & {
  value: Value;
};

export type FormRadioGroupProps<Value extends string = string> = {
  disabled?: boolean;
  inline?: boolean;
  name: string;
  onChange?: RadioChangeEventHandler;
  options: RadioOption<Value>[];
  small?: boolean;
  order?:
    | (keyof RadioOption)[]
    | ((option: RadioOption) => RadioOption[keyof RadioOption])[];
  orderDirection?: ('asc' | 'desc')[];
  value?: string;
} & FormFieldsetProps;

class FormRadioGroup<Value extends string = string> extends PureComponent<
  FormRadioGroupProps<Value>
> {
  public static defaultProps = {
    inline: false,
    legendSize: 'm',
    small: false,
    order: ['label'],
    orderDirection: ['asc'],
    value: '',
  };

  private ref = createRef<HTMLInputElement>();

  private handleChange: RadioChangeEventHandler = (event, option) => {
    const { onChange } = this.props;

    if (onChange) {
      onChange(event, option);
    }
  };

  public render() {
    const {
      disabled,
      id,
      inline,
      name,
      options,
      small,
      order,
      orderDirection,
      value,
    } = this.props;

    const orderedOptions =
      orderDirection && orderDirection.length === 0
        ? options
        : orderBy(options, order, orderDirection);

    return (
      <FormFieldset {...this.props}>
        <div
          className={classNames('govuk-radios', {
            'govuk-radios--inline': inline,
            'govuk-radios--small': small,
          })}
          ref={this.ref}
        >
          {orderedOptions.map(option => (
            <FormRadio
              disabled={disabled}
              {...option}
              id={option.id ? option.id : `${id}-${kebabCase(option.value)}`}
              checked={value === option.value}
              key={option.value}
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

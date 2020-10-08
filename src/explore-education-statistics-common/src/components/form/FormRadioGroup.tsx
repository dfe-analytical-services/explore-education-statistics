import { OmitStrict, PartialBy } from '@common/types/util';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import React, { FocusEventHandler, memo } from 'react';
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
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: RadioChangeEventHandler;
  options: RadioOption<Value>[];
  small?: boolean;
  order?:
    | (keyof RadioOption)[]
    | ((option: RadioOption) => RadioOption[keyof RadioOption])[];
  orderDirection?: ('asc' | 'desc')[];
  value?: string;
} & OmitStrict<FormFieldsetProps, 'onBlur' | 'onFocus'> & {
    onFieldsetBlur?: FocusEventHandler<HTMLFieldSetElement>;
    onFieldsetFocus?: FocusEventHandler<HTMLFieldSetElement>;
  };

const FormRadioGroup = <Value extends string = string>({
  hint,
  inline = false,
  legendSize = 'm',
  small = false,
  order = ['label'],
  orderDirection = ['asc'],
  options,
  value = '',
  onFieldsetBlur,
  onFieldsetFocus,
  ...props
}: FormRadioGroupProps<Value>) => {
  const { id } = props;

  const orderedOptions =
    orderDirection && orderDirection.length === 0
      ? options
      : orderBy(options, order, orderDirection);

  return (
    <FormFieldset
      {...props}
      hint={hint}
      legendSize={legendSize}
      onBlur={onFieldsetBlur}
      onFocus={onFieldsetFocus}
    >
      <div
        className={classNames('govuk-radios', {
          'govuk-radios--inline': inline,
          'govuk-radios--small': small,
        })}
      >
        {orderedOptions.map(option => (
          <FormRadio
            {...props}
            {...option}
            id={
              option.id
                ? option.id
                : `${id}-${option.value.replace(/\s/g, '-')}`
            }
            checked={value === option.value}
            key={option.value}
          />
        ))}
      </div>
    </FormFieldset>
  );
};

export default memo(FormRadioGroup);

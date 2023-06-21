import { OmitStrict, PartialBy } from '@common/types/util';
import FormFieldset, {
  FormFieldsetProps,
} from '@common/components/form/FormFieldset';
import FormRadio, {
  FormRadioProps,
  RadioChangeEventHandler,
} from '@common/components/form/FormRadio';
import classNames from 'classnames';
import React, { FocusEventHandler, memo } from 'react';

export type RadioOption<Value extends string = string> = PartialBy<
  OmitStrict<FormRadioProps, 'checked' | 'name' | 'onChange'>,
  'id'
> & {
  value: Value;
};

export type BaseFormRadioGroupProps<Value extends string = string> = {
  disabled?: boolean;
  id: string;
  inline?: boolean;
  name: string;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: RadioChangeEventHandler;
  options: RadioOption<Value>[];
  small?: boolean;
  value?: string;
};

/**
 * Base radio group without wrapping fieldset.
 */
export const BaseFormRadioGroup = <Value extends string = string>({
  id,
  inline = false,
  small = false,
  options,
  value = '',
  ...props
}: BaseFormRadioGroupProps<Value>) => {
  return (
    <div
      className={classNames('govuk-radios', {
        'govuk-radios--inline': inline,
        'govuk-radios--small': small,
      })}
    >
      {options.map(option => (
        <FormRadio
          {...props}
          {...option}
          // label={`${option.label}: ${value}, ${option.value} ${
          //   value === option.value
          // }`}
          id={
            option.id
              ? `${id}-${option.id}`
              : `${id}-${option.value.replace(/\s/g, '-')}`
          }
          checked={value === option.value}
          key={option.value}
        />
      ))}
    </div>
  );
};

export type FormRadioGroupProps<
  Value extends string = string
> = BaseFormRadioGroupProps<Value> &
  OmitStrict<FormFieldsetProps, 'useFormId' | 'onBlur' | 'onFocus'> & {
    onFieldsetBlur?: FocusEventHandler<HTMLFieldSetElement>;
    onFieldsetFocus?: FocusEventHandler<HTMLFieldSetElement>;
  };

const FormRadioGroup = <Value extends string = string>({
  hint,
  legendSize = 'm',
  onFieldsetBlur,
  onFieldsetFocus,
  ...props
}: FormRadioGroupProps<Value>) => {
  return (
    <FormFieldset
      {...props}
      hint={hint}
      legendSize={legendSize}
      useFormId={false}
      onBlur={onFieldsetBlur}
      onFocus={onFieldsetFocus}
    >
      <BaseFormRadioGroup {...props} />
    </FormFieldset>
  );
};

export default memo(FormRadioGroup) as typeof FormRadioGroup;

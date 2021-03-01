import { OmitStrict, PartialBy } from '@common/types/util';
import naturalOrderBy, {
  OrderDirection,
  OrderKeys,
} from '@common/utils/array/naturalOrderBy';
import classNames from 'classnames';
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
  order?: OrderKeys<RadioOption<Value>>;
  orderDirection?: OrderDirection | OrderDirection[];
  value?: string;
} & OmitStrict<FormFieldsetProps, 'useFormId' | 'onBlur' | 'onFocus'> & {
    useFieldsetFormId?: boolean;
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

  return (
    <FormFieldset
      {...props}
      hint={hint}
      legendSize={legendSize}
      useFormId={false}
      onBlur={onFieldsetBlur}
      onFocus={onFieldsetFocus}
    >
      <div
        className={classNames('govuk-radios', {
          'govuk-radios--inline': inline,
          'govuk-radios--small': small,
        })}
      >
        {naturalOrderBy(options, order, orderDirection).map(option => (
          <FormRadio
            {...props}
            {...option}
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
    </FormFieldset>
  );
};

export default memo(FormRadioGroup) as typeof FormRadioGroup;

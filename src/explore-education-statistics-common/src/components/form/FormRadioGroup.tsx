import { OmitStrict, PartialBy } from '@common/types/util';
import naturalOrderBy, {
  OrderDirection,
  OrderKeys,
} from '@common/utils/array/naturalOrderBy';
import styles from '@common/components/form/FormRadioGroup.module.scss';
import classNames from 'classnames';
import omit from 'lodash/omit';
import React, { FocusEventHandler, Ref, memo } from 'react';
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
  divider?: string;
};

export type BaseFormRadioGroupProps<Value extends string = string> = {
  disabled?: boolean;
  id: string;
  inline?: boolean;
  inputRef?: Ref<HTMLInputElement>;
  name: string;
  /* Array of radio options.
   * Each option should have a unique value. Use the 'order' attribute to specify the order of the options.
   * */
  options: RadioOption<Value>[];
  /* Order of the radio options.
   * If set to an empty array, the order of the options will be the same as the order of the array.
   * */
  order?: OrderKeys<RadioOption<Value>>;
  orderDirection?: OrderDirection | OrderDirection[];
  small?: boolean;
  value?: string;

  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: RadioChangeEventHandler;
};

/**
 * Base radio group without wrapping fieldset.
 */
export const BaseFormRadioGroup = <Value extends string = string>({
  id,
  inline = false,
  order = ['label'],
  orderDirection = ['asc'],
  options,
  inputRef,
  small = false,
  value = '',
  ...props
}: BaseFormRadioGroupProps<Value>) => {
  return (
    <div
      className={classNames('govuk-radios', styles.radio, {
        'govuk-radios--inline': inline,
        [`govuk-radios--small ${styles.small}`]: small,
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
          inputRef={inputRef}
        />
      ))}
    </div>
  );
};

export type FormRadioGroupProps<Value extends string = string> =
  BaseFormRadioGroupProps<Value> &
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
      <BaseFormRadioGroup {...omit(props, 'className')} />
    </FormFieldset>
  );
};

export default memo(FormRadioGroup) as typeof FormRadioGroup;

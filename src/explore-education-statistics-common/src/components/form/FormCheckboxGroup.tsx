import ButtonText from '@common/components/ButtonText';
import useMounted from '@common/hooks/useMounted';
import { OmitStrict, PartialBy } from '@common/types/util';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import React, {
  FocusEventHandler,
  memo,
  MouseEvent,
  MouseEventHandler,
  useCallback,
  useMemo,
  useRef,
} from 'react';
import FormCheckbox, {
  CheckboxChangeEventHandler,
  FormCheckboxProps,
} from './FormCheckbox';
import styles from './FormCheckboxGroup.module.scss';
import FormFieldset, { FormFieldsetProps } from './FormFieldset';

export type CheckboxOption = PartialBy<
  OmitStrict<FormCheckboxProps, 'name' | 'checked' | 'onChange'>,
  'id'
>;

export type CheckboxGroupAllChangeEvent = MouseEvent<HTMLButtonElement>;

export type CheckboxGroupAllChangeEventHandler = (
  event: CheckboxGroupAllChangeEvent,
  checked: boolean,
) => void;

interface BaseFormCheckboxGroupProps {
  disabled?: boolean;
  id: string;
  name: string;
  options: CheckboxOption[];
  selectAll?: boolean;
  small?: boolean;
  order?:
    | (keyof CheckboxOption)[]
    | ((option: CheckboxOption) => CheckboxOption[keyof CheckboxOption])[];
  orderDirection?: ('asc' | 'desc')[];
  value: string[];
  onAllChange?: CheckboxGroupAllChangeEventHandler;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: CheckboxChangeEventHandler;
}

/**
 * Basic checkbox group that should be used as a controlled component.
 */
export const BaseFormCheckboxGroup = ({
  disabled,
  value = [],
  id,
  name,
  options,
  selectAll = false,
  small,
  order = ['label'],
  orderDirection = ['asc'],
  onBlur,
  onChange,
  onAllChange,
}: BaseFormCheckboxGroupProps) => {
  const ref = useRef<HTMLDivElement>(null);

  useMounted(() => {
    if (ref.current) {
      import('govuk-frontend/govuk/components/checkboxes/checkboxes').then(
        ({ default: GovUkCheckboxes }) => {
          if (ref.current) {
            new GovUkCheckboxes(ref.current).init();
          }
        },
      );
    }
  });

  const isAllChecked = useMemo(() => {
    return options.every(option => value.indexOf(option.value) > -1);
  }, [options, value]);

  const handleAllChange: MouseEventHandler<HTMLButtonElement> = useCallback(
    event => {
      if (onAllChange) {
        onAllChange(event, isAllChecked);
      }
    },
    [isAllChecked, onAllChange],
  );

  return (
    <div
      className={classNames('govuk-checkboxes', {
        'govuk-checkboxes--small': small,
      })}
      ref={ref}
    >
      {options.length > 1 && selectAll && (
        <ButtonText
          id={`${id}-all`}
          onClick={handleAllChange}
          className={styles.selectAll}
          underline={false}
        >
          {`${isAllChecked ? 'Unselect' : 'Select'} all ${
            options.length
          } options`}
        </ButtonText>
      )}

      {orderBy(options, order, orderDirection).map(option => (
        <FormCheckbox
          disabled={disabled}
          {...option}
          id={
            option.id ? option.id : `${id}-${option.value.replace(/\s/g, '-')}`
          }
          name={name}
          key={option.value}
          checked={value.indexOf(option.value) > -1}
          onBlur={onBlur}
          onChange={onChange}
        />
      ))}

      {options.length === 0 && <p>No options available.</p>}
    </div>
  );
};

export type FormCheckboxGroupProps = BaseFormCheckboxGroupProps &
  OmitStrict<FormFieldsetProps, 'onBlur' | 'onFocus'> & {
    onFieldsetBlur?: FocusEventHandler<HTMLFieldSetElement>;
    onFieldsetFocus?: FocusEventHandler<HTMLFieldSetElement>;
  };

const FormCheckboxGroup = ({
  onFieldsetBlur,
  onFieldsetFocus,
  ...props
}: FormCheckboxGroupProps) => {
  return (
    <FormFieldset {...props} onBlur={onFieldsetBlur} onFocus={onFieldsetFocus}>
      <BaseFormCheckboxGroup {...props} />
    </FormFieldset>
  );
};

export default memo(FormCheckboxGroup);

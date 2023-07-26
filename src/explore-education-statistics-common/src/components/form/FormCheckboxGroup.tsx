import ButtonText from '@common/components/ButtonText';
import useMounted from '@common/hooks/useMounted';
import { OmitStrict, PartialBy } from '@common/types/util';
import naturalOrderBy, {
  OrderDirection,
  OrderKeys,
} from '@common/utils/array/naturalOrderBy';
import classNames from 'classnames';
import React, {
  FocusEventHandler,
  memo,
  MouseEvent,
  MouseEventHandler,
  useCallback,
  useMemo,
  useRef,
  Ref,
  ReactNode,
} from 'react';

import VisuallyHidden from '../VisuallyHidden';
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
  filteredOptions: CheckboxOption[],
) => void;

interface BaseFormCheckboxGroupProps {
  disabled?: boolean;
  groupLabel?: string;
  id: string;
  name: string;
  options: CheckboxOption[];
  order?: OrderKeys<CheckboxOption>;
  orderDirection?: OrderDirection | OrderDirection[];
  selectAll?: boolean;
  selectAllText?: (
    isAllChecked: boolean,
    options: CheckboxOption[],
  ) => string | ReactNode;
  small?: boolean;
  value: string[];
  onAllChange?: CheckboxGroupAllChangeEventHandler;
  onBlur?: FocusEventHandler<HTMLInputElement>;
  onChange?: CheckboxChangeEventHandler;
  inputRef?: Ref<HTMLInputElement>;
}

export const getDefaultSelectAllText = (isAllChecked: boolean, total: number) =>
  `${isAllChecked ? 'Unselect' : 'Select'} all ${total} options`;

/**
 * Basic checkbox group that should be used as a controlled component.
 */
export const BaseFormCheckboxGroup = ({
  disabled,
  groupLabel,
  id,
  name,
  options,
  order = ['label'],
  orderDirection = ['asc'],
  selectAll = false,
  selectAllText,
  small,
  value = [],
  onBlur,
  onChange,
  onAllChange,
  inputRef,
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
    return options.every(option => value.includes(option.value));
  }, [options, value]);

  const handleAllChange: MouseEventHandler<HTMLButtonElement> = useCallback(
    event => {
      if (onAllChange) {
        onAllChange(event, isAllChecked, options);
      }
    },
    [isAllChecked, onAllChange, options],
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
          {selectAllText ? (
            selectAllText(isAllChecked, options)
          ) : (
            <>
              {getDefaultSelectAllText(isAllChecked, options.length)}
              {groupLabel && (
                <VisuallyHidden>{` for ${groupLabel}`}</VisuallyHidden>
              )}
            </>
          )}
        </ButtonText>
      )}
      <>
        {naturalOrderBy(options, order, orderDirection).map(option => (
          <FormCheckbox
            disabled={disabled}
            {...option}
            id={
              option.id
                ? `${id}-${option.id}`
                : `${id}-${option.value.replace(/\s/g, '-')}`
            }
            name={name}
            key={option.value}
            checked={value.includes(option.value)}
            onBlur={onBlur}
            onChange={onChange}
            inputRef={inputRef}
          />
        ))}
      </>
    </div>
  );
};

export type FormCheckboxGroupProps = BaseFormCheckboxGroupProps &
  OmitStrict<FormFieldsetProps, 'useFormId' | 'onBlur' | 'onFocus'> & {
    onFieldsetBlur?: FocusEventHandler<HTMLFieldSetElement>;
    onFieldsetFocus?: FocusEventHandler<HTMLFieldSetElement>;
  };

const FormCheckboxGroup = ({
  onFieldsetBlur,
  onFieldsetFocus,
  ...props
}: FormCheckboxGroupProps) => {
  return (
    <FormFieldset
      {...props}
      useFormId={false}
      onBlur={onFieldsetBlur}
      onFocus={onFieldsetFocus}
    >
      <BaseFormCheckboxGroup {...props} />
    </FormFieldset>
  );
};

export default memo(FormCheckboxGroup);

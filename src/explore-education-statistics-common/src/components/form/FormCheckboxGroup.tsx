import ButtonText from '@common/components/ButtonText';
import { Omit, PartialBy } from '@common/types/util';
import classNames from 'classnames';
import kebabCase from 'lodash/kebabCase';
import orderBy from 'lodash/orderBy';
import memoize from 'memoizee';
import React, {
  ChangeEvent,
  createRef,
  MouseEvent,
  MouseEventHandler,
  PureComponent,
} from 'react';
import FormCheckbox, {
  CheckboxChangeEventHandler,
  FormCheckboxProps,
} from './FormCheckbox';
import styles from './FormCheckboxGroup.module.scss';
import FormFieldset, { FormFieldsetProps } from './FormFieldset';

export type CheckboxOption = PartialBy<
  Omit<FormCheckboxProps, 'name' | 'checked' | 'onChange'>,
  'id'
>;

export type CheckboxGroupAllChangeEvent = MouseEvent<HTMLButtonElement>;
export type CheckboxGroupAllChangeEventHandler = (
  event: CheckboxGroupAllChangeEvent,
  options: CheckboxOption[],
) => void;

export type CheckboxGroupChangeEventHandler = (
  event: ChangeEvent<HTMLInputElement>,
  option: CheckboxOption,
) => void;

interface BaseFormCheckboxGroupProps {
  id: string;
  name: string;
  onAllChange?: CheckboxGroupAllChangeEventHandler;
  onChange?: CheckboxGroupChangeEventHandler;
  options: CheckboxOption[];
  selectAll?: boolean;
  small?: boolean;
  order?:
    | (keyof CheckboxOption)[]
    | ((option: CheckboxOption) => CheckboxOption[keyof CheckboxOption])[];
  orderDirection?: ('asc' | 'desc')[];
  value: string[];
}

export type FormCheckboxGroupProps = BaseFormCheckboxGroupProps &
  FormFieldsetProps;

interface State {
  checkedCount: number;
}

/**
 * Basic checkbox group that should be used as a controlled component.
 * When using Formik, use {@see FormFieldRadioGroup} instead.
 */
export class BaseFormCheckboxGroup extends PureComponent<
  BaseFormCheckboxGroupProps,
  State
> {
  public static defaultProps = {
    legendSize: 'm',
    selectAll: false,
    small: false,
    order: ['label'],
    orderDirection: ['asc'],
    value: [],
  };

  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    if (this.ref.current) {
      import('govuk-frontend/components/checkboxes/checkboxes').then(
        ({ default: GovUkCheckboxes }) => {
          if (this.ref.current) {
            new GovUkCheckboxes(this.ref.current).init();
          }
        },
      );
    }
  }

  private handleAllChange: MouseEventHandler<HTMLButtonElement> = event => {
    const { onAllChange, options } = this.props;

    if (onAllChange) {
      onAllChange(event, options);
    }
  };

  // eslint-disable-next-line react/sort-comp
  private handleChange = memoize(
    (option: CheckboxOption): CheckboxChangeEventHandler => event => {
      const { onChange } = this.props;

      if (onChange) {
        onChange(event, option);
      }
    },
    {
      normalizer: ([option]: [CheckboxOption]) =>
        `${option.value}-${option.label}-${option.id}`,
    },
  );

  public render() {
    const {
      value,
      name,
      id,
      options,
      selectAll,
      order,
      orderDirection,
      small,
    } = this.props;

    const isAllChecked = options.every(
      option => value.indexOf(option.value) > -1,
    );

    return (
      <div
        className={classNames('govuk-checkboxes', {
          'govuk-checkboxes--small': small,
        })}
        ref={this.ref}
      >
        {options.length > 1 && selectAll && (
          <ButtonText
            id={`${id}-all`}
            onClick={this.handleAllChange}
            className={styles.selectAll}
          >
            {isAllChecked ? 'Unselect' : 'Select'} all {options.length} options
          </ButtonText>
        )}

        {orderBy(options, order, orderDirection).map(option => (
          <FormCheckbox
            {...option}
            id={option.id ? option.id : `${id}-${kebabCase(option.value)}`}
            name={name}
            key={option.value}
            checked={value.indexOf(option.value) > -1}
            onChange={this.handleChange(option)}
          />
        ))}

        {options.length === 0 && <p>No options available.</p>}
      </div>
    );
  }
}

const FormCheckboxGroup = (props: FormCheckboxGroupProps) => {
  return (
    <FormFieldset {...props}>
      <BaseFormCheckboxGroup {...props} />
    </FormFieldset>
  );
};

export default FormCheckboxGroup;

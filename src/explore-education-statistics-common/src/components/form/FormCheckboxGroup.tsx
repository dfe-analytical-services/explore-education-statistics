import ButtonText from '@common/components/ButtonText';
import { OmitStrict, PartialBy } from '@common/types/util';
import classNames from 'classnames';
import kebabCase from 'lodash/kebabCase';
import orderBy from 'lodash/orderBy';
import React, {
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
  OmitStrict<FormCheckboxProps, 'name' | 'checked' | 'onChange'>,
  'id'
>;

export type CheckboxGroupAllChangeEvent = MouseEvent<HTMLButtonElement>;

export type CheckboxGroupAllChangeEventHandler = (
  event: CheckboxGroupAllChangeEvent,
  checked: boolean,
) => void;

interface BaseFormCheckboxGroupProps {
  id: string;
  name: string;
  onAllChange?: CheckboxGroupAllChangeEventHandler;
  onChange?: CheckboxChangeEventHandler;
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

/**
 * Basic checkbox group that should be used as a controlled component.
 */
export class BaseFormCheckboxGroup extends PureComponent<
  BaseFormCheckboxGroupProps
> {
  public static defaultProps = {
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
    const { onAllChange } = this.props;

    if (onAllChange) {
      onAllChange(event, this.isAllChecked());
    }
  };

  private handleChange: CheckboxChangeEventHandler = (event, option) => {
    const { onChange } = this.props;

    if (onChange) {
      onChange(event, option);
    }
  };

  private isAllChecked = () => {
    const { options, value } = this.props;

    return options.every(option => value.indexOf(option.value) > -1);
  };

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
            underline={false}
          >
            {`${this.isAllChecked() ? 'Unselect' : 'Select'} all ${
              options.length
            } options`}
          </ButtonText>
        )}

        {orderBy(options, order, orderDirection).map(option => (
          <FormCheckbox
            {...option}
            id={option.id ? option.id : `${id}-${kebabCase(option.value)}`}
            name={name}
            key={option.value}
            checked={value.indexOf(option.value) > -1}
            onChange={this.handleChange}
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

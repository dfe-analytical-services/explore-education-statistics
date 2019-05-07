import { Omit, PartialBy } from '@common/types/util';
import classNames from 'classnames';
import kebabCase from 'lodash/kebabCase';
import sortBy from 'lodash/sortBy';
import memoize from 'memoizee';
import React, { ChangeEvent, createRef, PureComponent } from 'react';
import FormCheckbox, {
  CheckboxChangeEventHandler,
  FormCheckboxProps,
} from './FormCheckbox';
import FormFieldset, { FormFieldsetProps } from './FormFieldset';

export type CheckboxOption = PartialBy<
  Omit<FormCheckboxProps, 'name' | 'checked' | 'onChange'>,
  'id'
>;

export type CheckboxGroupAllChangeEventHandler = (
  event: ChangeEvent<HTMLInputElement>,
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
  sort?:
    | string[]
    | ((option: CheckboxOption) => CheckboxOption[keyof CheckboxOption])[];
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
    sort: ['label'],
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

  private handleAllChange: CheckboxChangeEventHandler = event => {
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
    { primitive: true },
  );

  public render() {
    const {
      value,
      name,
      id,
      options,
      selectAll,
      sort = [],
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
          <FormCheckbox
            id={`${id}-all`}
            label="Select all"
            name={name}
            value="select-all"
            checked={isAllChecked}
            onChange={this.handleAllChange}
          />
        )}

        {sortBy(options, sort).map(option => (
          <FormCheckbox
            {...option}
            id={option.id ? option.id : `${id}-${kebabCase(option.value)}`}
            name={name}
            key={option.value}
            checked={value.indexOf(option.value) > -1}
            onChange={this.handleChange(option)}
          />
        ))}
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

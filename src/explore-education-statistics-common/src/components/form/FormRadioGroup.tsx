import { Omit, PartialBy } from '@common/types/util';
import classNames from 'classnames';
import kebabCase from 'lodash/kebabCase';
import sortBy from 'lodash/sortBy';
import memoize from 'memoizee';
import React, { ChangeEvent, createRef, PureComponent } from 'react';
import FormFieldset, { FormFieldsetProps } from './FormFieldset';
import FormRadio, {
  FormRadioProps,
  RadioChangeEventHandler,
} from './FormRadio';

type RadioOption = PartialBy<
  Omit<FormRadioProps, 'checked' | 'name' | 'onChange'>,
  'id'
>;

export type RadioGroupChangeEventHandler = (
  event: ChangeEvent<HTMLInputElement>,
  option: RadioOption,
) => void;

export type FormRadioGroupProps = {
  inline?: boolean;
  name: string;
  onChange?: RadioGroupChangeEventHandler;
  options: RadioOption[];
  small?: boolean;
  sort?: string[] | ((option: RadioOption) => RadioOption[keyof RadioOption])[];
  value: string | null;
} & FormFieldsetProps;

class FormRadioGroup extends PureComponent<FormRadioGroupProps> {
  public static defaultProps = {
    inline: false,
    legendSize: 'm',
    small: false,
    sort: ['label'],
    value: '',
  };

  private ref = createRef<HTMLInputElement>();

  // eslint-disable-next-line react/sort-comp
  private handleChange = memoize(
    (option: RadioOption): RadioChangeEventHandler => event => {
      const { onChange } = this.props;

      if (onChange) {
        onChange(event, option);
      }
    },
    { primitive: true },
  );

  // eslint-disable-next-line react/sort-comp
  public componentDidMount(): void {
    if (this.ref.current) {
      import('govuk-frontend/components/radios/radios').then(
        ({ default: GovUkRadios }) => {
          new GovUkRadios(this.ref.current).init();
        },
      );
    }
  }

  public render() {
    const { id, inline, name, options, small, sort = [], value } = this.props;

    return (
      <FormFieldset {...this.props}>
        <div
          className={classNames('govuk-radios', {
            'govuk-radios--inline': inline,
            'govuk-radios--small': small,
          })}
          ref={this.ref}
        >
          {sortBy(options, sort).map(option => (
            <FormRadio
              {...option}
              id={option.id ? option.id : `${id}-${kebabCase(option.value)}`}
              checked={value === option.value}
              key={option.value}
              name={name}
              onChange={this.handleChange(option)}
            />
          ))}
        </div>
      </FormFieldset>
    );
  }
}

export default FormRadioGroup;

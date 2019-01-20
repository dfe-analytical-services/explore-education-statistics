import Checkboxes from 'govuk-frontend/components/checkboxes/checkboxes';
import React, { ChangeEventHandler, Component, createRef } from 'react';
import FormCheckbox from './FormCheckbox';
import FormFieldSet, { FieldSetProps } from './FormFieldSet';

interface CheckboxOption {
  checked?: boolean;
  hint?: string;
  id: string;
  label: string;
  value: string;
}

type Props = {
  checkedValues: {
    [value: string]: boolean;
  };
  name: string;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  options: CheckboxOption[];
} & Partial<FieldSetProps>;

class FormCheckboxGroup extends Component<Props> {
  public static defaultProps: Partial<Props> = {
    checkedValues: {},
    legendSize: 'm',
  };

  private ref = createRef<HTMLDivElement>();

  public componentDidMount(): void {
    new Checkboxes(this.ref.current).init();
  }

  private handleChange: ChangeEventHandler<HTMLInputElement> = event => {
    if (this.props.onChange) {
      this.props.onChange(event);
    }
  };

  private renderCheckboxes() {
    return (
      <div className="govuk-checkboxes" ref={this.ref}>
        {this.props.options.map(option => (
          <FormCheckbox
            {...option}
            name={name}
            key={option.id}
            onChange={this.handleChange}
            checked={this.props.checkedValues[option.value]}
          />
        ))}
      </div>
    );
  }

  public render() {
    const { legend, ...restProps } = this.props;

    return legend ? (
      <FormFieldSet {...restProps} legend={legend}>
        {this.renderCheckboxes()}
      </FormFieldSet>
    ) : (
      this.renderCheckboxes()
    );
  }
}

export default FormCheckboxGroup;

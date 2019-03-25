import classNames from 'classnames';
import React, { ChangeEventHandler, Component, ReactNode } from 'react';
import ErrorMessage from '../ErrorMessage';
import createDescribedBy from './util/createDescribedBy';

interface Props {
  error?: ReactNode | string;
  hint?: string;
  id: string;
  label: ReactNode | string;
  name: string;
  onChange?: ChangeEventHandler<HTMLInputElement>;
  width?: 20 | 10 | 5 | 4 | 3 | 2;
}

class FormTextInput extends Component<Props> {
  private handleChange: ChangeEventHandler<HTMLInputElement> = event => {
    if (this.props.onChange) {
      this.props.onChange(event);
    }
  };

  public render() {
    const { error, hint, id, label, name, width } = this.props;

    return (
      <>
        <label className="govuk-label" htmlFor={id}>
          {label}
        </label>
        {hint && (
          <span id={`${id}-hint`} className="govuk-hint">
            {hint}
          </span>
        )}
        {error && <ErrorMessage id={`${id}-error`}>{error}</ErrorMessage>}
        <input
          aria-describedby={createDescribedBy({
            id,
            error: !!error,
            hint: !!hint,
          })}
          type="text"
          className={classNames('govuk-input', {
            [`govuk-input--width-${width}`]: width !== undefined,
          })}
          id={id}
          name={name}
          onChange={this.handleChange}
        />
      </>
    );
  }
}

export default FormTextInput;

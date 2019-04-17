import { FormTextInputProps } from '@common/components/form/FormTextInput';
import classNames from 'classnames';
import debounce from 'lodash/debounce';
import React, { ChangeEvent } from 'react';
import ErrorMessage from '../ErrorMessage';
import styles from './FormTextSearchInput.module.scss';
import createDescribedBy from './util/createDescribedBy';

interface Props extends FormTextInputProps {
  debounce?: number;
  labelHidden?: boolean;
}

const FormTextSearchInput = ({
  debounce: debounceTime = 300,
  error,
  hint,
  id,
  label,
  labelHidden,
  name,
  onChange,
  width,
}: Props) => {
  const handleChange = debounce((event: ChangeEvent<HTMLInputElement>) => {
    if (onChange) {
      onChange(event);
    }
  }, debounceTime);

  return (
    <>
      <label
        className={classNames('govuk-label', {
          'govuk-visually-hidden': labelHidden,
        })}
        htmlFor={id}
      >
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
        className={classNames('govuk-input', styles.searchInput, {
          [`govuk-input--width-${width}`]: width !== undefined,
        })}
        id={id}
        name={name}
        onChange={event => {
          event.persist();
          handleChange(event);
        }}
      />
    </>
  );
};

export default FormTextSearchInput;

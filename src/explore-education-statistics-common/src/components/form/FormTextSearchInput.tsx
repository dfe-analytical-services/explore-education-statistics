import { FormTextInputProps } from '@common/components/form/FormTextInput';
import classNames from 'classnames';
import React from 'react';
import ErrorMessage from '../ErrorMessage';
import styles from './FormTextSearchInput.module.scss';
import createDescribedBy from './util/createDescribedBy';

type Props = FormTextInputProps;

const FormTextSearchInput = ({
  error,
  hint,
  id,
  label,
  name,
  onChange,
  width,
}: Props) => {
  return (
    <>
      <label className="govuk-label govuk-visually-hidden" htmlFor={id}>
        {label}
      </label>
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
        onChange={onChange}
      />
    </>
  );
};

export default FormTextSearchInput;

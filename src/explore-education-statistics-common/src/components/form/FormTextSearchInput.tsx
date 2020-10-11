import { FormTextInputProps } from '@common/components/form/FormTextInput';
import { FormTextInput } from '@common/components/form/index';
import debounce from 'lodash/debounce';
import React, { ChangeEvent } from 'react';
import styles from './FormTextSearchInput.module.scss';

interface Props extends FormTextInputProps {
  debounce?: number;
}

const FormTextSearchInput = ({
  debounce: debounceTime = 300,
  onChange,
  ...props
}: Props) => {
  const handleChange = debounce((event: ChangeEvent<HTMLInputElement>) => {
    if (onChange) {
      onChange(event);
    }
  }, debounceTime);

  return (
    <FormTextInput
      {...props}
      className={styles.searchInput}
      onChange={event => {
        event.persist();
        handleChange(event);
      }}
    />
  );
};

export default FormTextSearchInput;

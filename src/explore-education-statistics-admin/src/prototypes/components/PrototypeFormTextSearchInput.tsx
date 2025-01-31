import { FormTextInputProps } from '@common/components/form/FormTextInput';
import { FormTextInput } from '@common/components/form/index';
import classNames from 'classnames';
import debounce from 'lodash/debounce';
import React, { ChangeEvent, useState } from 'react';
import styles from '@common/components/form/FormTextSearchInput.module.scss';

interface Props extends FormTextInputProps {
  debounce?: number;
}

const PrototypeFormTextSearchInput = ({
  className,
  debounce: debounceTime = 300,
  onChange,
  ...props
}: Props) => {
  const [value, setValue] = useState<string>(props.value ?? '');

  const handleChange = debounce((event: ChangeEvent<HTMLInputElement>) => {
    if (onChange) {
      onChange(event);
    }
  }, debounceTime);

  return (
    <FormTextInput
      {...props}
      className={classNames(className, styles.searchInput)}
      value={value}
      onChange={event => {
        event.persist();
        setValue(event.target.value);
        handleChange(event);
      }}
    />
  );
};

export default PrototypeFormTextSearchInput;

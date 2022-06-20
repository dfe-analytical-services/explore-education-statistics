import { FormTextInputProps } from '@common/components/form/FormTextInput';
import { FormTextInput } from '@common/components/form/index';
import styles from '@common/components/form/FormTextSearchInput.module.scss';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import classNames from 'classnames';
import React, { ChangeEvent, useEffect, useState } from 'react';

interface Props extends FormTextInputProps {
  debounce?: number;
}

const FormTextSearchInput = ({
  className,
  debounce: debounceTime = 300,
  onChange,
  ...props
}: Props) => {
  const [value, setValue] = useState<string>(props.value ?? '');

  useEffect(() => {
    setValue(props.value ?? '');
  }, [props.value]);

  const [handleChange] = useDebouncedCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      if (onChange) {
        onChange(event);
      }
    },
    debounceTime,
  );

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

export default FormTextSearchInput;

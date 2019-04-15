import { FormTextInput } from '@common/components/form';
import debounce from 'lodash/debounce';
import React, { ChangeEvent, ChangeEventHandler } from 'react';

interface Props {
  id: string;
  label: string;
  name: string;
  onChange: ChangeEventHandler<HTMLInputElement>;
}

const SearchTextInput = ({ onChange, ...restProps }: Props) => {
  const setDebouncedFilterSearch = debounce(
    (event: ChangeEvent<HTMLInputElement>) => onChange(event),
    300,
  );

  return (
    <FormTextInput
      {...restProps}
      onChange={event => {
        event.persist();
        setDebouncedFilterSearch(event);
      }}
      width={10}
    />
  );
};

export default SearchTextInput;

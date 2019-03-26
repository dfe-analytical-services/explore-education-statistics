import debounce from 'lodash/debounce';
import React, { ChangeEvent, ChangeEventHandler } from 'react';
import { FormTextInput } from 'src/components/form';

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
      width={20}
    />
  );
};

export default SearchTextInput;

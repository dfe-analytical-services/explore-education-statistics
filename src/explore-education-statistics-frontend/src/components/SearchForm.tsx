import React, { useEffect, useState } from 'react';
import FormSearchBar from '@common/components/form/FormSearchBar';

const min = 3;
const formId = 'searchForm';

interface Props {
  label?: string;
  value?: string;
  onSubmit: (value: string) => void;
}

const SearchForm = ({
  label = 'Search',
  value: initialValue = '',
  onSubmit,
}: Props) => {
  const [searchTerm, setSearchTerm] = useState<string>(initialValue);

  useEffect(() => {
    setSearchTerm(initialValue);
  }, [initialValue]);

  return (
    <form
      id={formId}
      className="govuk-!-margin-bottom-2"
      onSubmit={e => {
        e.preventDefault();
        if (searchTerm.length >= min) {
          onSubmit(searchTerm);
        }
      }}
    >
      <FormSearchBar
        id={`${formId}-search`}
        label={label}
        min={min}
        name="search"
        value={searchTerm}
        onChange={setSearchTerm}
      />
    </form>
  );
};

export default SearchForm;

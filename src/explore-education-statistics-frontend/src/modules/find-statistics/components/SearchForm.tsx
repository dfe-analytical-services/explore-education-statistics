import React, { useEffect, useState } from 'react';
import FormSearchBar from '@common/components/form/FormSearchBar';

const min = 3;

interface Props {
  value?: string;
  onSubmit: (value: string) => void;
}

const SearchForm = ({ value: initialValue = '', onSubmit }: Props) => {
  const [searchTerm, setSearchTerm] = useState<string>(initialValue);

  useEffect(() => {
    setSearchTerm(initialValue);
  }, [initialValue]);

  return (
    <form
      id="searchForm"
      className="govuk-!-margin-bottom-2"
      onSubmit={e => {
        e.preventDefault();
        if (searchTerm.length >= min) {
          onSubmit(searchTerm);
        }
      }}
    >
      <FormSearchBar min={min} value={searchTerm} onChange={setSearchTerm} />
    </form>
  );
};

export default SearchForm;

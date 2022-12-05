import FormBaseInput from '@common/components/form/FormBaseInput';
import FormGroup from '@common/components/form/FormGroup';
import SearchIcon from '@common/components/SearchIcon';
import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@frontend/modules/find-statistics/components/SearchForm.module.scss';
import React, { useEffect, useState } from 'react';
import useToggle from '@common/hooks/useToggle';

const minCharacters = 3;

interface Props {
  searchTerm?: string;
  onSubmit: (searchTerm: string) => void;
}

const SearchForm = ({
  searchTerm: initialSearchTerm = '',
  onSubmit,
}: Props) => {
  const [searchTerm, setSearchTerm] = useState<string>(initialSearchTerm);
  const [showError, toggleError] = useToggle(false);

  useEffect(() => {
    setSearchTerm(initialSearchTerm);
  }, [initialSearchTerm]);

  return (
    <form
      id="searchForm"
      className="govuk-!-margin-bottom-2"
      onSubmit={e => {
        e.preventDefault();
        if (searchTerm.length >= minCharacters) {
          toggleError.off();
          return onSubmit(searchTerm);
        }
        return toggleError.on();
      }}
    >
      <FormGroup hasError={showError}>
        <FormBaseInput
          addOn={
            <button className={styles.button} type="submit">
              <SearchIcon className={styles.icon} />
              <VisuallyHidden>Search</VisuallyHidden>
            </button>
          }
          className={styles.input}
          id="searchTerm"
          name="search"
          error={
            showError
              ? `Search must be at least ${minCharacters} characters`
              : undefined
          }
          label="Search"
          labelSize="m"
          type="search"
          value={searchTerm}
          onChange={e => {
            setSearchTerm(e.target.value);
            if (
              e.target.value.length >= minCharacters ||
              !e.target.value.length
            ) {
              toggleError.off();
            }
          }}
        />
      </FormGroup>
    </form>
  );
};

export default SearchForm;

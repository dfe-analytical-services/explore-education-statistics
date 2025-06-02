import FormProvider from '@common/components/form/FormProvider';
import { Form, FormFieldTextInput } from '@common/components/form';
import SearchIcon from '@common/components/SearchIcon';
import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@common/components/form/FormSearchBar.module.scss';
import React from 'react';

interface Props {
  label?: string;
  value?: string;
  onSubmit: (value: string) => void;
}

export default function SearchForm({
  label = 'Search',
  value: initialValue = '',
  onSubmit,
}: Props) {
  return (
    <FormProvider
      initialValues={{
        search: initialValue,
      }}
    >
      <Form
        id="searchForm"
        showErrorSummary={false}
        onSubmit={({ search }) => onSubmit(search)}
      >
        <FormFieldTextInput
          addOn={
            <button type="submit" className={styles.button}>
              <SearchIcon className={styles.icon} />
              <VisuallyHidden>Search</VisuallyHidden>
            </button>
          }
          announceError
          id="search"
          label={label}
          labelSize="m"
          name="search"
        />
      </Form>
    </FormProvider>
  );
}

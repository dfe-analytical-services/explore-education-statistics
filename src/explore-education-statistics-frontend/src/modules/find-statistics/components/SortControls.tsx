import Button from '@common/components/Button';
import { FormRadioGroup, FormSelect } from '@common/components/form';
import { useMobileMedia } from '@common/hooks/useMedia';
import { PublicationSortOption } from '@common/services/publicationService';
import styles from '@frontend/modules/find-statistics/components/SortControls.module.scss';
import React from 'react';

interface Option {
  label: string;
  value: PublicationSortOption;
}

const defaultOptions: Option[] = [
  { label: 'Newest', value: 'newest' },
  { label: 'Oldest', value: 'oldest' },
  { label: 'A to Z', value: 'title' },
];

const formId = 'sortControlsForm';
const fieldId = `${formId}-sortBy`;

interface Props {
  hasSearch?: boolean;
  sortBy: PublicationSortOption;
  onChange: (nextSortBy: PublicationSortOption) => void;
}

const SortControls = ({ hasSearch = false, sortBy, onChange }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  const options = hasSearch
    ? [...defaultOptions, { label: 'Relevance', value: 'relevance' }]
    : defaultOptions;

  return (
    <form id={formId} className={styles.form}>
      {isMobileMedia ? (
        <FormSelect
          id={fieldId}
          inline
          name="sortBy"
          label="Sort results"
          options={options}
          order={[]}
          value={sortBy}
          onChange={event =>
            onChange(event.target.value as PublicationSortOption)
          }
        />
      ) : (
        <FormRadioGroup
          id={fieldId}
          inline
          legend="Sort results"
          legendSize="s"
          name="sortBy"
          options={options}
          order={[]}
          small
          value={sortBy}
          onChange={event =>
            onChange(event.target.value as PublicationSortOption)
          }
        />
      )}

      <Button className="dfe-js-hidden govuk-!-margin-top-4" type="submit">
        Submit
      </Button>
    </form>
  );
};

export default SortControls;

import Button from '@common/components/Button';
import { FormRadioGroup, FormSelect } from '@common/components/form';
import { useMobileMedia } from '@common/hooks/useMedia';
import { PublicationSortOption } from '@common/services/publicationService';
import styles from '@frontend/components/SortControls.module.scss';
import { DataSetFileSortOption } from '@frontend/services/dataSetFileService';
import classNames from 'classnames';
import React from 'react';

export interface SortOption {
  label: string;
  value: PublicationSortOption;
}

type OptionType = PublicationSortOption | DataSetFileSortOption;

const formId = 'sortControlsForm';
const fieldId = `${formId}-sortBy`;

interface Props {
  className?: string;
  options: SortOption[];
  sortBy: OptionType;
  onChange: (nextSortBy: OptionType) => void;
}

const SortControls = ({ className, options, sortBy, onChange }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <form id={formId} className={classNames(styles.form, className)}>
      {isMobileMedia ? (
        <FormSelect
          id={fieldId}
          inline
          name="sortBy"
          label="Sort results"
          options={options}
          order={[]}
          value={sortBy}
          onChange={event => onChange(event.target.value as OptionType)}
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
          onChange={event => onChange(event.target.value as OptionType)}
        />
      )}

      <Button className="dfe-js-hidden govuk-!-margin-top-4" type="submit">
        Submit
      </Button>
    </form>
  );
};

export default SortControls;

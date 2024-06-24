import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@frontend/components/FilterResetButton.module.scss';
import React from 'react';

interface Props {
  filterType: string;
  name: string;
  onClick: () => void;
}

const FilterResetButton = ({ filterType, name, onClick }: Props) => {
  return (
    <button className={styles.button} type="button" onClick={onClick}>
      <span aria-hidden>✕</span>
      <div className="govuk-!-margin-left-1">
        <VisuallyHidden>Remove filter:</VisuallyHidden>
        {filterType && <span className={styles.filterType}>{filterType}</span>}
        {name}
      </div>
    </button>
  );
};

export default FilterResetButton;

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
    <button
      className={styles.button}
      type="button"
      onClick={onClick}
      data-testid="filter-reset"
    >
      <span>
        <VisuallyHidden>Remove filter:</VisuallyHidden>
        {filterType && <span className={styles.filterType}>{filterType}:</span>}
        {name}
      </span>
    </button>
  );
};

export default FilterResetButton;

import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@frontend/modules/find-statistics/components/FilterClearButton.module.scss';
import React from 'react';

interface Props {
  name: string;
  onClick: () => void;
}

const FilterClearButton = ({ name, onClick }: Props) => {
  return (
    <button className={styles.button} type="button" onClick={onClick}>
      <span aria-hidden>✕</span>
      <VisuallyHidden>Remove filter:</VisuallyHidden> {name}
    </button>
  );
};

export default FilterClearButton;

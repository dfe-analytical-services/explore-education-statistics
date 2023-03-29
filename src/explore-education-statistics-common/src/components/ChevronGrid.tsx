import styles from '@common/components/ChevronGrid.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  testId?: string;
}

export default function ChevronGrid({ children, testId }: Props) {
  return (
    <ul className={styles.grid} data-testid={testId}>
      {children}
    </ul>
  );
}

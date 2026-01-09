import styles from '@common/components/ChevronGrid.module.scss';
import React, { ReactNode, type JSX } from 'react';

interface Props {
  as?: keyof JSX.IntrinsicElements;
  children: ReactNode;
  testId?: string;
}

export default function ChevronGrid({
  as: Component = 'ul',
  children,
  testId,
}: Props) {
  return (
    <Component className={`govuk-grid-row ${styles.grid}`} data-testid={testId}>
      {children}
    </Component>
  );
}

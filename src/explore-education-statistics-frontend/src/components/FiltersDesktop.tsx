import styles from '@frontend/components/FiltersDesktop.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

export default function FiltersDesktop({ children }: Props) {
  return (
    <div className={`govuk-grid-column-one-third ${styles.desktopFilters}`}>
      {children}
    </div>
  );
}

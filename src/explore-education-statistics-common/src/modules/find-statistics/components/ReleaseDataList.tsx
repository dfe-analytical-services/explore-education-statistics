import styles from '@common/modules/find-statistics/components/ReleaseDataList.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  actions?: ReactNode;
  children: ReactNode;
  heading: string;
  id?: string;
  testId?: string;
}

export default function ReleaseDataList({
  actions,
  children,
  heading,
  id,
  testId,
}: Props) {
  return (
    <div className={styles.container} id={id} data-testid={testId}>
      <header
        className={styles.header}
        id={id}
        data-page-section
        data-testid={testId}
      >
        <h3 className={styles.heading}>{heading}</h3>
        {actions}
      </header>
      <ul className={styles.list}>{children}</ul>
    </div>
  );
}

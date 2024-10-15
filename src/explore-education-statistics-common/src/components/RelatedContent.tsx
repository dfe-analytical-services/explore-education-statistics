import styles from '@common/components/RelatedContent.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  testId?: string;
}

export default function RelatedContent({ children, testId }: Props) {
  return (
    <div className={styles.container} data-testid={testId}>
      {children}
    </div>
  );
}

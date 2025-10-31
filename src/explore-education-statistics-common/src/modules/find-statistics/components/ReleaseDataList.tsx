import { useMobileMedia } from '@common/hooks/useMedia';
import styles from '@common/modules/find-statistics/components/ReleaseDataList.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  actions?: ReactNode;
  children: ReactNode;
  heading: string;
  id?: string;
  testId?: string;
  toggle?: ReactNode;
}

export default function ReleaseDataList({
  actions,
  children,
  heading,
  id,
  testId,
  toggle,
}: Props) {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <article className={styles.container} id={id} data-testid={testId}>
      <header className={styles.header}>
        <div>
          <h3 className="govuk-!-margin-bottom-0">{heading}</h3>
          {!isMobileMedia && toggle}
        </div>
        {actions}
        {isMobileMedia && toggle}
      </header>
      <ul className={styles.list}>{children}</ul>
    </article>
  );
}

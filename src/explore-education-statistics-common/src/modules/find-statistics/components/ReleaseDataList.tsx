import { useDesktopMedia } from '@common/hooks/useMedia';
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
  const { isMedia: isDesktopMedia } = useDesktopMedia();

  return (
    <article className={styles.container} id={id} data-testid={testId}>
      <header className={styles.header}>
        <div>
          <h3 className="govuk-!-margin-bottom-0">{heading}</h3>
          {isDesktopMedia && toggle}
        </div>
        {actions}
        {!isDesktopMedia && toggle}
      </header>
      <ul className={styles.list}>{children}</ul>
    </article>
  );
}

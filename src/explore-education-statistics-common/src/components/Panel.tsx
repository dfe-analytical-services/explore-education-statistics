import styles from '@common/components/Panel.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  headingTag?: 'h1' | 'h2';
  title: string;
}

export default function Panel({
  children,
  headingTag: Heading = 'h1',
  title,
}: Props) {
  return (
    <div
      className={`govuk-panel govuk-panel--confirmation ${styles.panelContainer}`}
    >
      <Heading className="govuk-panel__title">{title}</Heading>
      <div className="govuk-panel__body">{children}</div>
    </div>
  );
}

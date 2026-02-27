import styles from '@common/modules/release/components/ReleaseDataPageCardLink.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  caption: string;
  renderLink: ReactNode;
}

export default function ReleaseDataPageCardLink({
  caption,
  renderLink,
}: Props) {
  return (
    <li className={styles.card}>
      <p className="govuk-heading-m">{renderLink}</p>
      <p>{caption}</p>
    </li>
  );
}

export function ReleaseDataPageCardLinkGrid({
  children,
}: {
  children: ReactNode;
}) {
  return (
    <ul className={styles.grid} data-testid="links-grid">
      {children}
    </ul>
  );
}

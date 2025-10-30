import styles from '@common/modules/release/components/ReleaseDataPageCardLink.module.scss';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  caption: string;
  isHighlightVariant?: boolean;
  renderLink: ReactNode;
}

export default function ReleaseDataPageCardLink({
  caption,
  isHighlightVariant = false,
  renderLink,
}: Props) {
  return (
    <li
      className={classNames(styles.card, {
        [styles.highlight]: isHighlightVariant,
      })}
    >
      <h3>{renderLink}</h3>
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

import styles from '@common/modules/find-statistics/components/ReleaseDataListItem.module.scss';
import React, { ReactNode } from 'react';

interface Props {
  actions?: ReactNode;
  children?: ReactNode;
  description?: string;
  metaInfo?: string;
  title: string;
}

export default function ReleaseDataListItem({
  actions,
  children,
  description,
  metaInfo,
  title,
}: Props) {
  return (
    <li className={styles.listItem} data-testid="release-data-list-item">
      <div className={styles.content}>
        <h4 className={styles.title}>{title}</h4>
        {metaInfo && <p className="govuk-!-margin-bottom-1">{metaInfo}</p>}
        {description && (
          <p className="dfe-colour--dark-grey govuk-!-margin-bottom-0">
            {description}
          </p>
        )}
        {children}
      </div>
      <div className={styles.actions}>{actions}</div>
    </li>
  );
}
